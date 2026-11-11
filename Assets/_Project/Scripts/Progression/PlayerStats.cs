using System;
using Reborn.Combat;
using Reborn.Inventory;
using Reborn.Loot;
using Reborn.Player;
using UnityEngine;

namespace Reborn.Progression
{
    public enum PlayerAttribute
    {
        Staerke,
        Geschick,
        Vitalitaet,
        Intelligenz,
        Glueck
    }

    /// <summary>Attribute, freie Punkte und die daraus berechneten Endwerte des Spielers.</summary>
    public class PlayerStats : MonoBehaviour
    {
        public const int AttributeCount = 5;
        public static readonly string[] AttributeLabels = { "Stärke", "Geschick", "Vitalität", "Intelligenz", "Glück" };

        public int pointsPerLevel = 3;
        /// <summary>Verteilte Grundwerte (ohne Ausruestung), Start jeweils 1.</summary>
        public int[] baseAttributes = { 1, 1, 1, 1, 1 };

        public int UnspentPoints { get; private set; }

        public event Action OnChanged;

        Health _health;
        PlayerAttack _attack;
        PlayerController _controller;
        PlayerEquipment _equipment;
        PlayerProgression _progression;

        void Awake()
        {
            _health = GetComponent<Health>();
            _attack = GetComponent<PlayerAttack>();
            _controller = GetComponent<PlayerController>();
            _equipment = GetComponent<PlayerEquipment>();
            _progression = GetComponent<PlayerProgression>();
            if (baseAttributes == null || baseAttributes.Length != AttributeCount) baseAttributes = new[] { 1, 1, 1, 1, 1 };
        }

        void OnEnable()
        {
            if (_equipment != null) _equipment.OnChanged += HandleEquipmentChanged;
            if (_progression != null) _progression.OnLevelUp += HandleLevelUp;
        }

        void OnDisable()
        {
            if (_equipment != null) _equipment.OnChanged -= HandleEquipmentChanged;
            if (_progression != null) _progression.OnLevelUp -= HandleLevelUp;
        }

        void Start()
        {
            Recalculate(true);
        }

        void HandleEquipmentChanged() => Recalculate(false);

        void HandleLevelUp(int level)
        {
            UnspentPoints += pointsPerLevel;
            Recalculate(true);
        }

        StatBlock Gear => _equipment != null ? _equipment.TotalStats : new StatBlock();
        int Level => _progression != null ? _progression.Level : 1;

        public int BaseAttribute(PlayerAttribute attribute) => baseAttributes[(int)attribute];

        int GearAttribute(PlayerAttribute attribute)
        {
            var gear = Gear;
            switch (attribute)
            {
                case PlayerAttribute.Staerke: return gear.strength;
                case PlayerAttribute.Geschick: return gear.dexterity;
                case PlayerAttribute.Vitalitaet: return gear.vitality;
                case PlayerAttribute.Intelligenz: return gear.intelligence;
                default: return gear.luck;
            }
        }

        /// <summary>Grundwert plus Bonus aus der Ausruestung.</summary>
        public int GetAttribute(PlayerAttribute attribute) => BaseAttribute(attribute) + GearAttribute(attribute);

        public int MaxHealth => 45 + 5 * GetAttribute(PlayerAttribute.Vitalitaet) + 10 * (Level - 1) + Gear.maxHealth;
        public int Attack => 9 + GetAttribute(PlayerAttribute.Staerke) + Gear.attack;
        public int Defense => Gear.defense;
        public float AttackCooldown => Mathf.Max(0.25f, 0.4f * (1f - 0.02f * GetAttribute(PlayerAttribute.Geschick)));
        public float DodgeCooldown => Mathf.Max(0.4f, 0.6f * (1f - 0.02f * GetAttribute(PlayerAttribute.Geschick)));
        public float LootMultiplier => 1f + 0.03f * GetAttribute(PlayerAttribute.Glueck);

        public bool AddPoint(PlayerAttribute attribute)
        {
            if (UnspentPoints <= 0) return false;
            UnspentPoints--;
            baseAttributes[(int)attribute]++;
            Recalculate(false);
            return true;
        }

        /// <summary>Zustand aus dem Spielstand setzen (danach Recalculate aufrufen).</summary>
        public void SetState(int[] attributes, int unspentPoints)
        {
            if (attributes != null && attributes.Length == AttributeCount) baseAttributes = (int[])attributes.Clone();
            UnspentPoints = Mathf.Max(0, unspentPoints);
        }

        /// <summary>Gibt die Endwerte an Leben, Angriff und Ausweichen weiter.</summary>
        public void Recalculate(bool refillHealth)
        {
            if (_health != null)
            {
                _health.SetMaxHealth(MaxHealth, refillHealth);
                _health.defense = Defense;
            }
            if (_attack != null)
            {
                _attack.damage = Attack;
                _attack.cooldown = AttackCooldown;
            }
            if (_controller != null) _controller.dodgeCooldown = DodgeCooldown;
            OnChanged?.Invoke();
        }
    }
}
