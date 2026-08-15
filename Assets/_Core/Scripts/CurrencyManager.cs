// NY ROLLER RUSH - CORE SYSTEM
// Soft coins + premium gems wallet.

using System;
using UnityEngine;

namespace NYRollerRush.Core
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public int Coins { get; private set; }
        public int Gems { get; private set; }

        public event Action<int, int> Changed;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Set(int coins, int gems)
        {
            Coins = Mathf.Max(0, coins);
            Gems = Mathf.Max(0, gems);
            Changed?.Invoke(Coins, Gems);
        }

        public void AddCoins(int amount)
        {
            Coins += Mathf.Max(0, amount);
            Changed?.Invoke(Coins, Gems);
        }

        public void AddGems(int amount)
        {
            Gems += Mathf.Max(0, amount);
            Changed?.Invoke(Coins, Gems);
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (Coins < amount) return false;
            Coins -= amount;
            Changed?.Invoke(Coins, Gems);
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (amount <= 0) return true;
            if (Gems < amount) return false;
            Gems -= amount;
            Changed?.Invoke(Coins, Gems);
            return true;
        }

        public bool TrySpend(int coins, int gems)
        {
            if (Coins < coins || Gems < gems) return false;
            return TrySpendCoins(coins) && TrySpendGems(gems);
        }
    }
}
