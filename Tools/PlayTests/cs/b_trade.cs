// Zustand des Handelsfensters
var w = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>();
if (w == null) return "kein Handelsfenster";
var near = Reborn.UI.TradeWindow.NearbyNpc;
string page = !Reborn.UI.TradeWindow.IsOpen ? "-" : w.shopView.gameObject.activeSelf ? "Haendler" : w.smithView.gameObject.activeSelf ? "Schmied" : w.guildView != null && w.guildView.gameObject.activeSelf ? "Gilde" : "?";
return "nah=" + (near != null ? near.displayName : "-") + " offen=" + Reborn.UI.TradeWindow.IsOpen + " Seite=" + page
    + " Zeit=" + UnityEngine.Time.timeScale.ToString("F0") + " Menue=" + Reborn.UI.GameMenu.IsOpen + " Hinweis=" + (w.promptText != null ? w.promptText.text : "");