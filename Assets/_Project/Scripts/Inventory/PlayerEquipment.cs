using System;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.Inventory
{
    /// <summary>10 Ausruestungsplaetze (Reihenfolge siehe EquipSlots.Positions).</summary>
    [RequireComponent(typeof(PlayerInventory))]
    public class PlayerEquipment : MonoBehaviour
    {
        public const int SlotCount = 10;

        public ItemData[] equipped = new ItemData[SlotCount];

        public event Action OnChanged;

        PlayerInventory _inventory;

        void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            if (equipped == null || equipped.Length != SlotCount) equipped = new ItemData[SlotCount];
        }

        public ItemData Get(int position) => position >= 0 && position < SlotCount ? equipped[position] : null;

        /// <summary>Platz, in den dieses Teil kommen wuerde (Ringe: erst ein freier Ringplatz, sonst der erste).</summary>
        public int TargetPosition(ItemData item)
        {
            if (item == null || !item.IsEquipment) return -1;
            int first = -1;
            for (int i = 0; i < SlotCount; i++)
            {
                if (EquipSlots.Positions[i] != item.equipSlot) continue;
                if (first < 0) first = i;
                if (equipped[i] == null) return i;
            }
            return first;
        }

        /// <summary>Teil, das beim Anlegen ersetzt wuerde (fuer den Vergleich).</summary>
        public ItemData WouldReplace(ItemData item)
        {
            int position = TargetPosition(item);
            return position >= 0 ? equipped[position] : null;
        }

        /// <summary>Legt das Teil aus dem Inventarplatz an. Ein vorhandenes Teil wandert in diesen Inventarplatz.</summary>
        public bool Equip(int inventoryIndex)
        {
            if (_inventory == null || inventoryIndex < 0 || inventoryIndex >= PlayerInventory.Size) return false;
            var slot = _inventory.slots[inventoryIndex];
            if (slot.IsEmpty || !slot.item.IsEquipment) return false;
            var item = slot.item;
            int position = TargetPosition(item);
            if (position < 0) return false;
            var old = equipped[position];

            if (slot.count > 1)
            {
                // Mehrere gleiche Teile im Stapel: eines nehmen, das alte Teil anderswo ablegen
                if (old != null && _inventory.SpaceFor(old) <= 0) return false;
                _inventory.SetSlot(inventoryIndex, item, slot.count - 1);
                if (old != null) _inventory.Add(old, 1);
            }
            else
            {
                _inventory.SetSlot(inventoryIndex, old, old != null ? 1 : 0);
            }
            equipped[position] = item;
            _inventory.NotifyChanged();
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Legt das Teil ab und steckt es ins Inventar. Schlaegt fehl, wenn das Inventar voll ist.</summary>
        public bool Unequip(int position)
        {
            if (_inventory == null || position < 0 || position >= SlotCount) return false;
            var item = equipped[position];
            if (item == null) return false;
            if (_inventory.SpaceFor(item) <= 0) return false;
            _inventory.Add(item, 1);
            equipped[position] = null;
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Setzt einen Platz direkt (Spielstand laden). Ruft OnChanged nicht auf.</summary>
        public void SetEquipped(int position, ItemData item)
        {
            if (position < 0 || position >= SlotCount) return;
            equipped[position] = item;
        }

        public void ClearAll()
        {
            for (int i = 0; i < SlotCount; i++) equipped[i] = null;
            OnChanged?.Invoke();
        }

        public void NotifyChanged() => OnChanged?.Invoke();

        public StatBlock TotalStats
        {
            get
            {
                var total = new StatBlock();
                foreach (var item in equipped)
                    if (item != null) total = total + item.stats;
                return total;
            }
        }
    }
}
