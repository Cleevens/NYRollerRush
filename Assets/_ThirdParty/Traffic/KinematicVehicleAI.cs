// NY ROLLER RUSH - CORE SYSTEM
// Source: unity-traffic-simulation VehicleAI + CityBuilder Vehicle.
// Kinematic (no WheelCollider) so traffic stays cheap on mobile endless-runner streets.

using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Traffic
{
    public enum VehicleStatus
    {
        Go,
        SlowDown,
        Stop
    }

    public class KinematicVehicleAI : PoolingObject
    {
        [SerializeField] float speed = 8f;
        [SerializeField] float turnSpeed = 120f;
        [SerializeField] float waypointReach = 1.5f;
        [SerializeField] float recycleBehind = 24f;
        [SerializeField] float lightLookAhead = 10f;

        WaypointPath path;
        int waypointIndex;
        VehicleStatus status = VehicleStatus.Go;

        public void SetSpeed(float value) => speed = Mathf.Max(0.5f, value);
        public void SetLightLookAhead(float value) => lightLookAhead = value;

        public void BindPath(WaypointPath startPath, int startIndex = 0)
        {
            path = startPath;
            waypointIndex = startIndex;
            status = VehicleStatus.Go;
        }

        void Update()
        {
            EvaluateLights();
            if (transform.position.z < TrafficRules.GetPlayerZ() - recycleBehind)
            {
                ReturnToPool();
                return;
            }

            if (status == VehicleStatus.Stop)
                return;

            float step = speed * (status == VehicleStatus.SlowDown ? 0.35f : 1f) * Time.deltaTime;
            if (path == null || path.waypoints == null || path.waypoints.Length == 0)
            {
                transform.position += Vector3.forward * step;
                return;
            }

            var node = path.waypoints[Mathf.Clamp(waypointIndex, 0, path.waypoints.Length - 1)];
            if (node == null) return;

            Vector3 target = node.transform.position;
            target.y = transform.position.y;
            Vector3 to = target - transform.position;
            if (to.sqrMagnitude < waypointReach * waypointReach)
            {
                AdvanceWaypoint();
                return;
            }

            if (to.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            transform.position += transform.forward * step;
        }

        void EvaluateLights()
        {
            status = VehicleStatus.Go;
            if (TrafficRules.CarMustStop(transform.position, lightLookAhead))
            {
                status = VehicleStatus.Stop;
                return;
            }

            var net = TrafficNetwork.Instance;
            if (net == null || net.lights == null) return;
            for (int i = 0; i < net.lights.Length; i++)
            {
                var light = net.lights[i];
                if (light != null && light.IsRedFor(path))
                {
                    status = VehicleStatus.Stop;
                    return;
                }
            }
        }

        void AdvanceWaypoint()
        {
            waypointIndex++;
            if (waypointIndex < path.waypoints.Length) return;
            path = path.PickNext();
            waypointIndex = 0;
        }

        public override void OnRent()
        {
            status = VehicleStatus.Go;
        }
    }
}
