// Boss einschalten (t2_setup schaltet ihn aus), Spieler mit viel Leben an (X, Y) stellen
//#HEADER
Reborn.Enemies.BossAI boss = null;
foreach (var b in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.BossAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) boss = b;
if (boss == null) return "kein Boss";
boss.gameObject.SetActive(true);
hp.SetMaxHealth(%HP%, true);
hp.invulnerable = false;
rb.position = new UnityEngine.Vector2(%X%f, %Y%f);
p.transform.position = rb.position;
rb.linearVelocity = UnityEngine.Vector2.zero;
return "Boss an, Spieler bei (%X%, %Y%) HP " + hp.Current;