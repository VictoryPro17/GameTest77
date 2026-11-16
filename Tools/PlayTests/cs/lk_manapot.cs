// Placeholder MODE: "leer" = Mana 0 + 2 Traenke; "voll" = Mana voll
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var pot = db.Get("manatrank_klein");
if ("%MODE%" == "leer") { inv.Add(pot, 2); mana.SetCurrent(0f); }
else mana.SetCurrent(mana.Max);
UnityEngine.PlayerPrefs.SetInt("_t_pot", inv.Count(pot));
var kb2 = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb2, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.R));
return "R gedrueckt (%MODE%), Traenke " + inv.Count(pot);
