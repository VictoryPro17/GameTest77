// Demo-Fassung (Meilenstein 10c): ACT = on | off | state | givekey | unseal  (Siegel-Portal in der Hoehle)
//#HEADER
Reborn.World.SealedPortal sp = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<Reborn.World.SealedPortal>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (s.name == "Portal_Frost") sp = s;
if (sp == null) return "kein Portal_Frost";
var act = "%ACT%";
if (act == "on") Reborn.Core.Demo.Active = true;
else if (act == "off") Reborn.Core.Demo.Active = false;
else if (act == "givekey") inv.Add(db.Get("frostschluessel"), 1);
else if (act == "unseal") sp.TryUnseal();
return "Demo=" + Reborn.Core.Demo.Active + " offen=" + sp.IsOpen + " Betreten=" + sp.CanEnter + " Schluessel=" + inv.Count(db.Get("frostschluessel"))
    + " Flag=" + Reborn.World.WorldFlags.Has("frost_siegel") + " Meldung=" + sp.LastMessage;