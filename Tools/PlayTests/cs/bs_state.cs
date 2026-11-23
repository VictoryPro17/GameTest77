// Zustand des Boss-Kampfs
//#HEADER
Reborn.Enemies.BossAI boss = null;
foreach (var b in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.BossAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) boss = b;
var bar = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.BossBar>();
int wolves = 0;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) if (a.name.StartsWith("Boss_Wolf")) wolves++;
int spikes = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.RootSpike>().Length;
if (boss == null) return "kein Boss Leiste=" + (bar != null && bar.Visible) + " Spieler=" + hp.Current;
return "wach=" + boss.IsAwake + " Zustand=" + boss.StateName + " Phase=" + boss.Phase + " HP=" + boss.Health.Current + "/" + boss.Health.maxHealth
    + " Leiste=" + (bar != null && bar.Visible) + " Name=" + (bar != null && bar.nameText != null ? bar.nameText.text : "-")
    + " Schlaege=" + boss.SlamCount + " Wurzeln=" + boss.RootCount + " Ausgeloest=" + Reborn.Enemies.RootSpike.StrikeCount
    + " WurzelTreffer=" + Reborn.Enemies.RootSpike.HitCount + " Stacheln=" + spikes + " Woelfe=" + wolves + " Spieler=" + hp.Current;