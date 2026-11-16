// Aktuelle Animation und Bild des Spielers
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var sr = anim.GetComponent<UnityEngine.SpriteRenderer>();
return "clip=" + anim.CurrentClip + " bild=" + (sr.sprite != null ? sr.sprite.name : "-");
