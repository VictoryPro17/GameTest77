// Spielstand (Version 4) speichern und Gilde aus der Datei lesen
//#HEADER
p.GetComponent<Reborn.Save.SaveSystem>().Save();
var data = UnityEngine.JsonUtility.FromJson<Reborn.Save.SaveData>(System.IO.File.ReadAllText(Reborn.Save.SaveSystem.SavePath));
return "Version=" + data.version + " Gilde=" + data.guild + " Rang=" + data.guildRank + " Quest=" + data.questIndex + " aktiv=" + data.questActive + " Fortschritt=" + data.questProgress;