// A8: Tod des Spielers (ganz am Ende des Tests): Sterbe-Animation startet
//#HEADER
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
hp.invulnerable = false;
hp.TakeDamage(9999, UnityEngine.Vector2.zero);
hp.invulnerable = true;
return "tot=" + hp.IsDead + " clip=" + anim.CurrentClip;
