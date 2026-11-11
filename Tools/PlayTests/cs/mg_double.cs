// Zweimal direkt hintereinander zaubern (gleiches Bild): nur der erste darf klappen
//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
mana.SetCurrent(mana.Max);
book.ResetCooldowns();
int c0 = book.CastCount;
bool a = book.TryCast(0);
bool b = book.TryCast(0);
return "Zauber=" + (book.CastCount - c0) + " erster=" + a + " zweiter=" + b + " LastFail=" + book.LastFail;
