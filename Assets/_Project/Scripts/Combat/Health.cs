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
        /// <summary>Verteidigung: Schaden x 20 / (20 + Verteidigung), mindestens 1. Bei 0 unveraendert.</summary>
        public int defense;

        float _hitTimer;

        /// <summary>Schaden, Rueckstoss-Geschwindigkeit</summary>
        public event Action<int, Vector2> OnDamaged;
        public event Action OnDied;
        public event Action OnChanged;
        /// <summary>Tatsaechlich geheilte Menge</summary>
        public event Action<int> OnHealed;

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
            if (defense > 0) amount = Mathf.Max(1, Mathf.RoundToInt(amount * 20f / (20f + defense)));
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
            int before = Current;
            Current = Mathf.Min(maxHealth, Current + amount);
            if (Current > before) OnHealed?.Invoke(Current - before);
            OnChanged?.Invoke();
        }

        /// <summary>Leben direkt setzen (Spielstand laden).</summary>
        public void SetCurrent(int value)
        {
            Current = Mathf.Clamp(value, 1, maxHealth);
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
