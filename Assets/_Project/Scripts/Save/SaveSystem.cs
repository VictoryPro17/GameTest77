using System;
using System.IO;
using Reborn.Combat;
using Reborn.Inventory;
using Reborn.Loot;
using Reborn.Progression;
using Reborn.UI;
using Reborn.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reborn.Save
{
    /// <summary>
    /// Speichert und laedt den Spielstand als JSON in Application.persistentDataPath.
    /// Laden beim Start; Speichern bei Level-Aufstieg, Menue schliessen, Truhe oeffnen, App pausieren und App beenden.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class SaveSystem : MonoBehaviour
    {
        public const string FileName = "reborn_save.json";
        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public ItemDatabase database;

        public static event Action OnSaved;
        public static event Action OnLoaded;

        PlayerProgression _progression;
        PlayerStats _stats;
        PlayerInventory _inventory;
        PlayerEquipment _equipment;
        Health _health;
        Magic.SpellBook _spells;
        Magic.PlayerMana _mana;
        Economy.PlayerWallet _wallet;
        Economy.UpgradeBook _upgrades;
        Guilds.PlayerGuild _guild;
        readonly System.Collections.Generic.HashSet<string> _knownOpenChests = new System.Collections.Generic.HashSet<string>();
        readonly System.Collections.Generic.HashSet<string> _defeatedBosses = new System.Collections.Generic.HashSet<string>();

        /// <summary>Ist dieser Boss im Spielstand als besiegt vermerkt? (Meilenstein 8)</summary>
        public bool IsBossDefeated(string id) => _defeatedBosses.Contains(id);

        void HandleBossDefeated(string id)
        {
            _defeatedBosses.Add(id);
            Save();
        }
        bool _suppress;

        void Awake()
        {
            _wallet = GetComponent<Economy.PlayerWallet>();
            _upgrades = GetComponent<Economy.UpgradeBook>();
            _guild = GetComponent<Guilds.PlayerGuild>();
            _progression = GetComponent<PlayerProgression>();
            _stats = GetComponent<PlayerStats>();
            _inventory = GetComponent<PlayerInventory>();
            _equipment = GetComponent<PlayerEquipment>();
            _health = GetComponent<Health>();
            _spells = GetComponent<Magic.SpellBook>();
            _mana = GetComponent<Magic.PlayerMana>();
        }

        void OnEnable()
        {
            if (_progression != null) _progression.OnLevelUp += HandleLevelUp;
            TreasureChest.OnAnyOpened += HandleChestOpened;
            GameMenu.OnClosed += Save;
            Enemies.BossAI.OnDefeated += HandleBossDefeated;
        }

        void OnDisable()
        {
            if (_progression != null) _progression.OnLevelUp -= HandleLevelUp;
            TreasureChest.OnAnyOpened -= HandleChestOpened;
            GameMenu.OnClosed -= Save;
            Enemies.BossAI.OnDefeated -= HandleBossDefeated;
        }

        void Start()
        {
            Load();
        }

        void HandleLevelUp(int level) => Save();
        void HandleChestOpened(TreasureChest chest) => Save();

        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }

        public static bool Exists => File.Exists(SavePath);

        public void Save()
        {
            if (_suppress || _progression == null) return;
            var data = new SaveData
            {
                level = _progression.Level,
                xp = _progression.XP,
                attributes = _stats != null ? (int[])_stats.baseAttributes.Clone() : new[] { 1, 1, 1, 1, 1 },
                unspentPoints = _stats != null ? _stats.UnspentPoints : 0,
                currentHealth = _health != null ? _health.Current : 0
            };
            if (_inventory != null)
                for (int i = 0; i < PlayerInventory.Size; i++)
                {
                    var slot = _inventory.slots[i];
                    if (!slot.IsEmpty) data.inventory.Add(new SaveData.SlotData { index = i, id = slot.item.id, count = slot.count });
                }
            if (_equipment != null)
                for (int i = 0; i < PlayerEquipment.SlotCount; i++)
                {
                    var item = _equipment.Get(i);
                    if (item != null) data.equipment.Add(new SaveData.EquipData { slot = i, id = item.id });
                }
            // Offene Truhen: aus der Szene plus die aus anderen Szenen (Meilenstein 6: Basis und Kampfraum)
            foreach (var chest in FindObjectsByType<TreasureChest>())
                if (chest.IsOpen) _knownOpenChests.Add(chest.chestId);
            data.openedChests.AddRange(_knownOpenChests);
            data.defeatedBosses.AddRange(_defeatedBosses);   // Meilenstein 8
            if (_mana != null)
            {
                data.manaLevel = _mana.Level;
                data.manaXp = _mana.XP;
                data.currentMana = _mana.Current;
            }
            if (_spells != null)
                for (int i = 0; i < Magic.SpellBook.SlotCount; i++)
                {
                    var spell = _spells.Get(i);
                    if (spell != null) data.spells.Add(new SaveData.SpellSave { id = spell.id, level = _spells.Level(i), xp = _spells.XP(i) });
                }
            if (_wallet != null) data.gold = _wallet.Gold;
            if (_upgrades != null)
                foreach (var kv in _upgrades.All) data.upgrades.Add(new SaveData.UpgradeSave { id = kv.Key, level = kv.Value });
            data.lastScene = SceneManager.GetActiveScene().name;
            if (_guild != null && _guild.Guild != Guilds.GuildId.None)
            {
                data.guild = _guild.Guild.ToString();
                data.guildRank = _guild.Rank;
                data.questIndex = _guild.QuestIndex;
                data.questActive = _guild.QuestActive;
                data.questProgress = _guild.Progress;
            }

            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
                OnSaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand konnte nicht gespeichert werden: " + e.Message);
            }
        }

        public bool Load()
        {
            if (!Exists) return false;
            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand konnte nicht gelesen werden: " + e.Message);
                return false;
            }
            if (data == null) return false;

            if (_progression != null) _progression.SetState(data.level, data.xp);
            if (_stats != null) _stats.SetState(data.attributes, data.unspentPoints);

            if (_inventory != null)
            {
                for (int i = 0; i < PlayerInventory.Size; i++) _inventory.SetSlot(i, null, 0);
                foreach (var slot in data.inventory)
                    if (slot != null) _inventory.SetSlot(slot.index, database != null ? database.Get(slot.id) : null, slot.count);
            }
            if (_equipment != null)
            {
                for (int i = 0; i < PlayerEquipment.SlotCount; i++) _equipment.SetEquipped(i, null);
                foreach (var e in data.equipment)
                    if (e != null) _equipment.SetEquipped(e.slot, database != null ? database.Get(e.id) : null);
            }

            // Ab Version 3: Gold und Schmiede-Stufen (vor dem Neuberechnen der Werte)
            if (_wallet != null) _wallet.SetGold(data.gold);
            if (_upgrades != null)
            {
                var levels = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, int>>();
                if (data.upgrades != null)
                    foreach (var u in data.upgrades)
                        if (u != null) levels.Add(new System.Collections.Generic.KeyValuePair<string, int>(u.id, u.level));
                _upgrades.SetAll(levels);
            }

            // Ab Version 4: Gilde und Quest (aeltere Staende: keine Gilde)
            if (_guild != null)
            {
                var g = System.Enum.TryParse(data.guild, out Guilds.GuildId parsed) ? parsed : Guilds.GuildId.None;
                _guild.SetState(g, data.guildRank, data.questIndex, data.questActive, data.questProgress);
            }

            if (_stats != null) _stats.Recalculate(false);
            if (_health != null)
            {
                int max = _health.maxHealth;
                _health.SetCurrent(data.currentHealth > 0 ? Mathf.Min(data.currentHealth, max) : max);
            }
            if (_inventory != null) _inventory.NotifyChanged();
            if (_equipment != null) _equipment.NotifyChanged();

            _knownOpenChests.Clear();
            if (data.openedChests != null) _knownOpenChests.UnionWith(data.openedChests);
            foreach (var chest in FindObjectsByType<TreasureChest>())
                if (_knownOpenChests.Contains(chest.chestId)) chest.SetOpenedSilently();
            // Ab Version 5: besiegte Bosse erscheinen nicht wieder
            _defeatedBosses.Clear();
            if (data.defeatedBosses != null) _defeatedBosses.UnionWith(data.defeatedBosses);
            foreach (var boss in FindObjectsByType<Enemies.BossAI>())
                if (_defeatedBosses.Contains(boss.bossId)) boss.RemoveDefeated();

            // Ab Version 2: Zauber und Mana (bei aelteren Dateien bleiben die Startwerte)
            if (_spells != null && data.spells != null)
                foreach (var s in data.spells)
                {
                    int slot = s != null ? _spells.IndexOf(s.id) : -1;
                    if (slot >= 0) _spells.SetState(slot, s.level, s.xp);
                }
            if (_mana != null) _mana.SetState(data.manaLevel, data.manaXp, data.currentMana);

            OnLoaded?.Invoke();
            return true;
        }

        /// <summary>Loescht den Spielstand und startet mit Startwerten neu, immer in der Basis (Meilenstein 6).</summary>
        public void DeleteAndRestart()
        {
            _suppress = true;
            if (Exists) File.Delete(SavePath);
            _knownOpenChests.Clear();
            _defeatedBosses.Clear();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneTravel.BaseScene);
        }
    }
}
