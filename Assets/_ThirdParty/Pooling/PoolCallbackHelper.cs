// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames)

using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Pooling
{
    internal static class PoolCallbackHelper
    {
        static readonly List<IPoolCallbackReceiver> componentsBuffer = new List<IPoolCallbackReceiver>();

        public static void InvokeOnRent(GameObject obj)
        {
            obj.GetComponentsInChildren(componentsBuffer);
            for (int i = 0; i < componentsBuffer.Count; i++)
                componentsBuffer[i].OnRent();
        }

        public static void InvokeOnReturn(GameObject obj)
        {
            obj.GetComponentsInChildren(componentsBuffer);
            for (int i = 0; i < componentsBuffer.Count; i++)
                componentsBuffer[i].OnReturn();
        }
    }
}
