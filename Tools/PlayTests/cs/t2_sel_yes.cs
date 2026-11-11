var es = UnityEngine.EventSystems.EventSystem.current;
var cv = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.CharacterView>();
es.SetSelectedGameObject(cv.confirmYes.gameObject);
return "Auswahl=" + es.currentSelectedGameObject.name;
