// Woelfe weg, Boss besiegen, Beute am Boden auflisten, Spielstand lesen
//#HEADER
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) if (a.name.StartsWith("Boss_Wolf")) a.gameObject.SetActive(false);
var boss = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.BossAI>();
if (boss == null) return "kein Boss";
int lvl = prog.Level, xp = prog.XP;
boss.Health.TakeDamage(99999, UnityEngine.Vector2.zero);
var loot = new System.Collections.Generic.List<string>();
int gold = 0;
foreach (var pk in UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>()) { if (pk.item != null) loot.Add(pk.item.id + "x" + pk.count); gold += pk.gold; }
loot.Sort();
var data = UnityEngine.JsonUtility.FromJson<Reborn.Save.SaveData>(System.IO.File.ReadAllText(Reborn.Save.SaveSystem.SavePath));
var bar = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.BossBar>();
return "tot=" + boss.Health.IsDead + " Beute=" + string.Join(",", loot) + " Gold=" + gold + " EP+=" + (prog.Level > lvl || prog.XP > xp)
    + " gespeichert=" + string.Join(",", data.defeatedBosses) + " Version=" + data.version + " Leiste=" + (bar != null && bar.Visible);