//#HEADER
var order = "";
for (int i = 0; i < 5; i++) order += (i > 0 ? "," : "") + (inv.slots[i].IsEmpty ? "-" : inv.slots[i].item.id);
return "Sortiert: " + order;
