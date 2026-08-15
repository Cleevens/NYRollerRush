// NY ROLLER RUSH - CORE SYSTEM
// Concurrent timed power-ups, rented from PoolHub, applied to SkateController.

using System.Collections.Generic;
using NYRollerRush.Pooling;
using NYRollerRush.Runner;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public interface IPowerUp
    {
        string Id { get; }
        float Duration { get; }
        void Activate(SkateController skate);
        void Deactivate(SkateController skate);
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        class ActiveEffect
        {
            public PowerUpKind Kind;
            public float Remaining;
            public int Multiplier = 2;
        }

        readonly List<ActiveEffect> effects = new List<ActiveEffect>();
        SkateController skate;

        public IReadOnlyList<string> HudLines { get; private set; } = new List<string>();

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
                return;

            skate = SkateController.Instance;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Remaining -= Time.deltaTime;
                if (effects[i].Remaining <= 0f)
                {
                    End(effects[i]);
                    effects.RemoveAt(i);
                }
            }

            Recompute();
        }

        public void Activate(PowerUpKind kind)
        {
            skate = SkateController.Instance;
            AudioManager.Instance?.Play(SfxId.PowerUp);
            if (kind == PowerUpKind.CompanionCall)
            {
                CompanionSkaterSpawner.Instance?.CallCompanions(Random.Range(1, 4), 12f);
                RefreshHud();
                return;
            }

            var existing = Find(kind);
            float duration = DurationOf(kind);
            if (existing != null)
            {
                existing.Remaining = duration;
                if (kind == PowerUpKind.ScoreMultiplier)
                    existing.Multiplier = Random.value < 0.25f ? 3 : 2;
            }
            else
            {
                var fx = new ActiveEffect { Kind = kind, Remaining = duration, Multiplier = Random.value < 0.25f ? 3 : 2 };
                effects.Add(fx);
            }

            Recompute();
        }

        public void ClearAll()
        {
            for (int i = 0; i < effects.Count; i++)
                End(effects[i]);
            effects.Clear();
            TrafficRules.Frozen = false;
            PickupRules.MagnetRadius = 0f;
            PickupRules.ScoreMultiplier = 1f;
            Recompute();
        }

        public GameObject Spawn(Vector3 position, PowerUpKind? kind = null)
        {
            if (PoolHub.Instance == null || !PoolHub.Instance.HasPool(PooledKind.PowerUp))
                return null;
            var go = PoolHub.Instance.Rent(PooledKind.PowerUp, position, Quaternion.identity);
            if (go == null) return null;
            var pickup = go.GetComponent<PowerUpPickup>();
            pickup?.SetKind(kind ?? RandomKind());
            return go;
        }

        public static float DurationOf(PowerUpKind kind)
        {
            switch (kind)
            {
                case PowerUpKind.Magnet: return 9f;
                case PowerUpKind.SpeedBoost: return 7f;
                case PowerUpKind.Shield: return 7f;
                case PowerUpKind.Strength: return 8f;
                case PowerUpKind.ScoreMultiplier: return 12f;
                case PowerUpKind.Ghost: return 4.5f;
                case PowerUpKind.TrafficFreeze: return 4.5f;
                default: return 0f;
            }
        }

        public static PowerUpKind RandomKind()
        {
            var values = (PowerUpKind[])System.Enum.GetValues(typeof(PowerUpKind));
            return values[Random.Range(0, values.Length)];
        }

        ActiveEffect Find(PowerUpKind kind)
        {
            for (int i = 0; i < effects.Count; i++)
                if (effects[i].Kind == kind)
                    return effects[i];
            return null;
        }

        void End(ActiveEffect fx)
        {
            if (fx.Kind == PowerUpKind.TrafficFreeze)
                TrafficRules.Frozen = false;
        }

        void Recompute()
        {
            if (skate == null)
                skate = SkateController.Instance;

            float speed = 1f, jump = 1f, lane = 1f, score = 1f, magnet = 0f;
            bool shield = false, ghost = false, freeze = false;
            int multi = 1;
            float eqSpeed = 0f, eqJump = 0f, eqHandle = 0f, eqMagnet = 0f;
            int eqArmor = 0;
            if (ShopManager.Instance != null)
                ShopManager.Instance.PeekEquippedBonuses(out eqSpeed, out eqJump, out eqHandle, out eqArmor, out eqMagnet);
            float hood = 1f;
            if (NeighborhoodManager.Instance != null && NeighborhoodManager.Instance.Current != null)
                hood = Mathf.Max(0.75f, NeighborhoodManager.Instance.Current.speedModifier);
            speed += eqSpeed;
            jump += eqJump;
            lane += eqHandle;
            magnet += eqMagnet;

            for (int i = 0; i < effects.Count; i++)
            {
                switch (effects[i].Kind)
                {
                    case PowerUpKind.Magnet: magnet += 7.5f; break;
                    case PowerUpKind.SpeedBoost: speed += 0.5f; break;
                    case PowerUpKind.Shield: shield = true; break;
                    case PowerUpKind.Strength:
                        lane += 0.55f;
                        jump += 0.4f;
                        break;
                    case PowerUpKind.ScoreMultiplier:
                        multi = Mathf.Max(multi, effects[i].Multiplier);
                        break;
                    case PowerUpKind.Ghost: ghost = true; break;
                    case PowerUpKind.TrafficFreeze: freeze = true; break;
                }
            }

            if (skate != null)
            {
                skate.SpeedMul = speed * hood;
                skate.JumpMul = jump;
                skate.LaneMul = lane;
                skate.ScoreMul = multi;
                skate.MagnetRadius = magnet;
                skate.Invulnerable = shield || ghost;
                skate.Ghost = ghost;
                if (eqArmor > skate.ArmorCharges && !Has(PowerUpKind.Shield))
                    skate.ArmorCharges = eqArmor;
            }

            score = multi;
            PickupRules.ScoreMultiplier = score;
            PickupRules.MagnetRadius = magnet;
            PickupRules.MagnetOrigin = () => skate != null ? skate.transform.position : Vector3.zero;
            TrafficRules.Frozen = freeze;
            RefreshHud();
        }

        bool Has(PowerUpKind kind) => Find(kind) != null;

        void RefreshHud()
        {
            var lines = new List<string>();
            for (int i = 0; i < effects.Count; i++)
                lines.Add(effects[i].Kind + "  " + effects[i].Remaining.ToString("0.0") + "s");
            if (CompanionSkaterSpawner.Instance != null && CompanionSkaterSpawner.Instance.ActiveCount > 0)
                lines.Add("Companions x" + CompanionSkaterSpawner.Instance.ActiveCount);
            HudLines = lines;
        }
    }
}
