// Zustand Stufe 2 (Meilenstein 8b)
//#HEADER
var boss = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.BossAI>();
var bar = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.BossBar>();
int wolves = 0, wolfHp = 0;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) if (a.name.StartsWith("Boss_Wolf_W")) { wolves++; wolfHp = a.GetComponent<Reborn.Combat.Health>().maxHealth; }
if (boss == null) return "kein Boss Spieler=" + hp.Current + "/" + hp.maxHealth;
return "Stufe=" + boss.Stage + " Zustand=" + boss.StateName + " HP=" + boss.Health.Current + "/" + boss.Health.maxHealth + " unverwundbar=" + boss.Health.invulnerable
    + " Name=" + (bar != null && bar.nameText != null ? bar.nameText.text : "-") + " Erst=" + boss.FirstKill
    + " Wellen=" + Reborn.Enemies.ThornWave.WaveCount + " WellenTreffer=" + Reborn.Enemies.ThornWave.HitCount
    + " Ansturm=" + boss.ChargeCount + " AnsturmTreffer=" + boss.ChargeHits + " Ultimate=" + boss.UltimateCount + " laedt=" + boss.UltimateCharging + " Warnung=" + (bar != null && bar.WarningVisible)
    + " WolfRufe=" + boss.WolfCalls + " Woelfe=" + wolves + " WolfHP=" + wolfHp + " Spieler=" + hp.Current + "/" + hp.maxHealth;