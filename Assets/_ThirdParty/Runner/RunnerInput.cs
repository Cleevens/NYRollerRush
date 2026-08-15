// NY ROLLER RUSH - CORE SYSTEM
// Source: InfiniteRunner3D (dgkanatsios) — swipe + arrow input, cleaned namespace.

using System;
using UnityEngine;

namespace NYRollerRush.Runner
{
    public enum InputDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public interface IRunnerInput
    {
        InputDirection? DetectInputDirection();
    }

    public class SwipeInput : MonoBehaviour, IRunnerInput
    {
        enum SwipeState
        {
            Idle,
            Started
        }

        SwipeState state = SwipeState.Idle;
        Vector2 startPoint;
        DateTime timeSwipeStarted;
        readonly TimeSpan maxSwipeDuration = TimeSpan.FromSeconds(1);
        readonly TimeSpan minSwipeDuration = TimeSpan.FromMilliseconds(80);

        public InputDirection? DetectInputDirection()
        {
            if (state == SwipeState.Idle)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    timeSwipeStarted = DateTime.Now;
                    state = SwipeState.Started;
                    startPoint = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var timeDifference = DateTime.Now - timeSwipeStarted;
                state = SwipeState.Idle;
                if (timeDifference > maxSwipeDuration || timeDifference < minSwipeDuration)
                    return null;

                Vector2 difference = (Vector2)Input.mousePosition - startPoint;
                float angle = Vector2.Angle(difference, Vector2.right);
                if (Vector3.Cross(difference, Vector2.right).z > 0)
                    angle = 360f - angle;

                if (angle <= 45f || angle >= 315f) return InputDirection.Right;
                if (angle <= 135f) return InputDirection.Top;
                if (angle <= 225f) return InputDirection.Left;
                return InputDirection.Bottom;
            }

            return null;
        }
    }

    public class ArrowKeyInput : MonoBehaviour, IRunnerInput
    {
        public InputDirection? DetectInputDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return InputDirection.Top;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return InputDirection.Bottom;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return InputDirection.Right;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return InputDirection.Left;
            return null;
        }
    }

    public class CompositeRunnerInput : MonoBehaviour, IRunnerInput
    {
        IRunnerInput[] detectors;

        void Awake()
        {
            detectors = GetComponents<IRunnerInput>();
        }

        public InputDirection? DetectInputDirection()
        {
            if (detectors == null) return null;
            for (int i = 0; i < detectors.Length; i++)
            {
                if (ReferenceEquals(detectors[i], this)) continue;
                var dir = detectors[i].DetectInputDirection();
                if (dir.HasValue) return dir;
            }
            return null;
        }
    }
}
