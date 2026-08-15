// NY ROLLER RUSH - CORE SYSTEM
// Source: InfiniteRunner3D (dgkanatsios) — session/game-state + score. Renamed to avoid GameManager clash.

using UnityEngine;

namespace NYRollerRush.Runner
{
    public enum RunnerState
    {
        Start,
        Playing,
        Dead
    }

    public class RunnerSession : MonoBehaviour
    {
        public static RunnerSession Instance { get; private set; }

        public RunnerState State { get; private set; } = RunnerState.Start;
        public float Score { get; private set; }
        public bool CanSwipe { get; set; }

        public event System.Action<RunnerState> StateChanged;
        public event System.Action<float> ScoreChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetState(RunnerState state)
        {
            if (State == state) return;
            State = state;
            StateChanged?.Invoke(State);
        }

        public void AddScore(float amount)
        {
            Score += amount;
            ScoreChanged?.Invoke(Score);
        }

        public void ResetScore()
        {
            Score = 0f;
            ScoreChanged?.Invoke(Score);
        }

        public void Die()
        {
            SetState(RunnerState.Dead);
        }

        public void BeginRun()
        {
            ResetScore();
            SetState(RunnerState.Playing);
        }
    }
}
