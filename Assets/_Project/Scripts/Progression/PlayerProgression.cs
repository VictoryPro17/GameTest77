using System;
using Reborn.Combat;
using UnityEngine;

namespace Reborn.Progression
{
    /// <summary>Level und Erfahrung. Naechstes Level braucht 100 x Level XP.</summary>
    public class PlayerProgression : MonoBehaviour
    {
        public int healthPerLevel = 10;

        public int Level { get; private set; } = 1;
        public int XP { get; private set; }
        public int XPToNextLevel => 100 * Level;

        public event Action OnChanged;
        public event Action<int> OnLevelUp;

        Health _health;

        void Awake()
        {
            _health = GetComponent<Health>();
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            XP += amount;
            while (XP >= XPToNextLevel)
            {
                XP -= XPToNextLevel;
                Level++;
                if (_health != null) _health.SetMaxHealth(_health.maxHealth + healthPerLevel, true);
                OnLevelUp?.Invoke(Level);
            }
            OnChanged?.Invoke();
        }
    }
}
