using System;
using Reborn.Combat;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Inventory
{
    /// <summary>
    /// Traenke: UsePotion (Q / Nord-Taste) benutzt den schwaechsten Heiltrank,
    /// UseManaPotion (R / Steuerkreuz oben) den schwaechsten Manatrank. Gemeinsame Abklingzeit 1 s.
    /// </summary>
    [RequireComponent(typeof(PlayerInventory))]
    public class PotionUser : MonoBehaviour
    {
        public InputActionAsset actions;
        public float cooldown = 1f;

        public event Action OnUsed;

        PlayerInventory _inventory;
        Health _health;
        Magic.PlayerMana _mana;
        InputAction _use;
        InputAction _useMana;
        float _cooldownTimer;

        void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _health = GetComponent<Health>();
            _mana = GetComponent<Magic.PlayerMana>();
            if (actions != null)
            {
                var map = actions.FindActionMap("Gameplay", true);
                _use = map.FindAction("UsePotion", true);
                _useMana = map.FindAction("UseManaPotion", false);
            }
        }

        void OnEnable()
        {
            if (_use != null) _use.actionMap.Enable();
        }

        void Update()
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            if (_use != null && _use.WasPressedThisFrame()) UseNextPotion();
            if (_useMana != null && _useMana.WasPressedThisFrame()) UseNextManaPotion();
        }

        int Weakest(Func<ItemData, bool> filter, Func<ItemData, int> amount)
        {
            int best = -1;
            for (int i = 0; i < PlayerInventory.Size; i++)
            {
                var slot = _inventory.slots[i];
                if (slot.IsEmpty || !filter(slot.item)) continue;
                if (best < 0 || amount(slot.item) < amount(_inventory.slots[best].item)) best = i;
            }
            return best;
        }

        /// <summary>Inventarplatz des schwaechsten Heiltranks, oder -1.</summary>
        public int NextPotionIndex() => Weakest(i => i.IsHealPotion, i => i.healAmount);
        public int NextManaPotionIndex() => Weakest(i => i.IsManaPotion, i => i.manaAmount);

        ItemData ItemAt(int index) => index >= 0 ? _inventory.slots[index].item : null;
        public ItemData NextPotion => ItemAt(NextPotionIndex());
        public ItemData NextManaPotion => ItemAt(NextManaPotionIndex());
        public int PotionCount => NextPotion != null ? _inventory.Count(NextPotion) : 0;
        public int ManaPotionCount => NextManaPotion != null ? _inventory.Count(NextManaPotion) : 0;

        public bool UseNextPotion()
        {
            int index = NextPotionIndex();
            return index >= 0 && UseAt(index);
        }

        public bool UseNextManaPotion()
        {
            int index = NextManaPotionIndex();
            return index >= 0 && UseAt(index);
        }

        /// <summary>Benutzt den Gegenstand im Inventarplatz (Heil- oder Manatrank). Nichts passiert, wenn er gerade nichts bringt.</summary>
        public bool UseAt(int index)
        {
            if (_cooldownTimer > 0f || _health == null || _health.IsDead) return false;
            if (index < 0 || index >= PlayerInventory.Size) return false;
            var slot = _inventory.slots[index];
            if (slot.IsEmpty || !slot.item.IsUsable) return false;
            var item = slot.item;
            bool healHelps = item.healAmount > 0 && _health.Current < _health.maxHealth;
            bool manaHelps = item.manaAmount > 0 && _mana != null && _mana.Current < _mana.Max - 0.01f;
            if (!healHelps && !manaHelps) return false;
            _inventory.RemoveAt(index, 1);
            if (item.healAmount > 0) _health.Heal(item.healAmount);
            if (item.manaAmount > 0 && _mana != null) _mana.SetCurrent(_mana.Current + item.manaAmount);
            _cooldownTimer = cooldown;
            OnUsed?.Invoke();
            return true;
        }
    }
}
