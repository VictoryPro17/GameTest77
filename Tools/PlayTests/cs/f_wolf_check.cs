// Hat die Kreatur getroffen (Schaden, Angriffs-Animation)? Danach besiegen; Beute zaehlen
//#HEADER
hp.invulnerable = true;
var go = UnityEngine.GameObject.Find("Test_%ART%");
if (go == null) return "Test-Kreatur fehlt";
int before = UnityEngine.PlayerPrefs.GetInt("_t_hp");
int attack = UnityEngine.PlayerPrefs.GetInt("_t_attack");
int pickupsBefore = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>().Length;
int xpBefore = prog.XP + prog.Level * 100000;
go.GetComponent<Reborn.Combat.Health>().TakeDamage(9999, UnityEngine.Vector2.zero);
int pickups = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>().Length - pickupsBefore;
int xpGain = prog.XP + prog.Level * 100000 - xpBefore;
return "Schaden=" + (before - hp.Current) + " Angriff=" + attack + " besiegt=" + go.GetComponent<Reborn.Combat.Health>().IsDead + " Beute=" + pickups + " EP+=" + (xpGain > 0);