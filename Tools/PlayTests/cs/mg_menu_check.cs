var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
var view = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.MagicView>();
if (view == null) return "Seite=" + menu.Page + " MagicView fehlt";
view.Refresh();
var t0 = System.Text.RegularExpressions.Regex.Replace(view.titles[0].text, "<[^>]+>", "");
var i0 = System.Text.RegularExpressions.Regex.Replace(view.infos[0].text, "<[^>]+>", "");
var i1 = System.Text.RegularExpressions.Regex.Replace(view.infos[3].text, "<[^>]+>", "");
return "Seite=" + menu.Page + " | " + t0 + " | " + i0 + " | " + i1;
