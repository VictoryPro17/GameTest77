using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Beschreibt einen Gegenstand (Daten, kein Objekt in der Szene).</summary>
    [CreateAssetMenu(menuName = "Reborn/Item", fileName = "NewItem")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemRarity rarity = ItemRarity.Gewoehnlich;
        public int maxStack = 99;
    }
}
