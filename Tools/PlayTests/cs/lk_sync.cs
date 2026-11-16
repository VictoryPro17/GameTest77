// Zeigen alle belegten Ebenen dasselbe Bild (Datei-Name) wie der Koerper?
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
string body = bodySr.sprite != null ? bodySr.sprite.name : "-";
int used = 0, same = 0;
foreach (var slot in Reborn.Animation.EquipmentVisuals.LayerOrder)
{
    var n = vis.SpriteNameFor(slot);
    if (n == "") continue;
    used++;
    if (n == body) same++;
}
return "Koerper=" + body + " Clip=" + anim.CurrentClip + " Ebenen=" + used + " gleich=" + same;
