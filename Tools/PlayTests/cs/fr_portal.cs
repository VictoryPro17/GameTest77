// Siegel-Portal in der Hoehle (Meilenstein 9a): ACT = state | givekey | unseal
//#HEADER
Reborn.World.SealedPortal sp = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<Reborn.World.SealedPortal>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (s.name == "Portal_Frost") sp = s;
if (sp == null) return "kein Portal_Frost";
var act = "%ACT%";
if (act == "givekey") inv.Add(db.Get("frostschluessel"), 1);
else if (act == "unseal") sp.TryUnseal();
return "offen=" + sp.IsOpen + " Schluessel=" + inv.Count(db.Get("frostschluessel")) + " Meldung=" + sp.LastMessage
    + " versiegelt=" + (sp.sealedVisual != null && sp.sealedVisual.activeSelf) + " blau=" + (sp.openVisual != null && sp.openVisual.activeSelf)
    + " Flag=" + Reborn.World.WorldFlags.Has("frost_siegel") + " Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
