// NY ROLLER RUSH - CORE SYSTEM
// Source: AwesomeRunner (VladimirPirozhenko) — enter/exit/tick state machine.

using UnityEngine;

namespace NYRollerRush.Architecture
{
    public class StateMachine<T> where T : MonoBehaviour
    {
        public State<T> CurrentState { get; private set; }
        public State<T> PreviousState { get; private set; }

        public void SetState(State<T> newState)
        {
            if (CurrentState == newState || newState == null)
                return;

            if (CurrentState != null)
                CurrentState.OnStateExit();

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.OnStateEnter();
        }

        public void RevertState()
        {
            if (PreviousState != null)
                SetState(PreviousState);
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }

        public void FixedTick()
        {
            CurrentState?.FixedTick();
        }
    }
}
