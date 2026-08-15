// NY ROLLER RUSH - CORE SYSTEM
// Source: CityBuilder-and-Traffic-System RoadPoint — Go/Ready/Stop line used by lights.

using UnityEngine;

namespace NYRollerRush.Traffic
{
    public class RoadStopPoint : MonoBehaviour
    {
        public enum PointState
        {
            Go,
            Ready,
            Stop
        }

        public RoadStopPoint[] connected;
        public PointState roadState = PointState.Go;

        public void SetFromLight(LightPhase phase)
        {
            if (phase == LightPhase.Green) roadState = PointState.Go;
            else if (phase == LightPhase.Yellow) roadState = PointState.Ready;
            else roadState = PointState.Stop;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = roadState == PointState.Go ? Color.green : roadState == PointState.Ready ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
