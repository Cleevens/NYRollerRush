// NY ROLLER RUSH - CORE SYSTEM
// Source: AwesomeRunner (VladimirPirozhenko) — pooled MonoBehaviour that can return itself.

using UnityEngine;

namespace NYRollerRush.Pooling
{
    public abstract class PoolingObject : MonoBehaviour, IPoolCallbackReceiver
    {
        public GameObjectPool OwningPool { get; set; }

        public virtual void OnRent() { }
        public virtual void OnReturn() { }

        public void ReturnToPool()
        {
            if (OwningPool != null)
                OwningPool.Return(gameObject);
            else
                SharedGameObjectPool.Return(gameObject);
        }
    }
}
