using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Liste aller Gegenstaende, damit der Spielstand IDs wieder in Items umwandeln kann.</summary>
    [CreateAssetMenu(menuName = "Reborn/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();

        public ItemData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var item in items)
                if (item != null && item.id == id) return item;
            return null;
        }
    }
}
