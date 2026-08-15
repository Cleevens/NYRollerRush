// NY ROLLER RUSH - CORE SYSTEM

using System;
using System.Collections.Generic;

namespace NYRollerRush.Core
{
    [Serializable]
    public class SaveData
    {
        public int highScore;
        public int coins;
        public int gems;
        public List<string> unlockedNeighborhoods = new List<string>();
        public List<string> ownedItems = new List<string>();
        public string equippedSkates = "skates_street";
        public string equippedHelmet = "";
        public string equippedOutfit = "outfit_tee";
        public string equippedAvatar = "avatar_default";
        public string lastNeighborhood = "times_square";
        public string lastDailyRewardDate = "";
        public int dailyRewardStreak;
    }
}
