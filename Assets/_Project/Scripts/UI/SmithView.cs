using System.Collections.Generic;
using Reborn.Economy;
using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Schmied (Meilenstein 6): alle verbesserbaren Teile (angelegt und im Inventar, jede Art einmal),
    /// Details mit Stufe und Kosten, Knopf "Verbessern".
    /// </summary>
    public class SmithView : MonoBehaviour
    {
        /// <summary>18 Plaetze (es gibt 14 verbesserbare Arten)</summary>
        public ItemSlotView[] slots = new ItemSlotView[18];
        public ItemDetailView detail;
        /// <summary>"Stufe +1 -> +2", Kosten</summary>
        public Text levelText;
        /// <summary>Schleimgel (ab Stufe 3 noetig)</summary>
        public ItemData material;

        readonly List<ItemData> _items = new List<ItemData>();
        TradeWindow _window;
        PlayerInventory _inventory;
        PlayerEquipment _equipment;
        PlayerWallet _wallet;
        UpgradeBook _book;
        ItemData _selected;
        bool _setup;

        public IReadOnlyList<ItemData> Items => _items;

        void Setup()
        {
            if (_setup) return;
            _setup = true;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _inventory = player.GetComponent<PlayerInventory>();
                _equipment = player.GetComponent<PlayerEquipment>();
                _wallet = player.GetComponent<PlayerWallet>();
                _book = player.GetComponent<UpgradeBook>();
            }
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].index = i;
                slots[i].Selected += HandleSelected;
                slots[i].Submitted += v => { if (v.index < _items.Count) Upgrade(_items[v.index]); };
            }
        }

        public void Open(TradeWindow window)
        {
            Setup();
            _window = window;
            _selected = null;
            Refresh();
            if (detail != null) detail.Clear();
            if (levelText != null) levelText.text = "";
        }

        public void SelectFirst()
        {
            if (EventSystem.current == null) return;
            if (_items.Count > 0 && slots.Length > 0 && slots[0] != null) EventSystem.current.SetSelectedGameObject(slots[0].gameObject);
        }

        void Refresh()
        {
            _items.Clear();
            if (_equipment != null)
                for (int i = 0; i < PlayerEquipment.SlotCount; i++)
                {
                    var item = _equipment.Get(i);
                    if (UpgradeBook.CanUpgrade(item) && !_items.Contains(item)) _items.Add(item);
                }
            if (_inventory != null)
                foreach (var s in _inventory.slots)
                    if (!s.IsEmpty && UpgradeBook.CanUpgrade(s.item) && !_items.Contains(s.item)) _items.Add(s.item);
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].Show(i < _items.Count ? _items[i] : null, i < _items.Count ? 1 : 0);
            if (_selected != null) ShowDetail(_selected);
        }

        void HandleSelected(ItemSlotView view)
        {
            if (view.index >= _items.Count)
            {
                _selected = null;
                if (detail != null) detail.Clear();
                if (levelText != null) levelText.text = "";
                return;
            }
            ShowDetail(_items[view.index]);
        }

        void ShowDetail(ItemData item)
        {
            _selected = item;
            if (detail == null || _book == null) return;
            int level = _book.Level(item);
            bool max = level >= UpgradeBook.MaxLevel;
            detail.Show(item, 1, null, false, max ? null : "Verbessern", max ? null : () => Upgrade(item));
            if (detail.nameText != null) detail.nameText.text = ItemDetailView.DisplayName(item);
            if (detail.sellText != null) detail.sellText.text = "";
            if (levelText != null)
            {
                if (max) levelText.text = $"Stufe +{level} (höchste Stufe)";
                else
                {
                    int gel = _book.MaterialCost(item);
                    string mat = gel > 0 ? $" + {gel} Schleimgel" : "";
                    levelText.text = $"Stufe +{level} → +{level + 1}\nKosten: {_book.GoldCost(item)} Gold{mat}";
                }
            }
        }

        /// <summary>Naechste Stufe kaufen. Rueckgabe: Meldung</summary>
        public string Upgrade(ItemData item)
        {
            if (_book == null) return "kein Schmied";
            var result = _book.TryUpgrade(item, _wallet, _inventory, material);
            string text = UpgradeBook.Message(result);
            if (result == UpgradeBook.Result.Ok)
            {
                text = $"{ItemDetailView.DisplayName(item)}!";
                if (_window != null) _window.SaveNow();
            }
            if (_window != null) _window.ShowMessage(text);
            Refresh();
            return text;
        }
    }
}
