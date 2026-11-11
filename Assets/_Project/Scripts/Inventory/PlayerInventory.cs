using System;
using System.Collections.Generic;
using System.Linq;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.Inventory
{
    /// <summary>Raster-Inventar mit 24 Plaetzen (angezeigt als 6 x 4).</summary>
    public class PlayerInventory : MonoBehaviour
    {
        public const int Size = 24;

        [Serializable]
        public class Slot
        {
            public ItemData item;
            public int count;
            public bool IsEmpty => item == null || count <= 0;
        }

        public Slot[] slots = new Slot[Size];

        public event Action OnChanged;

        void Awake()
        {
            EnsureSlots();
        }

        void EnsureSlots()
        {
            if (slots == null || slots.Length != Size) slots = new Slot[Size];
            for (int i = 0; i < Size; i++)
                if (slots[i] == null) slots[i] = new Slot();
        }

        static int StackLimit(ItemData item) => Mathf.Max(1, item.maxStack);

        /// <summary>Legt Gegenstaende ab: erst auf vorhandene Stapel, dann in freie Plaetze.</summary>
        /// <returns>Menge, die nicht mehr gepasst hat.</returns>
        public int Add(ItemData item, int count)
        {
            EnsureSlots();
            if (item == null || count <= 0) return 0;
            int rest = count;
            int limit = StackLimit(item);
            foreach (var slot in slots)
            {
                if (rest <= 0) break;
                if (slot.IsEmpty || slot.item != item || slot.count >= limit) continue;
                int put = Mathf.Min(limit - slot.count, rest);
                slot.count += put;
                rest -= put;
            }
            foreach (var slot in slots)
            {
                if (rest <= 0) break;
                if (!slot.IsEmpty) continue;
                int put = Mathf.Min(limit, rest);
                slot.item = item;
                slot.count = put;
                rest -= put;
            }
            if (rest != count) OnChanged?.Invoke();
            return rest;
        }

        /// <summary>Wie viele Stuecke dieses Gegenstands noch passen.</summary>
        public int SpaceFor(ItemData item)
        {
            EnsureSlots();
            if (item == null) return 0;
            int limit = StackLimit(item);
            int space = 0;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) space += limit;
                else if (slot.item == item) space += Mathf.Max(0, limit - slot.count);
            }
            return space;
        }

        public void RemoveAt(int index, int count)
        {
            EnsureSlots();
            if (index < 0 || index >= Size || count <= 0) return;
            var slot = slots[index];
            if (slot.IsEmpty) return;
            slot.count -= count;
            if (slot.count <= 0)
            {
                slot.item = null;
                slot.count = 0;
            }
            OnChanged?.Invoke();
        }

        public void Swap(int a, int b)
        {
            EnsureSlots();
            if (a == b || a < 0 || b < 0 || a >= Size || b >= Size) return;
            (slots[a], slots[b]) = (slots[b], slots[a]);
            OnChanged?.Invoke();
        }

        static int CategoryOrder(ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Ausruestung: return 0;
                case ItemCategory.Verbrauch: return 1;
                default: return 2;
            }
        }

        /// <summary>Fasst Stapel zusammen und sortiert: Ausruestung, Verbrauch, Material; dann Seltenheit absteigend, dann Name.</summary>
        public void Sort()
        {
            EnsureSlots();
            var totals = new Dictionary<ItemData, int>();
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;
                totals.TryGetValue(slot.item, out int n);
                totals[slot.item] = n + slot.count;
            }
            var ordered = totals.Keys
                .OrderBy(i => CategoryOrder(i.category))
                .ThenByDescending(i => (int)i.rarity)
                .ThenBy(i => i.itemName, StringComparer.CurrentCulture)
                .ToList();
            foreach (var slot in slots)
            {
                slot.item = null;
                slot.count = 0;
            }
            int index = 0;
            foreach (var item in ordered)
            {
                int rest = totals[item];
                int limit = StackLimit(item);
                while (rest > 0 && index < Size)
                {
                    int put = Mathf.Min(limit, rest);
                    slots[index].item = item;
                    slots[index].count = put;
                    rest -= put;
                    index++;
                }
            }
            OnChanged?.Invoke();
        }

        public int Count(ItemData item)
        {
            EnsureSlots();
            int total = 0;
            foreach (var slot in slots)
                if (!slot.IsEmpty && slot.item == item) total += slot.count;
            return total;
        }

        public int IndexOf(ItemData item)
        {
            EnsureSlots();
            for (int i = 0; i < Size; i++)
                if (!slots[i].IsEmpty && slots[i].item == item) return i;
            return -1;
        }

        /// <summary>Setzt einen Platz direkt (Spielstand laden, Ausruestung tauschen). Ruft OnChanged nicht auf.</summary>
        public void SetSlot(int index, ItemData item, int count)
        {
            EnsureSlots();
            if (index < 0 || index >= Size) return;
            slots[index].item = count > 0 ? item : null;
            slots[index].count = item != null ? Mathf.Max(0, count) : 0;
        }

        public void Clear()
        {
            EnsureSlots();
            foreach (var slot in slots)
            {
                slot.item = null;
                slot.count = 0;
            }
            OnChanged?.Invoke();
        }

        public void NotifyChanged() => OnChanged?.Invoke();
    }
}
