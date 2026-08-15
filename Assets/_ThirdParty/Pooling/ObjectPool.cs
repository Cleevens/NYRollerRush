// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames)

using System;

namespace NYRollerRush.Pooling
{
    public sealed class ObjectPool<T> : ObjectPoolBase<T> where T : class
    {
        readonly Func<T> createFunc;
        readonly Action<T> onRent;
        readonly Action<T> onReturn;
        readonly Action<T> onDestroy;

        public ObjectPool(Func<T> createFunc, Action<T> onRent = null, Action<T> onReturn = null, Action<T> onDestroy = null)
        {
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));
            this.createFunc = createFunc;
            this.onRent = onRent;
            this.onReturn = onReturn;
            this.onDestroy = onDestroy;
        }

        protected override T CreateInstance() => createFunc();
        protected override void OnDestroy(T instance) => onDestroy?.Invoke(instance);
        protected override void OnRent(T instance) => onRent?.Invoke(instance);
        protected override void OnReturn(T instance) => onReturn?.Invoke(instance);
    }
}
