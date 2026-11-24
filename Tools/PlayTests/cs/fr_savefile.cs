// Spielstand-Datei lesen: Version, Welt-Schalter, besiegte Bosse
var data = UnityEngine.JsonUtility.FromJson<Reborn.Save.SaveData>(System.IO.File.ReadAllText(Reborn.Save.SaveSystem.SavePath));
return "Version=" + data.version + " Flags=" + string.Join(",", data.worldFlags) + " Bosse=" + string.Join(",", data.defeatedBosses);
