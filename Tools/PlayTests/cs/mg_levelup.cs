//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
prog.AddXP(prog.XPToNextLevel - prog.XP);
return "Level=" + prog.Level + " Feuerball frei=" + book.IsUnlocked(2);
