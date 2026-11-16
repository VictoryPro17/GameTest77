//#HEADER
var sb = new System.Text.StringBuilder();
sb.Append("Lv" + prog.Level + " XP" + prog.XP + " Attr[" + string.Join(",", st.baseAttributes) + "] frei" + st.UnspentPoints + " HP" + hp.Current + "/" + hp.maxHealth + " | Inv ");
for (int i = 0; i < Reborn.Inventory.PlayerInventory.Size; i++) { var sl = inv.slots[i]; if (!sl.IsEmpty) sb.Append(i + ":" + sl.item.id + "x" + sl.count + " "); }
sb.Append("| Ausr ");
for (int i = 0; i < Reborn.Inventory.PlayerEquipment.SlotCount; i++) { var it = eq.Get(i); if (it != null) sb.Append(i + ":" + it.id + " "); }
var chest = chestById("room_test_truhe_1");
sb.Append("| Truhe offen=" + chest.IsOpen);
return sb.ToString();
