// NY ROLLER RUSH - CORE SYSTEM
// Drives Green → Yellow → Red and tells the skater when to brake.

using System.Collections.Generic;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class TrafficLightController : MonoBehaviour
    {
        static readonly List<TrafficLightController> Active = new List<TrafficLightController>();

        [SerializeField] TrafficLightCycle cycle;
        [SerializeField] RoadStopPoint stopPoint;
        [SerializeField] Renderer signalRenderer;
        [SerializeField] float greenSeconds = 7f;
        [SerializeField] float yellowSeconds = 1.6f;
        [SerializeField] float redSeconds = 5f;
        [SerializeField] float playerApproach = 14f;
        [SerializeField] float playerStopPad = 1.4f;

        float timer;
        LightPhase phase = LightPhase.Green;

        public LightPhase Phase => phase;
        public Vector3 StopPosition => stopPoint != null ? stopPoint.transform.position : transform.position;

        void Awake()
        {
            TrafficRules.ShouldCarStop = IsRedForCars;
        }

        void OnEnable()
        {
            if (!Active.Contains(this))
                Active.Add(this);
            if (cycle == null)
                cycle = GetComponent<TrafficLightCycle>();
            if (stopPoint == null)
                stopPoint = GetComponentInChildren<RoadStopPoint>();
            Apply();
        }

        void OnDisable()
        {
            Active.Remove(this);
        }

        void Update()
        {
            timer += Time.deltaTime;
            float hold = phase == LightPhase.Green ? greenSeconds : phase == LightPhase.Yellow ? yellowSeconds : redSeconds;
            if (timer < hold) return;

            timer = 0f;
            if (phase == LightPhase.Green) phase = LightPhase.Yellow;
            else if (phase == LightPhase.Yellow) phase = LightPhase.Red;
            else phase = LightPhase.Green;
            Apply();
        }

        void Apply()
        {
            if (cycle != null)
            {
                cycle.DrivenExternally = true;
                cycle.SetPhase(phase);
            }

            if (stopPoint != null)
                stopPoint.SetFromLight(phase);
            if (signalRenderer != null)
            {
                Color color = phase == LightPhase.Green ? Color.green : phase == LightPhase.Yellow ? new Color(1f, 0.75f, 0.1f) : Color.red;
                signalRenderer.material.color = color;
            }
        }

        public bool IsBlocking() => phase != LightPhase.Green;

        public static bool ShouldPlayerBrake(Vector3 playerPos)
        {
            for (int i = 0; i < Active.Count; i++)
            {
                var light = Active[i];
                if (light == null || !light.IsBlocking()) continue;
                float dz = light.StopPosition.z - playerPos.z;
                if (dz > 0f && dz < light.playerApproach && playerPos.z < light.StopPosition.z - light.playerStopPad)
                    return true;
            }

            return false;
        }

        public static bool IsRedForCars(Vector3 carPos, float lookAhead)
        {
            for (int i = 0; i < Active.Count; i++)
            {
                var light = Active[i];
                if (light == null || light.phase == LightPhase.Green) continue;
                float dz = light.StopPosition.z - carPos.z;
                if (dz > 0f && dz < lookAhead)
                    return true;
            }

            return false;
        }

        public void Configure(Renderer lamp, RoadStopPoint point, TrafficLightCycle ownedCycle)
        {
            signalRenderer = lamp;
            stopPoint = point;
            cycle = ownedCycle;
            if (cycle != null)
                cycle.DrivenExternally = true;
            Apply();
        }
    }
}
