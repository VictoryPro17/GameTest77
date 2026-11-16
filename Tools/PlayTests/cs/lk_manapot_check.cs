//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var pot = db.Get("manatrank_klein");
int before = UnityEngine.PlayerPrefs.GetInt("_t_pot");
UnityEngine.PlayerPrefs.DeleteKey("_t_pot");
return "Mana=" + UnityEngine.Mathf.FloorToInt(mana.Current) + "/" + mana.Max + " Traenke " + before + "->" + inv.Count(pot);
