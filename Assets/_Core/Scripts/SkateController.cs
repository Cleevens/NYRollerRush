// NY ROLLER RUSH - CORE SYSTEM
// Roller-skate motor: momentum, 3–5 lanes, jump/slide, red-light brake.

using System;
using NYRollerRush.Runner;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class SkateController : MonoBehaviour
    {
        public static SkateController Instance { get; private set; }

        public event Action NearMiss;
        public event Action HitCar;
        public event Action<int> CoinCollected;

        [Header("Lanes")]
        [SerializeField] int laneCount = 3;
        [SerializeField] float laneWidth = 2f;
        [SerializeField] int startLane = 1;

        [Header("Speed / momentum")]
        [SerializeField] float baseSpeed = 9.4f;
        [SerializeField] float maxSpeed = 17.5f;
        [SerializeField] float acceleration = 1.15f;
        [SerializeField] AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] float secondsToMaxSpeed = 55f;
        [SerializeField] float brakeDeceleration = 32f;
        [SerializeField] float releaseDeceleration = 6f;

        [Header("Lane change")]
        [SerializeField] float laneChangeDuration = 0.26f;

        [Header("Jump")]
        [SerializeField] float jumpForce = 7.2f;
        [SerializeField] float jumpHoldForce = 14f;
        [SerializeField] float jumpHoldTime = 0.16f;
        [SerializeField] float gravity = 34f;

        [Header("Slide")]
        [SerializeField] float slideDuration = 0.65f;
        [SerializeField] float slideHeightScale = 0.45f;
        [SerializeField] float slideSpeedBonus = 1.4f;

        [Header("Feel")]
        [SerializeField] float groundedRay = 0.25f;
        [SerializeField] float fallKillY = -8f;
        [SerializeField] float nearMissDistance = 1.85f;
        [SerializeField] LayerMask groundMask = ~0;

        [Header("Runtime modifiers")]
        public float SpeedMul = 1f;
        public float JumpMul = 1f;
        public float LaneMul = 1f;
        public float ScoreMul = 1f;
        public float MagnetRadius;
        public bool Invulnerable;
        public bool Ghost;
        public int ArmorCharges;

        CharacterController controller;
        IRunnerInput runnerInput;
        int laneIndex;
        float laneFromX;
        float laneToX;
        float laneT = 1f;
        float verticalVelocity;
        float jumpHeld;
        float slideTimer;
        float defaultHeight;
        float defaultCenterY;
        float currentSpeed;
        float runTime;
        bool braking;
        bool jumpPressed;
        bool jumpHeldThisFrame;
        bool slideHeld;
        int lastNearMissId;
        bool wasGrounded = true;

        public bool IsGrounded { get; private set; }
        public bool IsSliding => slideTimer > 0f;
        public bool IsBraking => braking;
        public float CurrentSpeed => currentSpeed;
        public int LaneIndex => laneIndex;
        public float Distance { get; private set; }

        void Awake()
        {
            Instance = this;
            controller = GetComponent<CharacterController>();
            runnerInput = GetComponent<CompositeRunnerInput>();
            if (runnerInput == null)
                runnerInput = GetComponent<IRunnerInput>();

            defaultHeight = controller.height;
            defaultCenterY = controller.center.y;
            laneCount = Mathf.Clamp(laneCount, 3, 5);
            startLane = Mathf.Clamp(startLane, 0, laneCount - 1);
            laneIndex = startLane;
            laneToX = laneFromX = LaneX(laneIndex);
            currentSpeed = 0f;
        }

        void Start()
        {
            if (GameManager.Instance == null)
                currentSpeed = baseSpeed;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.State != GameState.Playing)
                return;

            ReadInput();
            UpdateSpeed();
            UpdateLane();
            UpdateJumpAndSlide();
            ApplyMove();
            CheckFall();
            Distance += currentSpeed * Time.deltaTime;
        }

        void ReadInput()
        {
            jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            jumpHeldThisFrame = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            slideHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl);
            bool manualBrake = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                TryChangeLane(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                TryChangeLane(1);

            var swipe = runnerInput != null ? runnerInput.DetectInputDirection() : null;
            if (swipe == InputDirection.Left) TryChangeLane(-1);
            else if (swipe == InputDirection.Right) TryChangeLane(1);
            else if (swipe == InputDirection.Top) jumpPressed = true;
            else if (swipe == InputDirection.Bottom) slideHeld = true;

            SetBraking(manualBrake || TrafficLightController.ShouldPlayerBrake(transform.position));
        }

        void UpdateSpeed()
        {
            runTime += Time.deltaTime;
            float curveT = secondsToMaxSpeed <= 0.01f ? 1f : Mathf.Clamp01(runTime / secondsToMaxSpeed);
            float target = Mathf.Lerp(baseSpeed, maxSpeed, accelerationCurve.Evaluate(curveT)) * Mathf.Max(0.4f, SpeedMul);
            if (IsSliding)
                target += slideSpeedBonus;

            if (braking)
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeDeceleration * Time.deltaTime);
            else if (currentSpeed < target)
                currentSpeed = Mathf.MoveTowards(currentSpeed, target, acceleration * (1.2f + currentSpeed * 0.08f) * Time.deltaTime);
            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, target, releaseDeceleration * Time.deltaTime);
        }

        void UpdateLane()
        {
            if (laneT >= 1f) return;
            laneT = Mathf.Clamp01(laneT + Time.deltaTime / Mathf.Max(0.05f, laneChangeDuration / Mathf.Max(0.4f, LaneMul)));
        }

        void UpdateJumpAndSlide()
        {
            IsGrounded = controller.isGrounded || Physics.SphereCast(
                transform.position + Vector3.up * 0.2f, 0.18f, Vector3.down,
                out _, groundedRay, groundMask, QueryTriggerInteraction.Ignore);

            if (jumpPressed && IsGrounded && !IsSliding)
            {
                verticalVelocity = jumpForce * Mathf.Max(0.6f, JumpMul);
                jumpHeld = jumpHoldTime;
                IsGrounded = false;
                AudioManager.Instance?.Play(SfxId.Jump);
            }
            else if (!IsGrounded)
            {
                if (jumpHeld > 0f && jumpHeldThisFrame && verticalVelocity > 0f)
                {
                    verticalVelocity += jumpHoldForce * Time.deltaTime;
                    jumpHeld -= Time.deltaTime;
                }
                else
                    jumpHeld = 0f;

                verticalVelocity -= gravity * Time.deltaTime;
            }
            else if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (slideHeld && IsGrounded && !IsSliding)
                BeginSlide();

            if (IsSliding)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f)
                    EndSlide();
            }

            if (!wasGrounded && IsGrounded)
                AudioManager.Instance?.Play(SfxId.Land);
            wasGrounded = IsGrounded;
        }

        void ApplyMove()
        {
            float x = Mathf.LerpUnclamped(laneFromX, laneToX, EaseOutBack(laneT));
            Vector3 motion;
            motion.x = x - transform.position.x;
            motion.y = verticalVelocity * Time.deltaTime;
            motion.z = currentSpeed * Time.deltaTime;
            controller.Move(motion);
        }

        void CheckFall()
        {
            if (transform.position.y < fallKillY)
                GameManager.Instance?.GameOver();
        }

        public void TryChangeLane(int delta)
        {
            if (laneT < 0.85f) return;
            int next = Mathf.Clamp(laneIndex + delta, 0, laneCount - 1);
            if (next == laneIndex) return;
            laneFromX = transform.position.x;
            laneIndex = next;
            laneToX = LaneX(laneIndex);
            laneT = 0f;
            AudioManager.Instance?.Play(SfxId.LaneChange);
        }

        public void SetBraking(bool value) => braking = value;

        public void NotifyCoinCollected(int points)
        {
            CoinCollected?.Invoke(points);
            AudioManager.Instance?.Play(SfxId.Coin);
            GameManager.Instance?.AddCoinScore(points);
        }

        public void NotifyNearMiss(int otherId)
        {
            if (otherId == lastNearMissId) return;
            lastNearMissId = otherId;
            NearMiss?.Invoke();
            AudioManager.Instance?.Play(SfxId.NearMiss);
            GameManager.Instance?.AddNearMissScore();
        }

        public void NotifyHitCar()
        {
            if (Ghost || Invulnerable)
                return;
            if (ArmorCharges > 0)
            {
                ArmorCharges--;
                return;
            }

            HitCar?.Invoke();
            GameManager.Instance?.GameOver();
        }

        public void ResetRun(Vector3 position)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            controller.enabled = true;
            laneIndex = Mathf.Clamp(startLane, 0, laneCount - 1);
            laneFromX = laneToX = LaneX(laneIndex);
            laneT = 1f;
            verticalVelocity = 0f;
            currentSpeed = 0f;
            runTime = 0f;
            Distance = 0f;
            braking = false;
            wasGrounded = true;
            SpeedMul = JumpMul = LaneMul = ScoreMul = 1f;
            MagnetRadius = 0f;
            Invulnerable = Ghost = false;
            ArmorCharges = 0;
            EndSlide();
        }

        public void ApplyLoadout(float speedBonus, float jumpBonus, float handlingBonus, int armor)
        {
            SpeedMul = 1f + speedBonus;
            JumpMul = 1f + jumpBonus;
            LaneMul = 1f + handlingBonus;
            ArmorCharges = Mathf.Max(0, armor);
        }

        float LaneX(int index)
        {
            float center = (laneCount - 1) * 0.5f;
            return (index - center) * laneWidth;
        }

        void BeginSlide()
        {
            slideTimer = slideDuration;
            controller.height = defaultHeight * slideHeightScale;
            var c = controller.center;
            c.y = defaultCenterY * slideHeightScale;
            controller.center = c;
        }

        void EndSlide()
        {
            slideTimer = 0f;
            controller.height = defaultHeight;
            var c = controller.center;
            c.y = defaultCenterY;
            controller.center = c;
        }

        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("WidePathBorder"))
            {
                laneFromX = laneToX = transform.position.x;
                laneT = 1f;
                return;
            }

            if (hit.gameObject.CompareTag("Car") || hit.gameObject.CompareTag("Pedestrian"))
                NotifyHitCar();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Car") || other.CompareTag("Pedestrian"))
            {
                float lateral = Mathf.Abs(other.transform.position.x - transform.position.x);
                if (lateral > nearMissDistance * 0.55f)
                    NotifyNearMiss(other.GetInstanceID());
                else
                    NotifyHitCar();
            }
        }
    }
}
