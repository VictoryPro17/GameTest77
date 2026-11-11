var path = Reborn.Save.SaveSystem.SavePath;
if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
return "Spielstand geloescht=" + !System.IO.File.Exists(path);
