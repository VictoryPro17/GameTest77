using System;
using UnityEngine;

namespace Reborn.Economy
{
    /// <summary>Gold des Spielers (Meilenstein 6). Beute, Verkaufen und Kaufen laufen hierueber.</summary>
    public class PlayerWallet : MonoBehaviour
    {
        public int Gold { get; private set; }

        /// <summary>Neuer Goldstand</summary>
        public event Action<int> OnChanged;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            OnChanged?.Invoke(Gold);
        }

        public bool CanAfford(int amount) => amount >= 0 && Gold >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount)) return false;
            Gold -= amount;
            OnChanged?.Invoke(Gold);
            return true;
        }

        /// <summary>Verliert einen Anteil (abgerundet), z. B. 0,1 beim Tod. Gibt den Verlust zurueck.</summary>
        public int LoseFraction(float fraction)
        {
            int lost = Mathf.FloorToInt(Gold * Mathf.Clamp01(fraction));
            if (lost <= 0) return 0;
            Gold -= lost;
            OnChanged?.Invoke(Gold);
            return lost;
        }

        /// <summary>Goldstand direkt setzen (Spielstand laden).</summary>
        public void SetGold(int value)
        {
            Gold = Mathf.Max(0, value);
            OnChanged?.Invoke(Gold);
        }
    }
}
