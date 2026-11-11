using System;
using Reborn.Combat;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Inventory
{
    /// <summary>Aktion UsePotion: benutzt den schwaechsten vorhandenen Heiltrank. Abklingzeit 1 s.</summary>
    [RequireComponent(typeof(PlayerInventory))]
    public class PotionUser : MonoBehaviour
    {
        public InputActionAsset actions;
        public float cooldown = 1f;

        public event Action OnUsed;

        PlayerInventory _inventory;
        Health _health;
        InputAction _use;
        float _cooldownTimer;

        void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _health = GetComponent<Health>();
            if (actions != null) _use = actions.FindActionMap("Gameplay", true).FindAction("UsePotion", true);
        }

        void OnEnable()
        {
            if (_use != null) _use.actionMap.Enable();
        }

        void Update()
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            if (_use != null && _use.WasPressedThisFrame()) UseNextPotion();
        }

        /// <summary>Inventarplatz des schwaechsten Heiltranks, oder -1.</summary>
        public int NextPotionIndex()
        {
            int best = -1;
            for (int i = 0; i < PlayerInventory.Size; i++)
            {
                var slot = _inventory.slots[i];
                if (slot.IsEmpty || !slot.item.IsUsable) continue;
                if (best < 0 || slot.item.healAmount < _inventory.slots[best].item.healAmount) best = i;
            }
            return best;
        }

        public ItemData NextPotion
        {
            get
            {
                int index = NextPotionIndex();
                return index >= 0 ? _inventory.slots[index].item : null;
            }
        }

        public int PotionCount
        {
            get
            {
                var potion = NextPotion;
                return potion != null ? _inventory.Count(potion) : 0;
            }
        }

        public bool UseNextPotion()
        {
            int index = NextPotionIndex();
            return index >= 0 && UseAt(index);
        }

        /// <summary>Benutzt den Gegenstand im Inventarplatz (nur Verbrauch mit Heilung).</summary>
        public bool UseAt(int index)
        {
            if (_cooldownTimer > 0f || _health == null || _health.IsDead) return false;
            if (index < 0 || index >= PlayerInventory.Size) return false;
            var slot = _inventory.slots[index];
            if (slot.IsEmpty || !slot.item.IsUsable) return false;
            if (_health.Current >= _health.maxHealth) return false;
            int heal = slot.item.healAmount;
            _inventory.RemoveAt(index, 1);
            _health.Heal(heal);
            _cooldownTimer = cooldown;
            OnUsed?.Invoke();
            return true;
        }
    }
}
