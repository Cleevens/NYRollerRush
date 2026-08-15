// NY ROLLER RUSH - CORE SYSTEM
// First launch of each UTC day grants bonus coins. Streak grows if claimed yesterday.

using System;
using System.Globalization;
using UnityEngine;

namespace NYRollerRush.Core
{
    public static class DailyReward
    {
        public const int BaseCoins = 80;
        public const int StreakBonus = 15;
        public const int MaxGrant = 200;

        public static int LastGrant { get; private set; }
        public static bool GrantedThisSession { get; private set; }

        public static int TryGrant()
        {
            LastGrant = 0;
            GrantedThisSession = false;
            var save = SaveSystem.Instance;
            var wallet = CurrencyManager.Instance;
            if (save == null || save.Data == null || wallet == null)
                return 0;

            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (save.Data.lastDailyRewardDate == today)
                return 0;

            int streak = 1;
            if (!string.IsNullOrEmpty(save.Data.lastDailyRewardDate)
                && DateTime.TryParseExact(save.Data.lastDailyRewardDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var last)
                && (DateTime.UtcNow.Date - last.Date).TotalDays <= 1.01)
                streak = Mathf.Max(1, save.Data.dailyRewardStreak + 1);

            int grant = Mathf.Min(MaxGrant, BaseCoins + (streak - 1) * StreakBonus);
            wallet.AddCoins(grant);
            save.Data.lastDailyRewardDate = today;
            save.Data.dailyRewardStreak = streak;
            save.CaptureFromManagers();
            save.Save();
            LastGrant = grant;
            GrantedThisSession = true;
            return grant;
        }
    }
}
