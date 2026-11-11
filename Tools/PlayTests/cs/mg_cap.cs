//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
book.AddXP(0, 100000);
mana.AddXP(100000);
book.AddXP(0, 50);
return "Stufe=" + book.Level(0) + " XP=" + book.XP(0) + " ManaStufe=" + mana.Level + " ManaXP=" + mana.XP + " Max=" + mana.Max + " Schaden=" + book.Damage(0);
