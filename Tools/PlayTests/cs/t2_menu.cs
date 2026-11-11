var es = UnityEngine.EventSystems.EventSystem.current;
var sel = es != null && es.currentSelectedGameObject != null ? es.currentSelectedGameObject.name : "-";
return "Menue offen=" + Reborn.UI.GameMenu.IsOpen + " timeScale=" + UnityEngine.Time.timeScale + " Auswahl=" + sel;
