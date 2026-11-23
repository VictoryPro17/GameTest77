using System;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Guilds
{
    public enum GuildId
    {
        None,
        Krieger,
        Magier,
        Assassine
    }

    public enum QuestKind
    {
        /// <summary>Kreaturen besiegen (target = "schleim", "wolf", "spinne" oder "*" fuer alle)</summary>
        Besiegen,
        /// <summary>Kreaturen mit Zaubern besiegen</summary>
        MitZauberBesiegen,
        /// <summary>Gegenstaende abgeben (target = Item-ID; optional item2/amount2)</summary>
        Bringen
    }

    /// <summary>Eine Gilden-Quest (Meilenstein 7c)</summary>
    [Serializable]
    public class QuestData
    {
        public string title;
        public QuestKind kind;
        public string target;
        public int amount = 1;
        public string item2;
        public int amount2;
        public int gold;
        public int xp;
    }

    /// <summary>Eine Gilde: Name, Attribut-Bonus, Gildenwaffe, 3 Quests (Meilenstein 7c).</summary>
    [CreateAssetMenu(menuName = "Reborn/Guild", fileName = "NewGuild")]
    public class GuildData : ScriptableObject
    {
        public GuildId id;
        public string guildName;
        public PlayerAttribute attribute;
        public ItemData weapon;
        public QuestData[] quests = new QuestData[0];

        public static string RankName(int rank) => rank >= 2 ? "Geselle" : rank == 1 ? "Anwärter" : "-";
    }
}
