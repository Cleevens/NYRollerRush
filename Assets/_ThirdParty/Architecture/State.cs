// NY ROLLER RUSH - CORE SYSTEM
// Source: AwesomeRunner (VladimirPirozhenko) — generic state for skate / game FSMs.

using UnityEngine;

namespace NYRollerRush.Architecture
{
    public abstract class State<T> where T : MonoBehaviour
    {
        public abstract void Tick();
        public virtual void FixedTick() { }
        public virtual void OnStateEnter() { }
        public virtual void OnStateExit() { }
    }
}
