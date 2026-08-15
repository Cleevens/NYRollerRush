// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames) — GameObject pool for runner pickups, cars, chunks.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Pooling
{
    public sealed class GameObjectPool : IObjectPool<GameObject>
    {
        readonly GameObject original;
        readonly Stack<GameObject> stack = new Stack<GameObject>(32);
        readonly HashSet<GameObject> rented = new HashSet<GameObject>();
        bool isDisposed;

        public GameObjectPool(GameObject original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            this.original = original;
        }

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

        public GameObject Rent()
        {
            ThrowIfDisposed();
            GameObject obj;
            if (stack.Count == 0)
                obj = UnityEngine.Object.Instantiate(original);
            else
            {
                obj = stack.Pop();
                if (obj == null)
                    obj = UnityEngine.Object.Instantiate(original);
            }

            obj.SetActive(true);
            rented.Add(obj);
            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public GameObject Rent(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            ThrowIfDisposed();
            GameObject obj;
            if (stack.Count == 0)
            {
                obj = parent == null
                    ? UnityEngine.Object.Instantiate(original, position, rotation)
                    : UnityEngine.Object.Instantiate(original, position, rotation, parent);
            }
            else
            {
                obj = stack.Pop();
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
                }
            }

            obj.SetActive(true);
            rented.Add(obj);
            PoolCallbackHelper.InvokeOnRent(obj);
            return obj;
        }

        public void Return(GameObject obj)
        {
            ThrowIfDisposed();
            if (obj == null) return;
            if (!rented.Remove(obj))
                return;
            obj.SetActive(false);
            stack.Push(obj);
            PoolCallbackHelper.InvokeOnReturn(obj);
        }

        public void ReturnRented()
        {
            ThrowIfDisposed();
            if (rented.Count == 0) return;
            var live = new List<GameObject>(rented);
            for (int i = 0; i < live.Count; i++)
                Return(live[i]);
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
            {
                var obj = UnityEngine.Object.Instantiate(original);
                obj.SetActive(false);
                stack.Push(obj);
                PoolCallbackHelper.InvokeOnReturn(obj);
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            while (stack.Count > 0)
            {
                var obj = stack.Pop();
                if (obj != null)
                    UnityEngine.Object.Destroy(obj);
            }
        }

        public void Dispose()
        {
            ThrowIfDisposed();
            Clear();
            isDisposed = true;
        }

        void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().Name);
        }
    }
}
