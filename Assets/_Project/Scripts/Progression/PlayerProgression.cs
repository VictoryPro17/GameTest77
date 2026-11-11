using System;
using UnityEngine;

namespace Reborn.Progression
{
    /// <summary>Level und Erfahrung. Naechstes Level braucht 100 x Level XP. Punkte und Leben regelt PlayerStats.</summary>
    public class PlayerProgression : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public int XP { get; private set; }
        public int XPToNextLevel => 100 * Level;

        public event Action OnChanged;
        public event Action<int> OnLevelUp;

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            XP += amount;
            while (XP >= XPToNextLevel)
            {
                XP -= XPToNextLevel;
                Level++;
                OnLevelUp?.Invoke(Level);
            }
            OnChanged?.Invoke();
        }

        /// <summary>Zustand aus dem Spielstand setzen.</summary>
        public void SetState(int level, int xp)
        {
            Level = Mathf.Max(1, level);
            XP = Mathf.Clamp(xp, 0, XPToNextLevel - 1);
            OnChanged?.Invoke();
        }
    }
}
