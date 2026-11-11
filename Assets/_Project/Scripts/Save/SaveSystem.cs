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
        bool _suppress;

        void Awake()
        {
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
        }

        void OnDisable()
        {
            if (_progression != null) _progression.OnLevelUp -= HandleLevelUp;
            TreasureChest.OnAnyOpened -= HandleChestOpened;
            GameMenu.OnClosed -= Save;
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
            foreach (var chest in FindObjectsByType<TreasureChest>())
                if (chest.IsOpen) data.openedChests.Add(chest.chestId);
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

            if (_stats != null) _stats.Recalculate(false);
            if (_health != null)
            {
                int max = _health.maxHealth;
                _health.SetCurrent(data.currentHealth > 0 ? Mathf.Min(data.currentHealth, max) : max);
            }
            if (_inventory != null) _inventory.NotifyChanged();
            if (_equipment != null) _equipment.NotifyChanged();

            foreach (var chest in FindObjectsByType<TreasureChest>())
                if (data.openedChests.Contains(chest.chestId)) chest.SetOpenedSilently();

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

        /// <summary>Loescht den Spielstand und startet die Szene mit Startwerten neu.</summary>
        public void DeleteAndRestart()
        {
            _suppress = true;
            if (Exists) File.Delete(SavePath);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
