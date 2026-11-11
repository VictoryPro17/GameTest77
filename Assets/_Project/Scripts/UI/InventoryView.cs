using System.Collections.Generic;
using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Reiter "Inventar": Ausruestung, Raster 6 x 4 mit Filter und Sortieren, Details.</summary>
    public class InventoryView : MonoBehaviour
    {
        public ItemSlotView[] gridSlots = new ItemSlotView[PlayerInventory.Size];
        public EquipmentView equipmentView;
        public ItemDetailView detail;
        public Button sortButton;
        /// <summary>Alle, Ausruestung, Verbrauch, Material</summary>
        public Button[] filterButtons = new Button[4];
        public Text messageText;
        public Color filterActive = new Color(0.85f, 0.7f, 0.3f, 1f);
        public Color filterInactive = new Color(0.25f, 0.23f, 0.3f, 1f);

        public int Filter { get; private set; }

        PlayerInventory _inventory;
        PlayerEquipment _equipment;
        PotionUser _potions;
        readonly List<int> _viewToIndex = new List<int>();
        ItemSlotView _current;
        float _messageTimer;
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
                _potions = player.GetComponent<PotionUser>();
            }
            for (int i = 0; i < gridSlots.Length; i++)
            {
                if (gridSlots[i] == null) continue;
                gridSlots[i].index = i;
                gridSlots[i].Selected += HandleSelected;
                gridSlots[i].Submitted += HandleSubmitted;
            }
            if (equipmentView != null) equipmentView.Setup(HandleSelected, HandleSubmitted);
            if (sortButton != null) sortButton.onClick.AddListener(() => { if (_inventory != null) _inventory.Sort(); });
            for (int i = 0; i < filterButtons.Length; i++)
            {
                int f = i;
                if (filterButtons[i] != null) filterButtons[i].onClick.AddListener(() => SetFilter(f));
            }
        }

        void OnEnable()
        {
            Setup();
            if (_inventory != null) _inventory.OnChanged += Refresh;
            if (_equipment != null) _equipment.OnChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (_inventory != null) _inventory.OnChanged -= Refresh;
            if (_equipment != null) _equipment.OnChanged -= Refresh;
        }

        void Update()
        {
            if (_messageTimer > 0f)
            {
                _messageTimer -= Time.unscaledDeltaTime;
                if (_messageTimer <= 0f && messageText != null) messageText.text = "";
            }
        }

        static bool Matches(ItemData item, int filter)
        {
            switch (filter)
            {
                case 1: return item.category == ItemCategory.Ausruestung;
                case 2: return item.category == ItemCategory.Verbrauch;
                case 3: return item.category == ItemCategory.Material;
                default: return true;
            }
        }

        public void SetFilter(int filter)
        {
            Filter = Mathf.Clamp(filter, 0, 3);
            Refresh();
        }

        /// <summary>Inventarplatz, der im Raster an dieser Stelle angezeigt wird (-1 = leer).</summary>
        public int InventoryIndexAt(int viewIndex) => viewIndex >= 0 && viewIndex < _viewToIndex.Count ? _viewToIndex[viewIndex] : -1;

        public int VisibleItemCount
        {
            get
            {
                int n = 0;
                foreach (int i in _viewToIndex) if (i >= 0) n++;
                return n;
            }
        }

        public void Refresh()
        {
            if (_inventory == null) return;
            _viewToIndex.Clear();
            if (Filter == 0)
            {
                for (int i = 0; i < PlayerInventory.Size; i++) _viewToIndex.Add(i);
            }
            else
            {
                for (int i = 0; i < PlayerInventory.Size; i++)
                {
                    var s = _inventory.slots[i];
                    if (!s.IsEmpty && Matches(s.item, Filter)) _viewToIndex.Add(i);
                }
                while (_viewToIndex.Count < PlayerInventory.Size) _viewToIndex.Add(-1);
            }
            for (int v = 0; v < gridSlots.Length; v++)
            {
                if (gridSlots[v] == null) continue;
                int index = InventoryIndexAt(v);
                var slot = index >= 0 ? _inventory.slots[index] : null;
                gridSlots[v].Show(slot != null && !slot.IsEmpty ? slot.item : null, slot != null ? slot.count : 0);
            }
            if (equipmentView != null) equipmentView.Refresh(_equipment);
            for (int i = 0; i < filterButtons.Length; i++)
            {
                if (filterButtons[i] == null) continue;
                var img = filterButtons[i].targetGraphic as Image;
                if (img != null) img.color = i == Filter ? filterActive : filterInactive;
            }
            ShowDetail(_current);
        }

        public void SelectFirst()
        {
            if (EventSystem.current != null && gridSlots.Length > 0 && gridSlots[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(gridSlots[0].gameObject);
            }
        }

        void HandleSelected(ItemSlotView view)
        {
            _current = view;
            ShowDetail(view);
        }

        void HandleSubmitted(ItemSlotView view)
        {
            _current = view;
            DoPrimary(view);
        }

        void ShowDetail(ItemSlotView view)
        {
            if (detail == null) return;
            if (view == null || _inventory == null)
            {
                detail.Clear();
                return;
            }
            if (view.isEquipSlot)
            {
                var item = _equipment != null ? _equipment.Get(view.index) : null;
                if (item == null) { detail.Clear(); return; }
                detail.Show(item, 1, null, false, "Ablegen", () => DoPrimaryAndReselect(view));
                return;
            }
            int index = InventoryIndexAt(view.index);
            var slot = index >= 0 ? _inventory.slots[index] : null;
            if (slot == null || slot.IsEmpty) { detail.Clear(); return; }
            if (slot.item.IsEquipment)
                detail.Show(slot.item, slot.count, _equipment != null ? _equipment.WouldReplace(slot.item) : null, true, "Anlegen", () => DoPrimaryAndReselect(view));
            else if (slot.item.IsUsable)
                detail.Show(slot.item, slot.count, null, false, "Benutzen", () => DoPrimaryAndReselect(view));
            else
                detail.Show(slot.item, slot.count, null, false, null, null);
        }

        /// <summary>Aktion ueber den Knopf im Detail-Fenster: danach wieder den Platz auswaehlen,
        /// damit Controller und Tastatur nicht an einem ausgeblendeten Knopf haengen bleiben.</summary>
        void DoPrimaryAndReselect(ItemSlotView view)
        {
            DoPrimary(view);
            if (view != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(view.gameObject);
            }
        }

        /// <summary>Anlegen, Benutzen oder Ablegen fuer den Platz.</summary>
        public void DoPrimary(ItemSlotView view)
        {
            if (view == null || _inventory == null) return;
            if (view.isEquipSlot)
            {
                if (_equipment != null && _equipment.Get(view.index) != null && !_equipment.Unequip(view.index))
                    ShowMessage("Inventar ist voll");
                return;
            }
            int index = InventoryIndexAt(view.index);
            if (index < 0) return;
            var slot = _inventory.slots[index];
            if (slot.IsEmpty) return;
            if (slot.item.IsEquipment && _equipment != null)
            {
                if (!_equipment.Equip(index)) ShowMessage("Kann nicht angelegt werden");
            }
            else if (slot.item.IsUsable && _potions != null)
            {
                if (!_potions.UseAt(index)) ShowMessage("Jetzt nicht nötig");
            }
        }

        void ShowMessage(string text)
        {
            if (messageText == null) return;
            messageText.text = text;
            _messageTimer = 2f;
        }
    }
}
