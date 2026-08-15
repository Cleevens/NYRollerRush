// NY ROLLER RUSH - CORE SYSTEM
// Source: unity-traffic-simulation Waypoint + CityBuilder RoadPoint — light waypoint node.

using UnityEngine;

namespace NYRollerRush.Traffic
{
    public class WaypointNode : MonoBehaviour
    {
        public WaypointNode[] next;
        public bool isStopLine;

        void OnDrawGizmos()
        {
            Gizmos.color = isStopLine ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.35f);
            if (next == null) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < next.Length; i++)
            {
                if (next[i] != null)
                    Gizmos.DrawLine(transform.position, next[i].transform.position);
            }
        }
    }
}
