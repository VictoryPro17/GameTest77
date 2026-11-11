using Reborn.Inventory;
using UnityEngine;

namespace Reborn.UI
{
    /// <summary>Die 10 Ausruestungsplaetze links im Inventar.</summary>
    public class EquipmentView : MonoBehaviour
    {
        /// <summary>In der Reihenfolge von EquipSlots.Positions</summary>
        public ItemSlotView[] slots = new ItemSlotView[PlayerEquipment.SlotCount];

        public void Setup(System.Action<ItemSlotView> selected, System.Action<ItemSlotView> submitted)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].index = i;
                slots[i].isEquipSlot = true;
                slots[i].Selected += selected;
                slots[i].Submitted += submitted;
            }
        }

        public void Refresh(PlayerEquipment equipment)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].Show(equipment != null ? equipment.Get(i) : null, 1);
        }
    }
}
