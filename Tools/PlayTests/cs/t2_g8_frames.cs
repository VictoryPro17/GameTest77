// Wie viele Plaetze zeigen gerade den Auswahlrahmen?
var names = "";
int count = 0;
foreach (var v in UnityEngine.Object.FindObjectsByType<Reborn.UI.ItemSlotView>())
    if (v.selection != null && v.selection.enabled) { count++; names += v.name + " "; }
return "Rahmen=" + count + " (" + names.Trim() + ")";
