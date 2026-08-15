// NY ROLLER RUSH - CORE SYSTEM
// Avatars, clothing, helmets/pads, skates. Equip bonuses hit SkateController.
// Catalog: Resources/ShopItems/*.asset, else CatalogFactory.BuildShopItems().

using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public ShopItemData[] catalog;
        readonly HashSet<string> owned = new HashSet<string>();
        public string EquippedSkates { get; private set; } = "skates_street";
        public string EquippedHelmet { get; private set; }
        public string EquippedOutfit { get; private set; } = "outfit_tee";
        public string EquippedAvatar { get; private set; } = "avatar_default";

        void Awake()
        {
            Instance = this;
            if (catalog == null || catalog.Length == 0)
                catalog = CatalogFactory.LoadShopItems();
            EnsureStarters();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool Owns(string id) => owned.Contains(id);

        public bool IsEquipped(string id)
        {
            return id == EquippedSkates || id == EquippedHelmet || id == EquippedOutfit || id == EquippedAvatar;
        }

        public List<ShopItemData> ItemsIn(ShopCategory category)
        {
            var list = new List<ShopItemData>();
            if (catalog == null) return list;
            for (int i = 0; i < catalog.Length; i++)
            {
                var item = catalog[i];
                if (item == null) continue;
                bool match = item.category == category
                    || (category == ShopCategory.Helmet && item.category == ShopCategory.Pads);
                if (match)
                    list.Add(item);
            }

            return list;
        }

        public bool TryBuy(string id)
        {
            var item = Find(id);
            if (item == null || owned.Contains(id)) return false;
            if (CurrencyManager.Instance == null) return false;
            if (!CurrencyManager.Instance.TrySpend(item.coinCost, item.gemCost))
                return false;
            owned.Add(id);
            Equip(id);
            SaveSystem.Instance?.CaptureFromManagers();
            SaveSystem.Instance?.Save();
            return true;
        }

        public bool Equip(string id)
        {
            var item = Find(id);
            if (item == null || !owned.Contains(id)) return false;
            switch (item.category)
            {
                case ShopCategory.Skates: EquippedSkates = id; break;
                case ShopCategory.Helmet:
                case ShopCategory.Pads: EquippedHelmet = id; break;
                case ShopCategory.Clothing: EquippedOutfit = id; break;
                case ShopCategory.Avatar: EquippedAvatar = id; break;
            }

            ApplyToSkater();
            SaveSystem.Instance?.CaptureFromManagers();
            SaveSystem.Instance?.Save();
            return true;
        }

        public void ApplyToSkater()
        {
            PeekEquippedBonuses(out float speed, out float jump, out float handling, out int armor, out _);
            SkateController.Instance?.ApplyLoadout(speed, jump, handling, armor);
        }

        public void PeekEquippedBonuses(out float speed, out float jump, out float handling, out int armor, out float magnet)
        {
            speed = jump = handling = magnet = 0f;
            armor = 0;
            Accumulate(Find(EquippedSkates), ref speed, ref jump, ref handling, ref armor, ref magnet);
            Accumulate(Find(EquippedHelmet), ref speed, ref jump, ref handling, ref armor, ref magnet);
            Accumulate(Find(EquippedOutfit), ref speed, ref jump, ref handling, ref armor, ref magnet);
        }

        public void WriteToSave(SaveData data)
        {
            data.ownedItems = new List<string>(owned);
            data.equippedSkates = EquippedSkates;
            data.equippedHelmet = EquippedHelmet;
            data.equippedOutfit = EquippedOutfit;
            data.equippedAvatar = EquippedAvatar;
        }

        public void ReadFromSave(SaveData data)
        {
            owned.Clear();
            if (data.ownedItems != null)
            {
                for (int i = 0; i < data.ownedItems.Count; i++)
                    owned.Add(data.ownedItems[i]);
            }

            EnsureStarters();
            EquippedSkates = owned.Contains(data.equippedSkates) ? data.equippedSkates : "skates_street";
            EquippedHelmet = owned.Contains(data.equippedHelmet) ? data.equippedHelmet : "";
            EquippedOutfit = owned.Contains(data.equippedOutfit) ? data.equippedOutfit : "outfit_tee";
            EquippedAvatar = owned.Contains(data.equippedAvatar) ? data.equippedAvatar : "avatar_default";
            ApplyToSkater();
        }

        public ShopItemData Find(string id)
        {
            if (string.IsNullOrEmpty(id) || catalog == null) return null;
            for (int i = 0; i < catalog.Length; i++)
                if (catalog[i] != null && catalog[i].id == id)
                    return catalog[i];
            return null;
        }

        public ShopItemData FirstAffordable()
        {
            if (catalog == null || CurrencyManager.Instance == null) return null;
            for (int i = 0; i < catalog.Length; i++)
            {
                var item = catalog[i];
                if (item == null || owned.Contains(item.id)) continue;
                if (CurrencyManager.Instance.Coins >= item.coinCost && CurrencyManager.Instance.Gems >= item.gemCost)
                    return item;
            }

            return null;
        }

        void EnsureStarters()
        {
            if (catalog == null) return;
            for (int i = 0; i < catalog.Length; i++)
                if (catalog[i] != null && catalog[i].starterOwned)
                    owned.Add(catalog[i].id);
        }

        static void Accumulate(ShopItemData item, ref float speed, ref float jump, ref float handling, ref int armor, ref float magnet)
        {
            if (item == null) return;
            speed += item.speedBonus;
            jump += item.jumpBonus;
            handling += item.handlingBonus;
            armor += item.armorSlots;
            magnet += item.magnetBonus;
        }

        public static ShopItemData[] BuildCatalog() => CatalogFactory.BuildShopItems();
    }
}
