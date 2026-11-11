//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
return "x=" + p.transform.position.x.ToString("F3", ic) + " y=" + p.transform.position.y.ToString("F3", ic) + " Mana=" + mana.Current.ToString("F1", ic) + " ManaXP=" + mana.XP + " meditiert=" + mana.Meditating;
