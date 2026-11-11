// Treffer gutschreiben (Platzhalter SLOT, N)
//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
for (int i = 0; i < %N%; i++) book.RegisterHit(%SLOT%);
return "Stufe=" + book.Level(%SLOT%) + " XP=" + book.XP(%SLOT%) + " Schaden=" + book.Damage(%SLOT%) + " frei=" + book.IsUnlocked(1) + "," + book.IsUnlocked(2) + "," + book.IsUnlocked(3);
