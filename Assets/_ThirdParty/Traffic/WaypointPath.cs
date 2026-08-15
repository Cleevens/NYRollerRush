// NY ROLLER RUSH - CORE SYSTEM
// Source: unity-traffic-simulation Segment — ordered waypoints a car follows.

using UnityEngine;

namespace NYRollerRush.Traffic
{
    public class WaypointPath : MonoBehaviour
    {
        public int id;
        public WaypointNode[] waypoints;
        public WaypointPath[] nextPaths;
        public float detectThreshold = 0.35f;

        public bool IsOnPath(Vector3 point)
        {
            if (waypoints == null || waypoints.Length < 2) return false;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] == null || waypoints[i + 1] == null) continue;
                float d1 = Vector3.Distance(waypoints[i].transform.position, point);
                float d2 = Vector3.Distance(waypoints[i + 1].transform.position, point);
                float d3 = Vector3.Distance(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                if (Mathf.Abs((d1 + d2) - d3) < detectThreshold)
                    return true;
            }
            return false;
        }

        public WaypointPath PickNext()
        {
            if (nextPaths == null || nextPaths.Length == 0) return this;
            return nextPaths[Random.Range(0, nextPaths.Length)];
        }
    }
}
