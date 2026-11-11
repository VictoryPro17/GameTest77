// W4: ein Schleim steht rechts vom Spieler und drueckt ihn gegen die linke Mauer
//#HEADER
rb.position = new UnityEngine.Vector2(-8.4f, 2f);
p.transform.position = rb.position;
UnityEngine.PlayerPrefs.SetInt("_t_hits", 0);
hp.OnDamaged += (amount, k) => UnityEngine.PlayerPrefs.SetInt("_t_hits", UnityEngine.PlayerPrefs.GetInt("_t_hits", 0) + 1);
hp.invulnerable = false;
Reborn.Enemies.SlimeAI slime = null;
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include)) { slime = sl; break; }
slime.transform.position = new UnityEngine.Vector3(-7.8f, 2f, 0f);
slime.gameObject.SetActive(true);
return "Spieler (-8.4, 2), Schleim rechts daneben, HP " + hp.Current;
