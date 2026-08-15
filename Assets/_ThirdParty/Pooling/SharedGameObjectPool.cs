// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames) — shared Rent/Return used instead of Instantiate/Destroy.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Pooling
{
    public static class SharedGameObjectPool
    {
        static readonly Dictionary<GameObject, Stack<GameObject>> pools = new Dictionary<GameObject, Stack<GameObject>>();
        static readonly Dictionary<GameObject, Stack<GameObject>> cloneReferences = new Dictionary<GameObject, Stack<GameObject>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            pools.Clear();
            cloneReferences.Clear();
        }

        public static GameObject Rent(GameObject original)
        {
            return Rent(original, original.transform.position, original.transform.rotation, null);
        }

        public static GameObject Rent(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var pool = GetOrCreatePool(original);
            GameObject obj = null;
            while (pool.Count > 0)
            {
                obj = pool.Pop();
                if (obj != null) break;
            }

            if (obj == null)
            {
                obj = parent == null
                    ? UnityEngine.Object.Instantiate(original, position, rotation)
                    : UnityEngine.Object.Instantiate(original, position, rotation, parent);
            }
            else
            {
                if (parent != null)
                    obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }

            cloneReferences[obj] = pool;
            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public static TComponent Rent<TComponent>(TComponent original) where TComponent : Component
        {
            return Rent(original.gameObject).GetComponent<TComponent>();
        }

        public static void Return(GameObject instance)
        {
            if (instance == null) return;
            if (!cloneReferences.TryGetValue(instance, out var pool))
            {
                instance.SetActive(false);
                return;
            }

            instance.SetActive(false);
            pool.Push(instance);
            cloneReferences.Remove(instance);
            PoolCallbackHelper.InvokeOnReturn(instance);
        }

        public static void Prewarm(GameObject original, int count)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            var pool = GetOrCreatePool(original);
            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);
                obj.SetActive(false);
                pool.Push(obj);
                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        static Stack<GameObject> GetOrCreatePool(GameObject original)
        {
            if (!pools.TryGetValue(original, out var pool))
            {
                pool = new Stack<GameObject>();
                pools.Add(original, pool);
            }
            return pool;
        }
    }
}
