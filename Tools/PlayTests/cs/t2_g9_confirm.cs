var es = UnityEngine.EventSystems.EventSystem.current;
var cv = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.CharacterView>();
var sel = es.currentSelectedGameObject;
return "Rueckfrage offen=" + cv.confirmPanel.activeSelf + " Auswahl=" + (sel != null ? sel.name : "-");
