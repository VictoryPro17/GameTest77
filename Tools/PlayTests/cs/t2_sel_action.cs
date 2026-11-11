var es = UnityEngine.EventSystems.EventSystem.current;
var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.InventoryView>();
es.SetSelectedGameObject(view.detail.actionButton.gameObject);
return "Auswahl=" + es.currentSelectedGameObject.name + " Knopf=" + view.detail.actionLabel.text;
