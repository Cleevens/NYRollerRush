// NY ROLLER RUSH - CORE SYSTEM
// Source: unity-traffic-simulation Intersection + CityBuilder RoadPoint.Go/Stop.

using UnityEngine;

namespace NYRollerRush.Traffic
{
    public enum LightPhase
    {
        Green,
        Yellow,
        Red
    }

    public class TrafficLightCycle : MonoBehaviour
    {
        [SerializeField] float greenSeconds = 8f;
        [SerializeField] float yellowSeconds = 2f;
        [SerializeField] float redSeconds = 8f;
        [SerializeField] WaypointPath[] groupA;
        [SerializeField] WaypointPath[] groupB;
        [SerializeField] Renderer[] signalRenderers;

        public LightPhase Phase { get; private set; } = LightPhase.Green;
        public bool DrivenExternally;
        int activeGroup = 0;
        float timer;

        public void SetPhase(LightPhase phase)
        {
            Phase = phase;
            ApplyVisuals();
        }

        void Update()
        {
            if (DrivenExternally) return;
            timer += Time.deltaTime;
            float hold = Phase == LightPhase.Green ? greenSeconds : Phase == LightPhase.Yellow ? yellowSeconds : redSeconds;
            if (timer < hold) return;

            timer = 0f;
            if (Phase == LightPhase.Green)
                Phase = LightPhase.Yellow;
            else if (Phase == LightPhase.Yellow)
                Phase = LightPhase.Red;
            else
            {
                Phase = LightPhase.Green;
                activeGroup = 1 - activeGroup;
            }

            ApplyVisuals();
        }

        public bool IsRedFor(WaypointPath path)
        {
            if (path == null) return Phase == LightPhase.Red;
            var blocked = activeGroup == 0 ? groupA : groupB;
            if (blocked == null) return Phase != LightPhase.Green;
            if (Phase == LightPhase.Green) return false;

            for (int i = 0; i < blocked.Length; i++)
            {
                if (blocked[i] == path)
                    return true;
            }
            return false;
        }

        void ApplyVisuals()
        {
            if (signalRenderers == null) return;
            Color color = Phase == LightPhase.Green ? Color.green : Phase == LightPhase.Yellow ? Color.yellow : Color.red;
            for (int i = 0; i < signalRenderers.Length; i++)
            {
                if (signalRenderers[i] != null)
                    signalRenderers[i].material.color = color;
            }
        }
    }
}
