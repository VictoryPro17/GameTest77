//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var open = new string[4];
for (int i = 0; i < 4; i++) open[i] = book.IsUnlocked(i).ToString();
return "Mana=" + UnityEngine.Mathf.FloorToInt(mana.Current) + "/" + mana.Max + " frei=" + string.Join(",", open) + " Stufen=" + string.Join(",", book.levels) + " ManaStufe=" + mana.Level + " ManaXP=" + mana.XP;
