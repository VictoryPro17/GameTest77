using System;
using System.Collections.Generic;

namespace Reborn.Save
{
    /// <summary>
    /// Inhalt der Spielstand-Datei (JSON).
    /// Version 2 (Meilenstein 3): Zauber-Stufen und Mana. Version 3 (Meilenstein 6): Gold, Schmiede-Stufen, Szene.
    /// Aeltere Dateien laden weiter;
    /// fehlende Felder behalten ihre Startwerte.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 7;

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

        // ---- ab Version 3 (Meilenstein 6: Basis)
        public int gold;
        public List<UpgradeSave> upgrades = new List<UpgradeSave>();
        /// <summary>Szene beim Speichern (nur Info; das Spiel startet immer in der Basis)</summary>
        public string lastScene = "";

        // ---- ab Version 4 (Meilenstein 7c: Gilden und Quests)
        /// <summary>"" = keine Gilde, sonst GuildId als Text</summary>
        public string guild = "";
        public int guildRank;
        public int questIndex;
        public bool questActive;
        public int questProgress;

        // ---- ab Version 5 (Meilenstein 8: Golem-Boss)
        public List<string> defeatedBosses = new List<string>();

        // ---- ab Version 6 (Meilenstein 8b: Boss kommt nach 10 Minuten wieder)
        public List<BossRecord> bossRecords = new List<BossRecord>();

        // ---- ab Version 7 (Meilenstein 9a: Siegel-Portal und weitere Welt-Schalter)
        public List<string> worldFlags = new List<string>();

        [Serializable]
        public class BossRecord
        {
            public string id;
            /// <summary>Zeitpunkt des letzten Siegs (UTC, DateTime.Ticks)</summary>
            public long defeatedAtTicks;
            public int kills;
        }

        [Serializable]
        public class UpgradeSave
        {
            public string id;
            public int level;
        }

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
