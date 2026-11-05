using System;
using UnityEngine;

namespace Reborn.Combat
{
    /// <summary>Lebenspunkte fuer Spieler und Gegner.</summary>
    public class Health : MonoBehaviour
    {
        public int maxHealth = 50;
        public bool invulnerable;
        /// <summary>Schutzzeit in Sekunden nach einem Treffer (0 = keine)</summary>
        public float hitInvulnerability = 0f;

        float _hitTimer;

        /// <summary>Schaden, Rueckstoss-Geschwindigkeit</summary>
        public event Action<int, Vector2> OnDamaged;
        public event Action OnDied;
        public event Action OnChanged;

        public int Current { get; private set; }
        public bool IsDead => Current <= 0;
        public bool IsHitInvulnerable => _hitTimer > 0f;

        void Awake()
        {
            Current = maxHealth;
        }

        void Update()
        {
            if (_hitTimer > 0f) _hitTimer -= Time.deltaTime;
        }

        /// <returns>true, wenn der Treffer zaehlt</returns>
        public bool TakeDamage(int amount, Vector2 knockback)
        {
            if (invulnerable || IsDead || amount <= 0) return false;
            if (_hitTimer > 0f) return false;
            Current = Mathf.Max(0, Current - amount);
            _hitTimer = hitInvulnerability;
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
