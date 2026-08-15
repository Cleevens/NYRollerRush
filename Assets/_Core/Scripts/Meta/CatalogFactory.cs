// NY ROLLER RUSH - CORE SYSTEM
// Single runtime catalog for neighborhoods and shop items.
// Prefers Resources/Neighborhoods and Resources/ShopItems .asset files when present.

using System;
using UnityEngine;

namespace NYRollerRush.Core
{
    public static class CatalogFactory
    {
        public static NeighborhoodData[] LoadNeighborhoods()
        {
            var loaded = Resources.LoadAll<NeighborhoodData>("Neighborhoods");
            if (loaded != null && loaded.Length > 0)
            {
                Array.Sort(loaded, (a, b) => a.unlockScore.CompareTo(b.unlockScore));
                return loaded;
            }

            return BuildNeighborhoods();
        }

        public static ShopItemData[] LoadShopItems()
        {
            var loaded = Resources.LoadAll<ShopItemData>("ShopItems");
            if (loaded != null && loaded.Length > 0)
                return loaded;
            return BuildShopItems();
        }

        public static NeighborhoodData[] BuildNeighborhoods()
        {
            return new[]
            {
                NeighborhoodData.Create(
                    "times_square", "Times Square", 0, 0.55f, 0.5f, 8f, 1f,
                    "Tourist crowds, bright signs, moderate cabs.",
                    "Audio/Music/times_square",
                    new Color(0.72f, 0.55f, 0.42f), new Color(0.55f, 0.38f, 0.28f), 0.01f,
                    new Color(1f, 0.88f, 0.7f)),
                NeighborhoodData.Create(
                    "midtown", "Midtown", 2500, 0.9f, 0.38f, 10.2f, 1.12f,
                    "Faster crosstown traffic, tighter blocks, bus lanes.",
                    "Audio/Music/midtown",
                    new Color(0.42f, 0.5f, 0.62f), new Color(0.28f, 0.34f, 0.42f), 0.016f,
                    new Color(0.85f, 0.9f, 1f)),
                NeighborhoodData.Create(
                    "central_park", "Central Park", 5000, 0.38f, 0.78f, 7f, 0.88f,
                    "Paths, carriages, joggers cutting across lanes.",
                    "Audio/Music/central_park",
                    new Color(0.38f, 0.55f, 0.4f), new Color(0.3f, 0.42f, 0.3f), 0.014f,
                    new Color(0.85f, 0.95f, 0.75f)),
                NeighborhoodData.Create(
                    "brooklyn_bridge", "Brooklyn Bridge", 8000, 0.82f, 0.42f, 11.2f, 1.22f,
                    "Cable shadows, tourist vans, sudden gusts on the span.",
                    "Audio/Music/brooklyn_bridge",
                    new Color(0.55f, 0.5f, 0.48f), new Color(0.4f, 0.38f, 0.36f), 0.018f,
                    new Color(1f, 0.82f, 0.62f)),
                NeighborhoodData.Create(
                    "soho_chinatown", "SoHo / Chinatown", 12000, 1.2f, 0.72f, 9.6f, 1.1f,
                    "Delivery bikes, market crates, tight neon alleys.",
                    "Audio/Music/soho_chinatown",
                    new Color(0.55f, 0.28f, 0.26f), new Color(0.45f, 0.16f, 0.14f), 0.02f,
                    new Color(1f, 0.55f, 0.4f))
            };
        }

        public static ShopItemData[] BuildShopItems()
        {
            return new[]
            {
                ShopItemData.Create("avatar_default", "Default Skater", ShopCategory.Avatar, 0, 0, true, 0, 0, 0, 0, 0),
                ShopItemData.Create("outfit_tee", "Times Square Tee", ShopCategory.Clothing, 0, 0, true, 0, 0, 0, 0, 0),
                ShopItemData.Create("outfit_neon", "Neon Windbreaker", ShopCategory.Clothing, 320, 0, false, 0, 0, 0.06f, 0, 1.1f),
                ShopItemData.Create("outfit_taxi", "Taxi Vest", ShopCategory.Clothing, 350, 0, false, 0.04f, 0, 0.1f, 0, 0),
                ShopItemData.Create("outfit_chinatown", "Chinatown Jacket", ShopCategory.Clothing, 520, 0, false, 0, 0.05f, 0.05f, 0, 1.5f),
                ShopItemData.Create("skates_street", "Street Gliders", ShopCategory.Skates, 0, 0, true, 0, 0, 0, 0, 0),
                ShopItemData.Create("skates_park", "Park Cruisers", ShopCategory.Skates, 280, 0, false, 0.08f, 0.22f, 0.08f, 0, 0),
                ShopItemData.Create("skates_night", "Night Rush Skates", ShopCategory.Skates, 450, 0, false, 0.18f, 0.12f, 0.16f, 0, 0),
                ShopItemData.Create("skates_brooklyn", "Bridge Carbons", ShopCategory.Skates, 900, 0, false, 0.24f, 0.1f, 0.2f, 0, 0),
                ShopItemData.Create("pads_street", "Street Knee Pads", ShopCategory.Pads, 180, 0, false, 0, 0, 0, 1, 0),
                ShopItemData.Create("helmet_bike", "Bike Helmet", ShopCategory.Helmet, 220, 0, false, 0, 0, 0, 1, 0),
                ShopItemData.Create("helmet_commuter", "Commuter Helmet", ShopCategory.Helmet, 480, 0, false, 0, 0, 0.08f, 1, 0),
                ShopItemData.Create("helmet_carbon", "Carbon Helmet", ShopCategory.Helmet, 750, 1, false, 0, 0, 0.06f, 2, 0)
            };
        }
    }
}
