using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Einfaches Inventar (Liste). Das Raster-Inventar folgt in Meilenstein 2.</summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Serializable]
        public class Stack
        {
            public ItemData item;
            public int count;
        }

        public List<Stack> items = new List<Stack>();

        public event Action OnChanged;

        public void Add(ItemData item, int count)
        {
            if (item == null || count <= 0) return;
            var stack = items.Find(s => s.item == item);
            if (stack == null)
            {
                stack = new Stack { item = item, count = 0 };
                items.Add(stack);
            }
            stack.count = Mathf.Min(stack.count + count, Mathf.Max(1, item.maxStack));
            OnChanged?.Invoke();
        }

        public int Count(ItemData item)
        {
            var stack = items.Find(s => s.item == item);
            return stack != null ? stack.count : 0;
        }
    }
}
