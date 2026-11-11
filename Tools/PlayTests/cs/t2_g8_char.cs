//#HEADER
var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
menu.ShowPage(1);
var sel = es.currentSelectedGameObject;
return "Seite=" + menu.Page + " frei=" + st.UnspentPoints + " Auswahl=" + (sel != null ? sel.name : "-");
