var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
if (!Reborn.UI.GameMenu.IsOpen) menu.Open();
menu.ShowPage(%PAGE%);
return "Menue offen, Seite=" + menu.Page;
