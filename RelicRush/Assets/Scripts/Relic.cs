﻿using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Relic : MonoBehaviour
    {
        public ParticleSystem CollisionParticleSystemPrefab;

        [Range(0, 2000)]
        public float EjectionSpeed;

        [Range(0, 500)]
        public float RotationMax;

        [Range(0, 1)]
        public float CameraShakeFactor;

        [Range(0, 10)]
        public float MinCollisionSpeed;

        [Range(0, 10)]
        public float SpawnPickupDelay;

        public string RelicName;

        private CameraShaker cameraShaker;
        private new Rigidbody rigidbody;

        public RelicSpawner Spawner { get; set; }
    
        public bool CanPickUp { get; set; }

        public bool EnableRespawning;

        // Use this for initialization
        private void Awake()
        {
            cameraShaker = CameraShaker.Get();
            rigidbody = GetComponent<Rigidbody>();
        }

        public void Start()
        {
            if(EnableRespawning)
                StartCoroutine(NotPickedUpAfter10Seconds());
        }

        public IEnumerator NotPickedUpAfter10Seconds()
        {
            yield return new WaitForSeconds(10);
            Spawner.RemoveAndSpawnNewRelic();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("DropPoint"))
            {
                RandomImpulse();
                return;
            }

            CollisionAnythingElse(collision);
        }

        public void OnTriggerWarpZone(Collider collider)
        {
            Transform offset = collider.gameObject.GetComponent<teleportZoneScript> ().offset;
            Rigidbody rigid = GetComponent<Rigidbody> ();

            Vector3 tempRigidPos = rigid.position;
            tempRigidPos.y += offset.localPosition.y;
            tempRigidPos.x += offset.localPosition.x;
            rigid.position = tempRigidPos;
        }

        private void CollisionAnythingElse(Collision collision)
        {
            var collisionMagnitude = collision.relativeVelocity.magnitude;
            if (collisionMagnitude < MinCollisionSpeed)
                return;

            cameraShaker.TicScreen(collisionMagnitude * CameraShakeFactor);

            var collisionPoint = collision.contacts.First().point;
            var particlePoint = new Vector3(collisionPoint.x, collisionPoint.y, 0);

            var particles = (GameObject)Instantiate(CollisionParticleSystemPrefab.gameObject, particlePoint, Quaternion.identity);
            Destroy(particles, 8);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("SpearTrap"))
            {
                RandomImpulse();
                return;
            }

            if (other.gameObject.CompareTag("KillZone"))
                EnterKillZone();

            if(other.gameObject.CompareTag("WarpZone"))
                OnTriggerWarpZone (other);
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("SpearTrap") == rigidbody.velocity.y < 0.1)
                RandomImpulse();
        }

        private void EnterKillZone()
        {
            Spawner.RemoveAndSpawnNewRelic();
        }

        public void RandomImpulse()
        {
            const float pi = Mathf.PI;

            var angle1 = Random.Range(pi, pi + (pi / 3));
            var angle2 = Random.Range(pi + (pi / 3), pi * 2);

            var angle = Random.Range(0, 2) == 0 ? angle1 : angle2;

            var x = Mathf.Cos(angle);
            var y = -Mathf.Sin(angle);

            var force = new Vector2(x, y) * EjectionSpeed;

            rigidbody.AddForce(force);
            rigidbody.AddTorque(new Vector3(0, 0, Random.Range(RotationMax / 2, RotationMax)));
        }

        public HoldingRelic BeHeldBy(RelicPlayer player)
        {
            Spawner.DespawnRelic();

            var holdingRelic = Instantiate(Spawner.HoldingRelicPrefab);
            holdingRelic.transform.SetParent(player.gameObject.transform, false);

            holdingRelic.Spawner = Spawner;

            return holdingRelic;
        }

        public void DelayBeingAbleToBePickedUp()
        {
            StartCoroutine(DelayBeingAbleToBePickedUpCouroutine());
        }

        private IEnumerator DelayBeingAbleToBePickedUpCouroutine()
        {
            CanPickUp = false;
        
            yield return new WaitForSeconds(SpawnPickupDelay);

            CanPickUp = true;
        }
    }
}
