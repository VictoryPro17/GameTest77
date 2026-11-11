using System;
using System.Collections.Generic;

namespace Reborn.Save
{
    /// <summary>Inhalt der Spielstand-Datei (JSON).</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int level = 1;
        public int xp;
        public int[] attributes = { 1, 1, 1, 1, 1 };
        public int unspentPoints;
        public int currentHealth;
        public List<SlotData> inventory = new List<SlotData>();
        public List<EquipData> equipment = new List<EquipData>();
        public List<string> openedChests = new List<string>();

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
    }
}
