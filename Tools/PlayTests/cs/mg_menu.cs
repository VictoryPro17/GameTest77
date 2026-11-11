// Magie-Reiter auswaehlen (die Sued-Taste loest ihn dann aus)
var es = UnityEngine.EventSystems.EventSystem.current;
var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
es.SetSelectedGameObject(menu.magicTabButton.gameObject);
return "Auswahl=" + es.currentSelectedGameObject.name;
