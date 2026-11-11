using System;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Ein Platz im Inventar oder in der Ausruestung. Auswaehlen (Klick, Tippen, Stick) zeigt die Details,
    /// Bestaetigen (Enter / Sued-Taste) fuehrt die Hauptaktion aus.
    /// </summary>
    public class ItemSlotView : Selectable, ISubmitHandler
    {
        public Image frame;
        public Image icon;
        public Image placeholder;
        public Image selection;
        public Text countText;
        public Sprite emptyFrame;
        /// <summary>Rahmen je Seltenheit: Gewoehnlich, Selten, Episch, Legendaer</summary>
        public Sprite[] rarityFrames = new Sprite[4];

        [NonSerialized] public int index;
        [NonSerialized] public bool isEquipSlot;

        public event Action<ItemSlotView> Selected;
        public event Action<ItemSlotView> Submitted;

        public ItemData Item { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (selection != null) selection.enabled = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (selection != null) selection.enabled = false;
        }

        public void Show(ItemData item, int count)
        {
            Item = item;
            bool has = item != null && count > 0;
            if (icon != null)
            {
                icon.enabled = has && item.icon != null;
                icon.sprite = has ? item.icon : null;
            }
            if (placeholder != null) placeholder.enabled = !has;
            if (countText != null) countText.text = has && count > 1 ? count.ToString() : "";
            if (frame != null)
            {
                int r = has ? (int)item.rarity : -1;
                frame.sprite = r >= 0 && r < rarityFrames.Length && rarityFrames[r] != null ? rarityFrames[r] : emptyFrame;
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (selection != null) selection.enabled = true;
            Selected?.Invoke(this);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (selection != null) selection.enabled = false;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Submitted?.Invoke(this);
        }
    }
}
