// NY ROLLER RUSH - CORE SYSTEM
// JSON blob in PlayerPrefs for high score, wallet, unlocks, loadout.

using UnityEngine;

namespace NYRollerRush.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        const string Key = "nyrr.save";
        public SaveData Data { get; private set; } = new SaveData();

        void Awake()
        {
            Instance = this;
            Load();
        }

        void Start()
        {
            ApplyToManagers();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(Key))
            {
                Data = NewDefault();
                Save();
                return;
            }

            try
            {
                Data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key)) ?? NewDefault();
                if (Data.ownedItems == null) Data.ownedItems = new System.Collections.Generic.List<string>();
                if (Data.unlockedNeighborhoods == null) Data.unlockedNeighborhoods = new System.Collections.Generic.List<string>();
            }
            catch
            {
                Data = NewDefault();
            }
        }

        public void Save()
        {
            if (Data == null) Data = NewDefault();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public void CaptureFromManagers()
        {
            if (CurrencyManager.Instance != null)
            {
                Data.coins = CurrencyManager.Instance.Coins;
                Data.gems = CurrencyManager.Instance.Gems;
            }

            if (ShopManager.Instance != null)
                ShopManager.Instance.WriteToSave(Data);
            if (NeighborhoodManager.Instance != null)
                NeighborhoodManager.Instance.WriteToSave(Data);
            if (GameManager.Instance != null)
                Data.highScore = Mathf.Max(Data.highScore, Mathf.FloorToInt(GameManager.Instance.Score));
        }

        public void ApplyToManagers()
        {
            CurrencyManager.Instance?.Set(Data.coins, Data.gems);
            ShopManager.Instance?.ReadFromSave(Data);
            NeighborhoodManager.Instance?.ReadFromSave(Data);
        }

        static SaveData NewDefault()
        {
            return new SaveData
            {
                coins = 200,
                gems = 5,
                unlockedNeighborhoods = { "times_square" },
                ownedItems = { "skates_street", "outfit_tee", "avatar_default" },
                equippedSkates = "skates_street",
                equippedHelmet = "",
                equippedOutfit = "outfit_tee",
                equippedAvatar = "avatar_default",
                lastNeighborhood = "times_square"
            };
        }
    }
}
