// Spielstand-Datei lesen: ist der Schalter der Einfuehrung gespeichert?
var data = UnityEngine.JsonUtility.FromJson<Reborn.Save.SaveData>(System.IO.File.ReadAllText(Reborn.Save.SaveSystem.SavePath));
return "Flag=" + data.worldFlags.Contains("tutorial_done") + " Flags=" + string.Join(",", data.worldFlags);
