var es = UnityEngine.EventSystems.EventSystem.current;
var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.InventoryView>();
es.SetSelectedGameObject(view.gridSlots[%N%].gameObject);
return "Auswahl=" + es.currentSelectedGameObject.name;
