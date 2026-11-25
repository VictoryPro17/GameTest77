// Hauptmenue bedienen: newgame | yes | no | continue | settings | back | news | credits | textback | savefile | flag
var mm = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.MainMenu>(UnityEngine.FindObjectsInactive.Include);
string act = "%ACT%";
if (act == "flag") return "Tutorial=" + Reborn.UI.MainMenu.TutorialRequested;
if (act == "savefile")
{
    System.IO.File.WriteAllText(Reborn.Save.SaveSystem.SavePath, UnityEngine.JsonUtility.ToJson(new Reborn.Save.SaveData()));
    return "ok savefile";
}
if (mm == null) return "kein Hauptmenue";
switch (act)
{
    case "newgame": mm.NewGame(); break;
    case "yes": mm.confirm.Answer(true); break;
    case "no": mm.confirm.Answer(false); break;
    case "continue": mm.Continue(); break;
    case "settings": mm.settingsButton.onClick.Invoke(); break;
    case "back": mm.settings.HandleBack(); break;
    case "news": mm.newsButton.onClick.Invoke(); break;
    case "credits": mm.creditsButton.onClick.Invoke(); break;
    case "textback": mm.textBackButton.onClick.Invoke(); break;
    default: return "unbekannt " + act;
}
return "ok " + act;
