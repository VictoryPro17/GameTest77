// Welche Ebene zeigt welches Bild? (leer = nichts)
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var sb = new System.Text.StringBuilder();
foreach (var slot in Reborn.Animation.EquipmentVisuals.LayerOrder) sb.Append(slot + "=" + vis.SpriteNameFor(slot) + " ");
return sb.ToString().Trim() + " | Koerper=" + (bodySr.sprite != null ? bodySr.sprite.name : "-");
