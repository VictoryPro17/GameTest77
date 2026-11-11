//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
book.SetState(1, 4, 7);
book.SetState(3, 2, 5);
mana.SetState(3, 12, -1f);
p.GetComponent<Reborn.Save.SaveSystem>().Save();
return "gespeichert=" + System.IO.File.Exists(Reborn.Save.SaveSystem.SavePath);
