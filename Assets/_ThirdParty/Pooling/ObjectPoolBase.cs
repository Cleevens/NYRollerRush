// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames) — generic stack pool used by cars, coins, chunks, pedestrians.

using System;
using System.Collections.Generic;

namespace NYRollerRush.Pooling
{
    public abstract class ObjectPoolBase<T> : IObjectPool<T> where T : class
    {
        protected readonly Stack<T> stack = new Stack<T>(32);
        bool isDisposed;

        protected abstract T CreateInstance();
        protected virtual void OnDestroy(T instance) { }
        protected virtual void OnRent(T instance) { }
        protected virtual void OnReturn(T instance) { }

        public T Rent()
        {
            ThrowIfDisposed();
            if (stack.Count > 0)
            {
                var obj = stack.Pop();
                OnRent(obj);
                if (obj is IPoolCallbackReceiver receiver)
                    receiver.OnRent();
                return obj;
            }

            return CreateInstance();
        }

        public void Return(T obj)
        {
            ThrowIfDisposed();
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            OnReturn(obj);
            if (obj is IPoolCallbackReceiver receiver)
                receiver.OnReturn();
            stack.Push(obj);
        }

        public void Clear()
        {
            ThrowIfDisposed();
            while (stack.Count > 0)
                OnDestroy(stack.Pop());
        }

        public void Prewarm(int count)
        {
            ThrowIfDisposed();
            for (int i = 0; i < count; i++)
                Return(CreateInstance());
        }

        public int Count => stack.Count;
        public bool IsDisposed => isDisposed;

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
