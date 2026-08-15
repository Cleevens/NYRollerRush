// NY ROLLER RUSH - CORE SYSTEM
// Pooled world pickup. Collect → PowerUpManager → timed effect → return.

using NYRollerRush.Pooling;
using NYRollerRush.Traffic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class PowerUpPickup : PoolingObject
    {
        [SerializeField] PowerUpKind kind = PowerUpKind.Magnet;
        [SerializeField] float recycleBehind = 16f;
        [SerializeField] Renderer gem;

        public PowerUpKind Kind => kind;

        public void SetKind(PowerUpKind value)
        {
            kind = value;
            ApplyColor();
        }

        public override void OnRent()
        {
            if (gem == null)
                gem = GetComponentInChildren<Renderer>();
            ApplyColor();
        }

        void Update()
        {
            transform.Rotate(Vector3.up, 140f * Time.deltaTime);
            if (transform.position.z < TrafficRules.GetPlayerZ() - recycleBehind)
                ReturnToPool();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other == null || !other.CompareTag("Player")) return;
            PowerUpManager.Instance?.Activate(kind);
            ReturnToPool();
        }

        void ApplyColor()
        {
            if (gem == null) return;
            Color color;
            switch (kind)
            {
                case PowerUpKind.Magnet: color = new Color(1f, 0.55f, 0.1f); break;
                case PowerUpKind.SpeedBoost: color = new Color(0.2f, 0.85f, 1f); break;
                case PowerUpKind.Shield: color = new Color(0.35f, 0.7f, 1f); break;
                case PowerUpKind.Strength: color = new Color(0.95f, 0.25f, 0.2f); break;
                case PowerUpKind.ScoreMultiplier: color = new Color(1f, 0.85f, 0.15f); break;
                case PowerUpKind.Ghost: color = new Color(0.75f, 0.75f, 0.95f, 0.6f); break;
                case PowerUpKind.CompanionCall: color = new Color(0.2f, 0.95f, 0.45f); break;
                default: color = new Color(0.6f, 0.35f, 1f); break;
            }

            if (gem.material.HasProperty("_BaseColor"))
                gem.material.SetColor("_BaseColor", color);
            if (gem.material.HasProperty("_Color"))
                gem.material.color = color;
        }
    }
}
