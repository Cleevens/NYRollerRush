// NY ROLLER RUSH - CORE SYSTEM
// Pooled AI skater that drafts the player, dodges cars, and shares magnet.

using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class CompanionSkater : PoolingObject
    {
        [SerializeField] float followLag = 0.18f;
        [SerializeField] float styleInterval = 1.4f;

        float laneOffset;
        float life;
        float styleTimer;
        SkateController player;

        public void Bind(float offsetX, float duration)
        {
            laneOffset = offsetX;
            life = duration;
            styleTimer = styleInterval;
            player = SkateController.Instance;
        }

        void Update()
        {
            if (player == null)
                player = SkateController.Instance;
            var gm = GameManager.Instance;
            if (gm != null && gm.State == GameState.Paused)
                return;
            if (player == null || gm == null || gm.State != GameState.Playing)
            {
                ReturnToPool();
                return;
            }

            life -= Time.deltaTime;
            if (life <= 0f)
            {
                ReturnToPool();
                return;
            }

            Vector3 target = player.transform.position + new Vector3(laneOffset, 0f, -1.6f);
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, 3.2f))
                target.x += laneOffset >= 0f ? 0.8f : -0.8f;

            transform.position = Vector3.Lerp(transform.position, target, 1f - followLag);
            transform.rotation = Quaternion.Slerp(transform.rotation, player.transform.rotation, 8f * Time.deltaTime);

            styleTimer -= Time.deltaTime;
            if (styleTimer <= 0f)
            {
                styleTimer = styleInterval;
                GameManager.Instance.AddScore(15f * Mathf.Max(1f, player.ScoreMul));
            }
        }

        public override void OnReturn()
        {
            player = null;
        }
    }
}
