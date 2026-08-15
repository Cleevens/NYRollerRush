// NY ROLLER RUSH - CORE SYSTEM

using UnityEngine;

namespace NYRollerRush.Core
{
    [CreateAssetMenu(menuName = "NY Roller Rush/Neighborhood", fileName = "Neighborhood")]
    public class NeighborhoodData : ScriptableObject
    {
        public string displayName;
        public string id;
        public int unlockScore;
        public float carDensity = 0.65f;
        public float pedestrianDensity = 0.35f;
        public float carSpeed = 8.5f;
        public float speedModifier = 1f;
        [TextArea] public string hazardNotes;
        public string musicPlaceholder;
        public Color ambientColor = new Color(0.55f, 0.62f, 0.72f);
        public Color fogColor = new Color(0.45f, 0.42f, 0.38f);
        public float fogDensity = 0.012f;
        public Color sunColor = new Color(1f, 0.96f, 0.88f);

        public static NeighborhoodData Create(string id, string name, int unlock, float density, float pedDensity, float carSpd, float speedMod, string hazards, string music, Color ambient, Color fog, float fogD, Color sun)
        {
            var data = CreateInstance<NeighborhoodData>();
            data.id = id;
            data.displayName = name;
            data.unlockScore = unlock;
            data.carDensity = density;
            data.pedestrianDensity = pedDensity;
            data.carSpeed = carSpd;
            data.speedModifier = speedMod;
            data.hazardNotes = hazards;
            data.musicPlaceholder = music;
            data.ambientColor = ambient;
            data.fogColor = fog;
            data.fogDensity = fogD;
            data.sunColor = sun;
            data.name = name;
            return data;
        }
    }
}
