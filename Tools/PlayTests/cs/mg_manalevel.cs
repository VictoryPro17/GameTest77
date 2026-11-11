//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
mana.SetState(1, 0, -1f);
int max0 = mana.Max;
mana.AddXP(50);
return "ManaStufe=" + mana.Level + " Max " + max0 + "->" + mana.Max;
