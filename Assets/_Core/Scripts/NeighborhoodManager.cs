// NY ROLLER RUSH - CORE SYSTEM
// Five NYC neighborhoods: density, speed, unlock score, fog/light mood.
// Catalog: Resources/Neighborhoods/*.asset, else CatalogFactory.BuildNeighborhoods().

using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class NeighborhoodManager : MonoBehaviour
    {
        public static NeighborhoodManager Instance { get; private set; }

        public NeighborhoodData[] neighborhoods;
        public NeighborhoodData Current { get; private set; }
        readonly HashSet<string> unlocked = new HashSet<string>();

        void Awake()
        {
            Instance = this;
            if (neighborhoods == null || neighborhoods.Length == 0)
                neighborhoods = CatalogFactory.LoadNeighborhoods();
            unlocked.Add("times_square");
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Apply(NeighborhoodData data)
        {
            if (data == null) return;
            Current = data;
            TrafficManager.Instance?.ApplyNeighborhood(data);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = data.fogColor;
            RenderSettings.fogDensity = data.fogDensity;
            RenderSettings.ambientLight = data.ambientColor;
            var sun = Object.FindObjectOfType<Light>();
            if (sun != null && sun.type == LightType.Directional)
                sun.color = data.sunColor;
            AudioManager.Instance?.PlayMusicForNeighborhood(data.id);
        }

        public void ApplyStarting()
        {
            Apply(Current ?? FindById("times_square") ?? neighborhoods[0]);
        }

        public void TryUnlockFromScore(float score)
        {
            if (neighborhoods == null) return;
            for (int i = 0; i < neighborhoods.Length; i++)
            {
                var n = neighborhoods[i];
                if (n == null || unlocked.Contains(n.id) || score < n.unlockScore) continue;
                unlocked.Add(n.id);
            }
        }

        public bool IsUnlocked(string id) => unlocked.Contains(id);

        public bool TrySelect(string id)
        {
            var data = FindById(id);
            if (data == null || !IsUnlocked(id)) return false;
            Apply(data);
            return true;
        }

        public void SelectNextUnlocked()
        {
            if (neighborhoods == null || neighborhoods.Length == 0) return;
            int start = 0;
            for (int i = 0; i < neighborhoods.Length; i++)
            {
                if (Current != null && neighborhoods[i] != null && neighborhoods[i].id == Current.id)
                    start = i;
            }

            for (int n = 1; n <= neighborhoods.Length; n++)
            {
                var data = neighborhoods[(start + n) % neighborhoods.Length];
                if (data != null && IsUnlocked(data.id))
                {
                    Apply(data);
                    SaveSystem.Instance?.CaptureFromManagers();
                    SaveSystem.Instance?.Save();
                    return;
                }
            }
        }

        public void UnlockAllForDebug()
        {
            if (neighborhoods == null) return;
            for (int i = 0; i < neighborhoods.Length; i++)
                if (neighborhoods[i] != null)
                    unlocked.Add(neighborhoods[i].id);
        }

        public NeighborhoodData FindById(string id)
        {
            if (neighborhoods == null) return null;
            for (int i = 0; i < neighborhoods.Length; i++)
                if (neighborhoods[i] != null && neighborhoods[i].id == id)
                    return neighborhoods[i];
            return null;
        }

        public void WriteToSave(SaveData data)
        {
            data.lastNeighborhood = Current != null ? Current.id : "times_square";
            data.unlockedNeighborhoods = new List<string>(unlocked);
        }

        public void ReadFromSave(SaveData data)
        {
            unlocked.Clear();
            if (data.unlockedNeighborhoods != null)
            {
                for (int i = 0; i < data.unlockedNeighborhoods.Count; i++)
                    unlocked.Add(data.unlockedNeighborhoods[i]);
            }

            if (unlocked.Count == 0)
                unlocked.Add("times_square");
            Apply(FindById(data.lastNeighborhood) ?? FindById("times_square"));
        }

        public static NeighborhoodData[] BuildDefaults() => CatalogFactory.BuildNeighborhoods();
    }
}
