using System;
using System.Collections.Generic;
using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.Economy
{
    /// <summary>
    /// Verbesserungs-Stufen beim Schmied (Meilenstein 6): +1 bis +5 je Gegenstandsart (ID).
    /// Waffe: Angriff +1 pro Stufe; Ruestung und Schild: Verteidigung +1 pro Stufe; Schmuck nicht verbesserbar.
    /// Kosten fuer die naechste Stufe n: Verkaufswert x n x 3 Gold, ab n = 3 zusaetzlich n x 2 Schleimgel.
    /// </summary>
    public class UpgradeBook : MonoBehaviour
    {
        public const int MaxLevel = 5;
        public const string MaterialId = "schleimgel";

        readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

        public event Action OnChanged;

        public enum Result { Ok, NotUpgradable, MaxLevel, NotEnoughGold, NotEnoughMaterial }

        public static bool CanUpgrade(ItemData item) =>
            item != null && item.IsEquipment && item.equipSlot != EquipSlot.Ring && item.equipSlot != EquipSlot.Halskette;

        public int Level(ItemData item) => item != null ? Level(item.id) : 0;
        public int Level(string id) => id != null && _levels.TryGetValue(id, out int l) ? l : 0;

        /// <summary>Zusatzwerte durch die Stufe dieses Gegenstands</summary>
        public StatBlock Bonus(ItemData item)
        {
            int level = Level(item);
            if (level <= 0 || !CanUpgrade(item)) return new StatBlock();
            return item.equipSlot == EquipSlot.Hauptwaffe ? new StatBlock { attack = level } : new StatBlock { defense = level };
        }

        /// <summary>Gold fuer die naechste Stufe (0 = nicht moeglich)</summary>
        public int GoldCost(ItemData item)
        {
            int next = Level(item) + 1;
            if (!CanUpgrade(item) || next > MaxLevel) return 0;
            return Mathf.Max(1, item.sellValue) * next * 3;
        }

        /// <summary>Schleimgel fuer die naechste Stufe</summary>
        public int MaterialCost(ItemData item)
        {
            int next = Level(item) + 1;
            if (!CanUpgrade(item) || next > MaxLevel) return 0;
            return next >= 3 ? next * 2 : 0;
        }

        public Result Check(ItemData item, PlayerWallet wallet, PlayerInventory inventory, ItemData material)
        {
            if (!CanUpgrade(item)) return Result.NotUpgradable;
            if (Level(item) >= MaxLevel) return Result.MaxLevel;
            if (wallet == null || !wallet.CanAfford(GoldCost(item))) return Result.NotEnoughGold;
            int need = MaterialCost(item);
            if (need > 0 && (inventory == null || material == null || inventory.Count(material) < need)) return Result.NotEnoughMaterial;
            return Result.Ok;
        }

        /// <summary>Naechste Stufe kaufen: Gold und Material abziehen, Stufe erhoehen, Werte neu berechnen.</summary>
        public Result TryUpgrade(ItemData item, PlayerWallet wallet, PlayerInventory inventory, ItemData material)
        {
            var result = Check(item, wallet, inventory, material);
            if (result != Result.Ok) return result;
            int need = MaterialCost(item);
            wallet.TrySpend(GoldCost(item));
            if (need > 0) inventory.Remove(material, need);
            _levels[item.id] = Level(item) + 1;
            OnChanged?.Invoke();
            var equipment = GetComponent<PlayerEquipment>();
            if (equipment != null) equipment.NotifyChanged();
            return Result.Ok;
        }

        public static string Message(Result result)
        {
            switch (result)
            {
                case Result.Ok: return "Verbessert!";
                case Result.NotUpgradable: return "Kann nicht verbessert werden";
                case Result.MaxLevel: return "Höchste Stufe erreicht";
                case Result.NotEnoughGold: return "Nicht genug Gold";
                default: return "Nicht genug Schleimgel";
            }
        }

        /// <summary>Stufe direkt setzen (Gilden-Rang, Meilenstein 7c)</summary>
        public void SetLevel(string id, int level)
        {
            if (string.IsNullOrEmpty(id)) return;
            _levels[id] = Mathf.Clamp(level, 0, MaxLevel);
            OnChanged?.Invoke();
            var equipment = GetComponent<PlayerEquipment>();
            if (equipment != null) equipment.NotifyChanged();
        }

        /// <summary>Alle Stufen (Spielstand speichern)</summary>
        public IEnumerable<KeyValuePair<string, int>> All => _levels;

        /// <summary>Stufen direkt setzen (Spielstand laden). Ruft OnChanged auf.</summary>
        public void SetAll(IEnumerable<KeyValuePair<string, int>> levels)
        {
            _levels.Clear();
            if (levels != null)
                foreach (var kv in levels)
                    if (!string.IsNullOrEmpty(kv.Key) && kv.Value > 0) _levels[kv.Key] = Mathf.Min(MaxLevel, kv.Value);
            OnChanged?.Invoke();
        }
    }
}
