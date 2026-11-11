using System;
using System.Collections.Generic;

namespace Reborn.Save
{
    /// <summary>
    /// Inhalt der Spielstand-Datei (JSON).
    /// Version 2 (Meilenstein 3): Zauber-Stufen und Mana. Aeltere Dateien laden weiter;
    /// fehlende Felder behalten ihre Startwerte.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public int level = 1;
        public int xp;
        public int[] attributes = { 1, 1, 1, 1, 1 };
        public int unspentPoints;
        public int currentHealth;
        public List<SlotData> inventory = new List<SlotData>();
        public List<EquipData> equipment = new List<EquipData>();
        public List<string> openedChests = new List<string>();

        // ---- ab Version 2
        public int manaLevel = 1;
        public int manaXp;
        /// <summary>-1 = voll</summary>
        public float currentMana = -1f;
        public List<SpellSave> spells = new List<SpellSave>();

        [Serializable]
        public class SlotData
        {
            public int index;
            public string id;
            public int count;
        }

        [Serializable]
        public class EquipData
        {
            public int slot;
            public string id;
        }

        [Serializable]
        public class SpellSave
        {
            public string id;
            public int level = 1;
            public int xp;
        }
    }
}
