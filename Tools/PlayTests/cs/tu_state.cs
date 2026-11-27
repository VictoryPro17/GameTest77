// Einfuehrung: Zustand (Schritt, Panel, Pfeil, Ziel, Gold, Traenke, Belohnung, Mentor-Abstand, Hinweis)
//#HEADER
var guide = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TutorialGuide>(UnityEngine.FindObjectsInactive.Include);
var wal = p.GetComponent<Reborn.Economy.PlayerWallet>();
var mentor = UnityEngine.GameObject.Find("Mentor_Aldric");
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
    + " Hinweis=" + (guide != null ? guide.HintText : "");
