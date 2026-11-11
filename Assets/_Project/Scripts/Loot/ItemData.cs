using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Beschreibt einen Gegenstand (Daten, kein Objekt in der Szene).</summary>
    [CreateAssetMenu(menuName = "Reborn/Item", fileName = "NewItem")]
    public class ItemData : ScriptableObject
    {
        /// <summary>Eindeutige ID fuer den Spielstand, z. B. "holzschwert".</summary>
        public string id;
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemRarity rarity = ItemRarity.Gewoehnlich;
        public ItemCategory category = ItemCategory.Material;
        public EquipSlot equipSlot = EquipSlot.None;
        public StatBlock stats;
        /// <summary>Heilung beim Benutzen (nur Verbrauchsgegenstaende).</summary>
        public int healAmount;
        public int sellValue;
        public int maxStack = 99;

        public bool IsEquipment => category == ItemCategory.Ausruestung && equipSlot != EquipSlot.None;
        public bool IsUsable => category == ItemCategory.Verbrauch && healAmount > 0;

        public string CategoryLabel
        {
            get
            {
                switch (category)
                {
                    case ItemCategory.Verbrauch: return "Verbrauchsgegenstand";
                    case ItemCategory.Ausruestung: return EquipSlots.Label(equipSlot);
                    default: return "Material";
                }
            }
        }

        public static string RarityLabel(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Selten: return "Selten";
                case ItemRarity.Episch: return "Episch";
                case ItemRarity.Legendaer: return "Legendär";
                default: return "Gewöhnlich";
            }
        }

        public static Color RarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Selten: return new Color32(70, 130, 230, 255);
                case ItemRarity.Episch: return new Color32(170, 80, 220, 255);
                case ItemRarity.Legendaer: return new Color32(240, 150, 40, 255);
                default: return new Color32(150, 150, 150, 255);
            }
        }
    }
}
