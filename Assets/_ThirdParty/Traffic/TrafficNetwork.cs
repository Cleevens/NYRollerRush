// NY ROLLER RUSH - CORE SYSTEM
// Source: unity-traffic-simulation TrafficSystem — registry of paths and lights.

using UnityEngine;

namespace NYRollerRush.Traffic
{
    public class TrafficNetwork : MonoBehaviour
    {
        public static TrafficNetwork Instance { get; private set; }

        public WaypointPath[] paths;
        public TrafficLightCycle[] lights;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public WaypointPath FindPathNear(Vector3 position)
        {
            if (paths == null) return null;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] != null && paths[i].IsOnPath(position))
                    return paths[i];
            }
            return paths.Length > 0 ? paths[0] : null;
        }
    }
}
