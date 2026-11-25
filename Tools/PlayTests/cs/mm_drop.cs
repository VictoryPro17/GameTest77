// Wegwerfen: give:<id>:<n> | open | drop1:<id> | dropall:<id> | equipdrop | state:<id>
//#HEADER
var gm = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>(UnityEngine.FindObjectsInactive.Include);
var parts = "%ACT%".Split(':');
string last = "-";
Reborn.Loot.ItemData item = parts.Length > 1 ? db.Get(parts[1]) : null;
if (parts.Length > 1 && item == null) return "kein Gegenstand " + parts[1];
switch (parts[0])
{
    case "give":
        inv.Add(item, int.Parse(parts[2]));
        break;
    case "open":
        gm.Open();
        gm.inventoryView.Refresh();
        break;
    case "drop1":
    case "dropall":
        Reborn.UI.ItemSlotView found = null;
        foreach (var sv in gm.inventoryView.gridSlots)
        {
            int ix = gm.inventoryView.InventoryIndexAt(sv.index);
            if (ix >= 0 && !inv.slots[ix].IsEmpty && inv.slots[ix].item == item) { found = sv; break; }
        }
        if (found == null) { last = "False"; break; }
        last = gm.inventoryView.Drop(found, parts[0] == "dropall").ToString();
        break;
    case "equipdrop":
        Reborn.UI.ItemSlotView eqView = null;
        if (gm.inventoryView.equipmentView != null)
            foreach (var sv in gm.inventoryView.equipmentView.slots) if (sv != null) { eqView = sv; break; }
        if (eqView == null)
            foreach (var sv in UnityEngine.Object.FindObjectsByType<Reborn.UI.ItemSlotView>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
                if (sv.isEquipSlot) { eqView = sv; break; }
        if (eqView == null) return "kein Ausruestungsplatz";
        last = gm.inventoryView.Drop(eqView, false).ToString();
        break;
    case "onto":
        // Spieler direkt auf den weggeworfenen Gegenstand stellen (Aufheb-Sperre pruefen)
        foreach (var pk in UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>())
            if (pk.item == item) { rb.position = pk.transform.position; p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero; break; }
        break;
    case "state": break;
    default: return "unbekannt %ACT%";
}
int ground = 0;
float near = -1f;
foreach (var pk in UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>())
{
    if (item == null || pk.item != item) continue;
    ground += pk.count;
    float dist = UnityEngine.Vector2.Distance(pk.transform.position, p.transform.position);
    if (near < 0f || dist < near) near = dist;
}
var detail = gm.inventoryView.detail;
return "Anzahl=" + (item != null ? inv.Count(item) : -1) + " Boden=" + ground
    + " Abstand=" + (near < 0f ? "-" : near.ToString("0.00", ic))
    + " Knopf=" + (detail.dropButton != null && detail.dropButton.gameObject.activeSelf) + "|" + (detail.dropAllButton != null && detail.dropAllButton.gameObject.activeSelf)
    + " Ergebnis=" + last;
