//#HEADER
hp.invulnerable = true;
int dmg = UnityEngine.PlayerPrefs.GetInt("_t_dmg", -1);
UnityEngine.PlayerPrefs.DeleteKey("_t_dmg");
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) sl.gameObject.SetActive(false);
hp.Heal(999);
var pos = p.transform.position;
bool inRoom = pos.x >= -8.9f && pos.x <= 8.9f && pos.y >= -5.0f && pos.y <= 4.8f;
return "Schaden pro Schleimtreffer=" + dmg + " HP " + hp.Current + "/" + hp.maxHealth + " x=" + pos.x.ToString("F2", ic) + " y=" + pos.y.ToString("F2", ic) + " imRaum=" + inRoom;
