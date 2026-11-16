// Saphirring anlegen: Intelligenz +2 -> Mana-Maximum +10
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
int max0 = mana.Max, int0 = st.GetAttribute(Reborn.Progression.PlayerAttribute.Intelligenz);
bool ok = eq.Equip(inv.IndexOf(db.Get("saphirring")));
return "angelegt=" + ok + " Intelligenz " + int0 + "->" + st.GetAttribute(Reborn.Progression.PlayerAttribute.Intelligenz) + " Mana-Max " + max0 + "->" + mana.Max;
