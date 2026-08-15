// NY ROLLER RUSH - CORE SYSTEM
// InfiniteRunner3D had no dedicated camera script. Standard runner follow extracted as a light chase cam.

using UnityEngine;

namespace NYRollerRush.Runner
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 6f, -8f);
        [SerializeField] float followSpeed = 10f;
        [SerializeField] float lookAhead = 6f;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
            transform.LookAt(target.position + Vector3.forward * lookAhead);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
