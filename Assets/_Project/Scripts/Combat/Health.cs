using System;
using UnityEngine;

namespace Reborn.Combat
{
    /// <summary>Lebenspunkte fuer Spieler und Gegner.</summary>
    public class Health : MonoBehaviour
    {
        public int maxHealth = 50;
        public bool invulnerable;

        /// <summary>Schaden, Rueckstoss-Geschwindigkeit</summary>
        public event Action<int, Vector2> OnDamaged;
        public event Action OnDied;
        public event Action OnChanged;

        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        void Awake()
        {
            Current = maxHealth;
        }

        /// <returns>true, wenn der Treffer zaehlt</returns>
        public bool TakeDamage(int amount, Vector2 knockback)
        {
            if (invulnerable || IsDead || amount <= 0) return false;
            Current = Mathf.Max(0, Current - amount);
            OnDamaged?.Invoke(amount, knockback);
            OnChanged?.Invoke();
            if (Current == 0) OnDied?.Invoke();
            return true;
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            OnChanged?.Invoke();
        }

        public void SetMaxHealth(int value, bool refill)
        {
            maxHealth = Mathf.Max(1, value);
            Current = refill ? maxHealth : Mathf.Min(Current, maxHealth);
            OnChanged?.Invoke();
        }
    }
}
