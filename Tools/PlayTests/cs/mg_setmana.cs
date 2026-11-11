//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
mana.SetCurrent(%M%f);
book.ResetCooldowns();
return "Mana gesetzt=" + UnityEngine.Mathf.FloorToInt(mana.Current);
