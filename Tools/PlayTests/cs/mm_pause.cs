// Pause-Menue: open | close | settings | back | mainmenu | no | yes | state
//#HEADER
var gm = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>(UnityEngine.FindObjectsInactive.Include);
if (gm == null) return "kein Menue";
switch ("%ACT%")
{
    case "open": gm.Open(); break;
    case "close": gm.Close(); break;
    case "settings": gm.settingsButton.onClick.Invoke(); break;
    case "back": gm.settingsView.HandleBack(); break;
    case "mainmenu": gm.mainMenuButton.onClick.Invoke(); break;
    case "no": gm.confirm.Answer(false); break;
    case "yes": gm.confirm.Answer(true); break;
    case "state": break;
    default: return "unbekannt %ACT%";
}
return "Menue=" + Reborn.UI.GameMenu.IsOpen + " Wurzel=" + gm.root.activeSelf + " Einst=" + gm.settingsView.IsOpen
    + " Nachfrage=" + gm.confirm.IsOpen + " Meldung=" + gm.confirm.Message
    + " Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
