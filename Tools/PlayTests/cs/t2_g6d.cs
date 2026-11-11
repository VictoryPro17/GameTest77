//#HEADER
var pk = UnityEngine.GameObject.Find("TestPickupVoll");
if (pk != null) UnityEngine.Object.Destroy(pk);
inv.Clear();
inv.Add(db.Get("schleimgel"), 5);
inv.Add(db.Get("heiltrank_klein"), 2);
inv.Add(db.Get("holzschwert"), 1);
inv.Add(db.Get("kupferring"), 1);
inv.Add(db.Get("heiltrank"), 1);
inv.Add(db.Get("lederhose"), 1);
inv.Sort();
var order = "";
for (int i = 0; i < 6; i++) order += (inv.slots[i].IsEmpty ? "-" : inv.slots[i].item.itemName) + ", ";
return "Sortiert: " + order + "Platz6leer=" + inv.slots[6].IsEmpty;
