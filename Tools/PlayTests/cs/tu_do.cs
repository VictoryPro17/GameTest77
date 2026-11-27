// Einfuehrung bedienen: request | begin:<n> | next | skip | clickskip | walk | cast | menuopen | menuclose | trade | tradeclose
//   | tocave | far | peaceful | resetflag. Gibt danach denselben Zustand wie tu_state aus.
//#HEADER
var guide = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TutorialGuide>(UnityEngine.FindObjectsInactive.Include);
var gm = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>(UnityEngine.FindObjectsInactive.Include);
var tw = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>(UnityEngine.FindObjectsInactive.Include);
var parts = "%ACT%".Split(':');
string note = "";
if (guide == null && parts[0] != "request" && parts[0] != "resetflag" && parts[0] != "peaceful") return "kein Mentor";
switch (parts[0])
{
    case "request": Reborn.UI.MainMenu.TutorialRequested = true; break;
    case "begin": guide.Begin(int.Parse(parts[1])); break;
    case "next": guide.Next(); break;
    case "skip": guide.Skip(); break;
    case "clickskip":
        if (guide.SkipButton == null) return "kein Ueberspringen-Knopf";
        guide.SkipButton.onClick.Invoke();
        break;
    case "walk":
    {
        // ein Versatz von 1.2 pro Aufruf, hin und her (der Guide misst die Strecke im Update)
        float dx = p.transform.position.x < 0f ? 1.2f : -1.2f;
        var np = (UnityEngine.Vector2)p.transform.position + new UnityEngine.Vector2(dx, 0f);
        rb.position = np;
        p.transform.position = new UnityEngine.Vector3(np.x, np.y, p.transform.position.z);
        break;
    }
    case "cast":
    {
        var book = p.GetComponent<Reborn.Magic.SpellBook>();
        var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
        book.ResetCooldowns();
        mana.SetCurrent(mana.Max);
        note = " Gezaubert=" + book.TryCast(0) + " Grund=" + book.LastFail;
        break;
    }
    case "menuopen": gm.Open(); break;
    case "menuclose": gm.Close(); break;
    case "trade":
    {
        Reborn.World.Npc found = null;
        foreach (var n in UnityEngine.Object.FindObjectsByType<Reborn.World.Npc>())
            if (n.name == "Npc_haendler") { found = n; break; }
        if (found == null) return "kein Npc_haendler";
        tw.Open(found);
        break;
    }
    case "tradeclose": tw.Close(); break;
    case "tocave":
    {
        var door = UnityEngine.GameObject.Find("Tuer_Hoehle");
        if (door == null) return "keine Tuer_Hoehle";
        // 2.2 unterhalb der Tuer: ausserhalb des Portal-Triggers (Radius 0.4), innerhalb der Reichweite 2.6
        var np = (UnityEngine.Vector2)door.transform.position + new UnityEngine.Vector2(0f, -2.2f);
        rb.position = np;
        p.transform.position = new UnityEngine.Vector3(np.x, np.y, p.transform.position.z);
        rb.linearVelocity = UnityEngine.Vector2.zero;
        break;
    }
    case "start":
        // zurueck zum Startpunkt (0,-2): weit genug vom Haendler, damit der Pfeil sichtbar ist
        rb.position = new UnityEngine.Vector2(0f, -2f); p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero;
        break;
    case "far":
    {
        var m = UnityEngine.GameObject.Find("Mentor_Aldric");
        if (m == null) return "kein Mentor-Objekt";
        float dx = m.transform.position.x < 0f ? 15f : -15f;
        var np = (UnityEngine.Vector2)m.transform.position + new UnityEngine.Vector2(dx, 0f);
        rb.position = np;
        p.transform.position = new UnityEngine.Vector3(np.x, np.y, p.transform.position.z);
        rb.linearVelocity = UnityEngine.Vector2.zero;
        break;
    }
    case "peaceful": break;
    case "resetflag":
    {
        var keep = new System.Collections.Generic.List<string>();
        foreach (var f in Reborn.World.WorldFlags.All) if (f != Reborn.World.TutorialGuide.DoneFlag) keep.Add(f);
        Reborn.World.WorldFlags.SetAll(keep);
        break;
    }
    default: return "unbekannt %ACT%";
}
var wal = p.GetComponent<Reborn.Economy.PlayerWallet>();
var mentor = UnityEngine.GameObject.Find("Mentor_Aldric");
guide = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TutorialGuide>(UnityEngine.FindObjectsInactive.Include);
var tr = guide != null ? guide.ArrowTarget : null;
var potion = db.Get("heiltrank_klein");
return "Schritt=" + Reborn.World.TutorialGuide.CurrentStep
    + " Aktiv=" + Reborn.World.TutorialGuide.Active
    + " Fertig=" + Reborn.World.TutorialGuide.Completed
    + " Panel=" + (guide != null && guide.PanelVisible)
    + " Pfeil=" + (guide != null && guide.ArrowVisible)
    + " Ziel=" + (tr != null ? tr.name : "-")
    + " Training=" + Reborn.World.SceneRules.TrainingAllowed
    + " Frieden=" + Reborn.World.SceneRules.Peaceful
    + " Gold=" + wal.Gold
    + " Traenke=" + (potion != null ? inv.Count(potion) : -1)
    + " Belohnt=" + Reborn.World.TutorialGuide.RewardCount
    + " Ueberspr=" + Reborn.World.TutorialGuide.Skipped
    + " Mentor=" + (mentor != null ? UnityEngine.Vector2.Distance(mentor.transform.position, p.transform.position).ToString("0.00", ic) : "-1")
    + note
    + " Hinweis=" + (guide != null ? guide.HintText : "");
