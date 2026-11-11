// Nur wenn G8.5 fehlschlaegt: Was war gewaehlt, kam eine Bestaetigung an, legt ein direkter Aufruf an?
//#HEADER
var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.InventoryView>();
var sel = es != null ? es.currentSelectedGameObject : null;
var before = eq.Get(5);
UnityEngine.EventSystems.ExecuteEvents.Execute(view.gridSlots[1].gameObject, new UnityEngine.EventSystems.BaseEventData(es), UnityEngine.EventSystems.ExecuteEvents.submitHandler);
var after = eq.Get(5);
return "DIAGNOSE: Auswahl=" + (sel != null ? sel.name : "-") + " Bestaetigungen=" + UnityEngine.PlayerPrefs.GetInt("_t_sub", 0) + " direkterAufrufLegtAn=" + (before == null && after != null);
