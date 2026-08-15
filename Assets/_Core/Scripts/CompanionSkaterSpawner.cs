// NY ROLLER RUSH - CORE SYSTEM
// Spawns 1–3 pooled companion skaters from score thresholds or Companion Call.

using System.Collections.Generic;
using NYRollerRush.Pooling;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class CompanionSkaterSpawner : MonoBehaviour
    {
        public static CompanionSkaterSpawner Instance { get; private set; }

        [SerializeField] float defaultDuration = 12f;
        [SerializeField] float[] scoreThresholds = { 1800f, 4500f, 8500f };

        readonly List<CompanionSkater> live = new List<CompanionSkater>();
        readonly HashSet<int> usedThresholds = new HashSet<int>();
        readonly float[] offsets = { -1.7f, 1.7f, -3.2f };

        public int ActiveCount
        {
            get
            {
                live.RemoveAll(c => c == null || !c.gameObject.activeInHierarchy);
                return live.Count;
            }
        }

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
            float score = GameManager.Instance.Score;
            for (int i = 0; i < scoreThresholds.Length; i++)
            {
                if (usedThresholds.Contains(i) || score < scoreThresholds[i]) continue;
                usedThresholds.Add(i);
                CallCompanions(Mathf.Min(3, i + 1), defaultDuration);
            }
        }

        public void CallCompanions(int count, float duration)
        {
            count = Mathf.Clamp(count, 1, 3);
            var skate = SkateController.Instance;
            if (skate == null || PoolHub.Instance == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = skate.transform.position + new Vector3(offsets[i % offsets.Length], 0f, -1.4f);
                var go = PoolHub.Instance.Rent(PooledKind.Companion, pos, skate.transform.rotation);
                if (go == null) continue;
                var ai = go.GetComponent<CompanionSkater>();
                if (ai == null)
                    ai = go.AddComponent<CompanionSkater>();
                ai.Bind(offsets[i % offsets.Length], duration);
                live.Add(ai);
            }
        }

        public void DespawnAll()
        {
            for (int i = 0; i < live.Count; i++)
            {
                if (live[i] != null)
                    live[i].ReturnToPool();
            }

            live.Clear();
            usedThresholds.Clear();
        }
    }
}
