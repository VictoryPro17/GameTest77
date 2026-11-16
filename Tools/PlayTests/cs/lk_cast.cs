//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
mana.SetCurrent(mana.Max);
var book = p.GetComponent<Reborn.Magic.SpellBook>();
book.ResetCooldowns();
return "zaubert=" + book.TryCast(0);
