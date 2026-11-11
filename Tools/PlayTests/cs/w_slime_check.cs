//#HEADER
hp.invulnerable = true;
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) sl.gameObject.SetActive(false);
int hits = UnityEngine.PlayerPrefs.GetInt("_t_hits", 0);
UnityEngine.PlayerPrefs.DeleteKey("_t_hits");
var r = "x=" + p.transform.position.x.ToString("F3", ic) + " y=" + p.transform.position.y.ToString("F3", ic) + " Treffer=" + hits + " HP " + hp.Current + "/" + hp.maxHealth;
hp.Heal(999);
return r;
