//#HEADER
hp.SetCurrent(33);
p.GetComponent<Reborn.Save.SaveSystem>().Save();
return "gespeichert=" + System.IO.File.Exists(Reborn.Save.SaveSystem.SavePath);
