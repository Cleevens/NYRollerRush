// NY ROLLER RUSH - CORE SYSTEM
// Hooks so Core can drive lights / recycle without a circular asmdef.

using System;
using UnityEngine;

namespace NYRollerRush.Traffic
{
    public static class TrafficRules
    {
        public static Func<Vector3, float, bool> ShouldCarStop;
        public static Func<float> PlayerZ;
        public static bool Frozen;

        public static bool CarMustStop(Vector3 position, float lookAhead)
        {
            if (Frozen) return true;
            return ShouldCarStop != null && ShouldCarStop(position, lookAhead);
        }

        public static float GetPlayerZ()
        {
            return PlayerZ != null ? PlayerZ() : 0f;
        }
    }
}
