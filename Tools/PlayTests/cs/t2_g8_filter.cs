var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.InventoryView>();
var r = "";
for (int f = 1; f <= 3; f++) { view.SetFilter(f); r += "Filter" + f + "=" + view.VisibleItemCount + " "; }
view.SetFilter(0);
return r.Trim();
