// Treffer: Koerper und Ebenen muessen gleich gefaerbt sein (rotes Aufblitzen)
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
hp.invulnerable = false;
hp.TakeDamage(1, UnityEngine.Vector2.zero);
hp.invulnerable = true;
var bodyColor = bodySr.color;
int same = 0, used = 0;
foreach (var l in vis.layers) { if (l.sprite == null) continue; used++; if (l.color == bodyColor) same++; }
return "Koerperfarbe=" + bodyColor + " rot=" + (bodyColor.g < 0.9f) + " Ebenen=" + used + " gleich=" + same;
