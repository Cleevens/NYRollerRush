// NY ROLLER RUSH - CORE SYSTEM

using UnityEngine;

namespace NYRollerRush.Core
{
    public enum ShopCategory
    {
        Avatar,
        Clothing,
        Helmet,
        Pads,
        Skates
    }

    [CreateAssetMenu(menuName = "NY Roller Rush/Shop Item", fileName = "ShopItem")]
    public class ShopItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public ShopCategory category;
        public int coinCost;
        public int gemCost;
        public bool starterOwned;
        public float speedBonus;
        public float jumpBonus;
        public float handlingBonus;
        public int armorSlots;
        public float magnetBonus;

        public static ShopItemData Create(string id, string name, ShopCategory category, int coins, int gems, bool starter, float speed, float jump, float handling, int armor, float magnet)
        {
            var item = CreateInstance<ShopItemData>();
            item.id = id;
            item.displayName = name;
            item.category = category;
            item.coinCost = coins;
            item.gemCost = gems;
            item.starterOwned = starter;
            item.speedBonus = speed;
            item.jumpBonus = jump;
            item.handlingBonus = handling;
            item.armorSlots = armor;
            item.magnetBonus = magnet;
            item.name = name;
            return item;
        }
    }
}
