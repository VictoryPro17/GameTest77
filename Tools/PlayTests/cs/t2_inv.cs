//#HEADER
var sb = new System.Text.StringBuilder();
for (int i = 0; i < Reborn.Inventory.PlayerInventory.Size; i++)
{
    var sl = inv.slots[i];
    if (!sl.IsEmpty) sb.Append(i + ":" + sl.item.id + "x" + sl.count + " ");
}
return "Inventar [" + sb.ToString().Trim() + "]";
