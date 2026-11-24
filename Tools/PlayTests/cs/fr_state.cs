// Zustand des Drachenkampfs (Meilenstein 9a)
//#HEADER
var dragon = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.DragonAI>();
var bar = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.BossBar>();
if (dragon == null) return "kein Drache Leiste=" + (bar != null && bar.Visible);
int spirits = 0;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) if (a.name.StartsWith("Dragon_Spirit")) spirits++;
return "wach=" + dragon.IsAwake + " Zustand=" + dragon.StateName + " HP=" + dragon.Health.Current + "/" + dragon.Health.maxHealth + " unverwundbar=" + dragon.Health.invulnerable
    + " Flug=" + dragon.IsFlying + " Geflogen=" + dragon.HasFlown + " Atem=" + dragon.BreathCount + " AtemTreffer=" + dragon.BreathHits + " Regen=" + dragon.RainCount
    + " Zapfen=" + Reborn.Enemies.RootSpike.StrikeCount + " ZapfenTreffer=" + Reborn.Enemies.RootSpike.HitCount
    + " Fluegel=" + dragon.WingCount + " FluegelTreffer=" + dragon.WingHits + " Blizzard=" + dragon.BlizzardCount + " BlizzardTreffer=" + dragon.BlizzardHits
    + " Geschuetzt=" + dragon.BlizzardBlocked + " laedt=" + dragon.UltimateCharging + " Warnung=" + (bar != null && bar.WarningVisible) + " Leiste=" + (bar != null && bar.Visible)
    + " Geister=" + spirits + " Spieler=" + hp.Current + " Erst=" + dragon.FirstKill + " Name=" + (bar != null && bar.nameText != null ? bar.nameText.text : "-");
