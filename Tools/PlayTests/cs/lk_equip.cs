// Gegenstaende (Platzhalter IDS = "id,id") ins Inventar legen und anlegen
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
foreach (var id in "%IDS%".Split(','))
{
    if (id == "") continue;
    inv.Add(db.Get(id), 1);
    eq.Equip(inv.IndexOf(db.Get(id)));
}
return "angelegt: %IDS%";
