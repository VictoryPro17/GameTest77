using Reborn.Economy;
using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Haendler (Meilenstein 6): Reiter "Kaufen" (Waren) und "Verkaufen" (eigenes Inventar), Details rechts.</summary>
    public class ShopView : MonoBehaviour
    {
        public Button buyTab;
        public Button sellTab;
        /// <summary>24 Plaetze; beim Kaufen die Waren, beim Verkaufen das Inventar</summary>
        public ItemSlotView[] slots = new ItemSlotView[PlayerInventory.Size];
        public ItemDetailView detail;
        public Color tabActive = new Color(1f, 0.93f, 0.72f, 1f);
        public Color tabInactive = new Color(0.5f, 0.48f, 0.5f, 1f);

        public bool SellMode { get; private set; }

        TradeWindow _window;
        ShopData _shop;
        PlayerInventory _inventory;
        PlayerEquipment _equipment;
        PlayerWallet _wallet;
        bool _setup;

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
            }
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].index = i;
                slots[i].Selected += HandleSelected;
                slots[i].Submitted += HandleSubmitted;
            }
            if (buyTab != null) buyTab.onClick.AddListener(() => ShowTab(false));
            if (sellTab != null) sellTab.onClick.AddListener(() => ShowTab(true));
        }

        public void Open(TradeWindow window, ShopData shop)
        {
            Setup();
            _window = window;
            _shop = shop;
            ShowTab(false);
        }

        public void ShowTab(bool sell)
        {
            Setup();
            SellMode = sell;
            SetTabColor(buyTab, !sell);
            SetTabColor(sellTab, sell);
            Refresh();
            if (detail != null) detail.Clear();
        }

        public void SelectFirst()
        {
            if (EventSystem.current == null) return;
            foreach (var s in slots)
                if (s != null && s.Item != null) { EventSystem.current.SetSelectedGameObject(s.gameObject); return; }
            if (buyTab != null) EventSystem.current.SetSelectedGameObject(SellMode && sellTab != null ? sellTab.gameObject : buyTab.gameObject);
        }

        void Refresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                if (SellMode)
                {
                    var s = _inventory != null ? _inventory.slots[i] : null;
                    slots[i].Show(s != null && !s.IsEmpty ? s.item : null, s != null ? s.count : 0);
                }
                else
                {
                    var item = _shop != null && i < _shop.items.Count ? _shop.items[i] : null;
                    slots[i].Show(item, item != null ? 1 : 0);
                }
            }
        }

        /// <summary>Gegenstand an einem Platz (Kaufen: Ware, Verkaufen: Inventar)</summary>
        public ItemData ItemAt(int index)
        {
            if (SellMode) return _inventory != null && index >= 0 && index < PlayerInventory.Size && !_inventory.slots[index].IsEmpty ? _inventory.slots[index].item : null;
            return _shop != null && index >= 0 && index < _shop.items.Count ? _shop.items[index] : null;
        }

        void HandleSelected(ItemSlotView view)
        {
            var item = ItemAt(view.index);
            if (item == null || detail == null)
            {
                if (detail != null) detail.Clear();
                return;
            }
            int index = view.index;
            if (SellMode)
            {
                int count = _inventory.slots[index].count;
                detail.Show(item, count, _equipment != null ? _equipment.WouldReplace(item) : null, item.IsEquipment,
                    $"Verkaufen (+{item.sellValue})", () => Sell(index));
            }
            else
            {
                detail.Show(item, 1, _equipment != null ? _equipment.WouldReplace(item) : null, item.IsEquipment,
                    $"Kaufen ({item.buyValue})", () => Buy(item));
                if (detail.sellText != null) detail.sellText.text = $"Preis: {item.buyValue} Gold";
            }
        }

        void HandleSubmitted(ItemSlotView view)
        {
            var item = ItemAt(view.index);
            if (item == null) return;
            if (SellMode) Sell(view.index);
            else Buy(item);
        }

        /// <summary>Ein Stueck kaufen. Rueckgabe: Meldung</summary>
        public string Buy(ItemData item)
        {
            string result;
            if (item == null || _wallet == null || _inventory == null) result = "Nichts ausgewählt";
            else if (!_wallet.CanAfford(item.buyValue)) result = "Nicht genug Gold";
            else if (_inventory.SpaceFor(item) <= 0) result = "Inventar voll";
            else
            {
                _wallet.TrySpend(item.buyValue);
                _inventory.Add(item, 1);
                result = "Gekauft: " + item.itemName;
                if (_window != null) _window.SaveNow();
            }
            if (_window != null) _window.ShowMessage(result);
            Refresh();
            return result;
        }

        /// <summary>Ein Stueck aus einem Inventarplatz verkaufen. Rueckgabe: Meldung</summary>
        public string Sell(int index)
        {
            string result;
            if (_inventory == null || _wallet == null || index < 0 || index >= PlayerInventory.Size || _inventory.slots[index].IsEmpty)
                result = "Nichts ausgewählt";
            else
            {
                var item = _inventory.slots[index].item;
                _inventory.RemoveAt(index, 1);
                _wallet.Add(item.sellValue);
                result = $"Verkauft: {item.itemName} (+{item.sellValue} Gold)";
                if (_window != null) _window.SaveNow();
                if (_inventory.slots[index].IsEmpty && detail != null) detail.Clear();
            }
            if (_window != null) _window.ShowMessage(result);
            Refresh();
            return result;
        }

        void SetTabColor(Button button, bool active)
        {
            if (button == null) return;
            var img = button.targetGraphic as Image;
            if (img != null) img.color = active ? tabActive : tabInactive;
        }
    }
}
