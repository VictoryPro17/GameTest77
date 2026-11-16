// A4: Ein Treffer spielt die Treffer-Animation (Spieler bleibt am Leben)
//#HEADER
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
hp.invulnerable = false;
hp.TakeDamage(1, UnityEngine.Vector2.zero);
hp.invulnerable = true;
var sr = anim.GetComponent<UnityEngine.SpriteRenderer>();
return "clip=" + anim.CurrentClip + " bild=" + (sr.sprite != null ? sr.sprite.name : "-") + " HP=" + hp.Current;
