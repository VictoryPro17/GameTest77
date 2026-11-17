// Spielstand speichern und die Datei lesen
//#HEADER
p.GetComponent<Reborn.Save.SaveSystem>().Save();
var data = UnityEngine.JsonUtility.FromJson<Reborn.Save.SaveData>(System.IO.File.ReadAllText(Reborn.Save.SaveSystem.SavePath));
var ups = "";
foreach (var u in data.upgrades) ups += u.id + ":" + u.level + " ";
return "Version=" + data.version + " Gold=" + data.gold + " Stufen=" + ups.Trim() + " Szene=" + data.lastScene + " Truhen=" + string.Join(",", data.openedChests);