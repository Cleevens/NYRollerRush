// NY ROLLER RUSH - CORE SYSTEM
// Source: CityBuilder InitialCharactersManager pattern — pooled sidewalk walker.

using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Traffic
{
    public class PooledPedestrian : PoolingObject
    {
        [SerializeField] float speed = 1.6f;
        [SerializeField] float recycleBehind = 16f;
        Vector3 walkDirection = Vector3.forward;

        public void SetWalk(Vector3 direction, float walkSpeed)
        {
            walkDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
            speed = walkSpeed;
        }

        void Update()
        {
            transform.position += walkDirection * speed * Time.deltaTime;
            if (transform.position.z < TrafficRules.GetPlayerZ() - recycleBehind)
                ReturnToPool();
        }
    }
}
