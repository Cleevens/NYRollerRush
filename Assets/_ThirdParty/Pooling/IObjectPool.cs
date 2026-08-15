// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames) — cleaned, namespaced, no Addressables/UniTask.

using System;

namespace NYRollerRush.Pooling
{
    public interface IObjectPool<T> : IDisposable
    {
        T Rent();
        void Return(T obj);
    }
}
