// NY ROLLER RUSH - CORE SYSTEM
// Source: InfiniteRunner3D Candy — pooled coin with magnet pull.

using NYRollerRush.Pooling;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public class CollectibleCoin : PoolingObject
    {
        [SerializeField] int scorePoints = 100;
        [SerializeField] float rotateSpeed = 90f;
        [SerializeField] float recycleBehind = 16f;
        [SerializeField] float magnetSpeed = 18f;

        public static event System.Action<int> Collected;

        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            if (transform.position.z < TrafficRules.GetPlayerZ() - recycleBehind)
            {
                ReturnToPool();
                return;
            }

            if (PickupRules.MagnetRadius > 0.1f && PickupRules.MagnetOrigin != null)
            {
                Vector3 origin = PickupRules.MagnetOrigin();
                if ((transform.position - origin).sqrMagnitude <= PickupRules.MagnetRadius * PickupRules.MagnetRadius)
                    transform.position = Vector3.MoveTowards(transform.position, origin, magnetSpeed * Time.deltaTime);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!PickupRules.IsCollector(other)) return;
            int awarded = Mathf.RoundToInt(scorePoints * Mathf.Max(1f, PickupRules.ScoreMultiplier));
            Collected?.Invoke(awarded);
            ReturnToPool();
        }

        public override void OnReturn()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
