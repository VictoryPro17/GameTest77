//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var xs = new string[4];
for (int i = 0; i < 4; i++) xs[i] = book.Level(i) + ":" + book.XP(i);
return "Lv" + prog.Level + " Zauber[" + string.Join(" ", xs) + "] ManaStufe=" + mana.Level + " ManaXP=" + mana.XP + " Mana=" + UnityEngine.Mathf.FloorToInt(mana.Current) + "/" + mana.Max;
