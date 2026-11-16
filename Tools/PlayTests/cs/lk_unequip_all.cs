//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
for (int i = 0; i < Reborn.Inventory.PlayerEquipment.SlotCount; i++) eq.Unequip(i);
inv.Clear();
return "alles abgelegt";
