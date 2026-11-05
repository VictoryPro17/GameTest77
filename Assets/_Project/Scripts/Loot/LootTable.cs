using System;
using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Welche Gegenstaende ein Gegner mit welcher Chance fallen laesst.</summary>
    [CreateAssetMenu(menuName = "Reborn/Loot Table", fileName = "NewLootTable")]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public ItemData item;
            [Range(0f, 1f)] public float dropChance = 1f;
            public int min = 1;
            public int max = 1;
        }

        public Entry[] entries = Array.Empty<Entry>();
    }
}
