// Holzschwert ins Inventar legen und waehlen, waehrend das Eisenschwert angelegt ist
//#HEADER
var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.InventoryView>();
inv.Add(db.Get("holzschwert"), 1);
int idx = inv.IndexOf(db.Get("holzschwert"));
es.SetSelectedGameObject(view.gridSlots[idx].gameObject);
return "Platz=" + idx + " Detail=" + view.detail.statsText.text.Replace("\n", " / ");
