using Reborn.Core;
using System;
using Reborn.Economy;
using Reborn.Enemies;
using Reborn.Inventory;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Guilds
{
    /// <summary>
    /// Gilde des Spielers (Meilenstein 7c): nur eine Gilde, Wechsel kostet 200 Gold.
    /// Rang 1 "Anwaerter" (+2 auf das Gilden-Attribut, Gildenwaffe), nach 3 Quests Rang 2 "Geselle" (+4, Waffe auf Stufe +2).
    /// Eine Quest ist aktiv; Besiegen zaehlt ueber SlimeAI.OnAnyKilled, Bringen ueber das Inventar.
    /// </summary>
    public class PlayerGuild : MonoBehaviour
    {
        public const int SwitchCost = 200;
        public const int QuestsPerRank = 3;

        public GuildData[] guilds = new GuildData[0];

        public GuildId Guild { get; private set; } = GuildId.None;
        public int Rank { get; private set; }
        /// <summary>Naechste bzw. aktive Quest (0-2), 3 = alle erledigt</summary>
        public int QuestIndex { get; private set; }
        public bool QuestActive { get; private set; }
        public int Progress { get; private set; }

        public event Action OnChanged;

        PlayerInventory _inventory;
        PlayerWallet _wallet;
        PlayerStats _stats;
        PlayerProgression _progression;
        UpgradeBook _upgrades;

        void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _wallet = GetComponent<PlayerWallet>();
            _stats = GetComponent<PlayerStats>();
            _progression = GetComponent<PlayerProgression>();
            _upgrades = GetComponent<UpgradeBook>();
        }

        void OnEnable() => SlimeAI.OnAnyKilled += HandleKill;
        void OnDisable() => SlimeAI.OnAnyKilled -= HandleKill;

        public GuildData Data => Find(Guild);
        public GuildData Find(GuildId id)
        {
            foreach (var g in guilds) if (g != null && g.id == id) return g;
            return null;
        }

        public QuestData CurrentQuest
        {
            get
            {
                var d = Data;
                return d != null && QuestIndex >= 0 && QuestIndex < d.quests.Length ? d.quests[QuestIndex] : null;
            }
        }

        /// <summary>Attribut-Bonus durch den Rang (+2 je Rang)</summary>
        public StatBlock Bonus
        {
            get
            {
                var d = Data;
                if (d == null || Rank <= 0) return new StatBlock();
                int v = 2 * Rank;
                switch (d.attribute)
                {
                    case PlayerAttribute.Staerke: return new StatBlock { strength = v };
                    case PlayerAttribute.Geschick: return new StatBlock { dexterity = v };
                    case PlayerAttribute.Intelligenz: return new StatBlock { intelligence = v };
                    case PlayerAttribute.Vitalitaet: return new StatBlock { vitality = v };
                    default: return new StatBlock { luck = v };
                }
            }
        }

        // ------------------------------------------------------------ Beitreten
        public string Join(GuildId id)
        {
            var d = Find(id);
            if (d == null) return Loc.T("Unbekannte Gilde");
            if (Guild == id) return Loc.T("Du bist schon Mitglied");
            bool switching = Guild != GuildId.None;
            if (switching && (_wallet == null || !_wallet.CanAfford(SwitchCost))) return Loc.F("Wechsel kostet {0} Gold", SwitchCost);
            if (d.weapon != null && _inventory != null && _inventory.SpaceFor(d.weapon) <= 0) return Loc.T("Inventar voll");
            if (switching) _wallet.TrySpend(SwitchCost);
            Guild = id;
            Rank = 1;
            QuestIndex = 0;
            QuestActive = false;
            Progress = 0;
            if (d.weapon != null && _inventory != null) _inventory.Add(d.weapon, 1);
            Changed();
            return Loc.F("Willkommen in der {0}!", Loc.T(d.guildName));
        }

        // ------------------------------------------------------------ Quests
        public string Accept()
        {
            if (Guild == GuildId.None) return Loc.T("Erst beitreten");
            if (QuestActive) return Loc.T("Du hast schon eine Quest");
            if (CurrentQuest == null) return Loc.T("Alle Quests erledigt");
            QuestActive = true;
            Progress = 0;
            Changed();
            return Loc.F("Quest angenommen: {0}", Loc.T(CurrentQuest.title));
        }

        void HandleKill(string creature, bool bySpell)
        {
            var q = CurrentQuest;
            if (!QuestActive || q == null || q.kind == QuestKind.Bringen) return;
            if (q.kind == QuestKind.MitZauberBesiegen && !bySpell) return;
            if (q.target != "*" && q.target != creature) return;
            if (Progress >= q.amount) return;
            Progress++;
            Changed();
        }

        /// <summary>Fortschritt der aktiven Quest (Bringen: im Inventar vorhanden, gedeckelt)</summary>
        public int CurrentProgress
        {
            get
            {
                var q = CurrentQuest;
                if (!QuestActive || q == null) return 0;
                if (q.kind != QuestKind.Bringen) return Progress;
                int have = Mathf.Min(q.amount, Count(q.target));
                if (!string.IsNullOrEmpty(q.item2)) have += Mathf.Min(q.amount2, Count(q.item2));
                return have;
            }
        }

        public int Required
        {
            get
            {
                var q = CurrentQuest;
                if (q == null) return 0;
                return q.amount + (q.kind == QuestKind.Bringen && !string.IsNullOrEmpty(q.item2) ? q.amount2 : 0);
            }
        }

        public bool IsComplete => QuestActive && CurrentQuest != null && CurrentProgress >= Required;

        int Count(string id)
        {
            if (_inventory == null || string.IsNullOrEmpty(id)) return 0;
            int n = 0;
            foreach (var s in _inventory.slots) if (!s.IsEmpty && s.item.id == id) n += s.count;
            return n;
        }

        ItemData ItemById(string id)
        {
            if (_inventory == null) return null;
            foreach (var s in _inventory.slots) if (!s.IsEmpty && s.item.id == id) return s.item;
            return null;
        }

        public string TurnIn()
        {
            var q = CurrentQuest;
            if (!QuestActive || q == null) return Loc.T("Keine aktive Quest");
            if (!IsComplete) return Loc.F("Noch nicht fertig ({0}/{1})", CurrentProgress, Required);
            if (q.kind == QuestKind.Bringen)
            {
                _inventory.Remove(ItemById(q.target), q.amount);
                if (!string.IsNullOrEmpty(q.item2)) _inventory.Remove(ItemById(q.item2), q.amount2);
            }
            if (_wallet != null) _wallet.Add(q.gold);
            if (_progression != null) _progression.AddXP(q.xp);
            QuestIndex++;
            QuestActive = false;
            Progress = 0;
            string msg = Loc.F("Quest erledigt: +{0} Gold, +{1} EP", q.gold, q.xp);
            if (QuestIndex >= QuestsPerRank && Rank < 2)
            {
                Rank = 2;
                var w = Data != null ? Data.weapon : null;
                if (w != null && _upgrades != null && _upgrades.Level(w) < 2) _upgrades.SetLevel(w.id, 2);
                msg += Loc.T(" · Neuer Rang: Geselle!");
            }
            Changed();
            return msg;
        }

        /// <summary>Kurztext fuer das HUD, z. B. "Woelfe besiegen 2/3" (leer ohne aktive Quest)</summary>
        public string HudText => QuestActive && CurrentQuest != null ? $"{Loc.T(CurrentQuest.title)} {CurrentProgress}/{Required}" : "";

        /// <summary>Zustand aus dem Spielstand (Meilenstein 7c, Version 4)</summary>
        public void SetState(GuildId guild, int rank, int questIndex, bool active, int progress)
        {
            Guild = Find(guild) != null ? guild : GuildId.None;
            Rank = Guild == GuildId.None ? 0 : Mathf.Clamp(rank, 1, 2);
            QuestIndex = Mathf.Clamp(questIndex, 0, QuestsPerRank);
            QuestActive = Guild != GuildId.None && active && QuestIndex < QuestsPerRank;
            Progress = Mathf.Max(0, progress);
            Changed();
        }

        void Changed()
        {
            if (_stats != null) _stats.Recalculate(false);
            OnChanged?.Invoke();
        }
    }
}
