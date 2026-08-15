// NY ROLLER RUSH - CORE SYSTEM
// Hooks so coins can magnetize and be collected by companions without a Core reference.

using System;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public static class PickupRules
    {
        public static Func<Vector3> MagnetOrigin;
        public static float MagnetRadius;
        public static Func<Collider, bool> CanCollect;
        public static float ScoreMultiplier = 1f;

        public static bool IsCollector(Collider other)
        {
            if (CanCollect != null) return CanCollect(other);
            return other != null && other.CompareTag("Player");
        }
    }
}
