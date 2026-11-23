// Boss um N verletzen (Phasenwechsel testen); Spieler bei Bedarf unverwundbar (simuliert Ausweichen)
//#HEADER
Reborn.Enemies.BossAI boss = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.BossAI>();
hp.invulnerable = %INV%;
if (%N% > 0 && boss != null) { boss.Health.TakeDamage(%N%, UnityEngine.Vector2.zero); }
return boss != null ? "Boss HP " + boss.Health.Current + " Phase " + boss.Phase : "kein Boss";