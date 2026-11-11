// Spielstand im alten Format (Version 1, ohne Zauber und Mana) schreiben
var json = "{\"version\":1,\"level\":3,\"xp\":40,\"attributes\":[1,1,1,2,1],\"unspentPoints\":0,\"currentHealth\":44,\"inventory\":[{\"index\":0,\"id\":\"schleimgel\",\"count\":3}],\"equipment\":[],\"openedChests\":[]}";
System.IO.File.WriteAllText(Reborn.Save.SaveSystem.SavePath, json);
return "Version-1-Spielstand geschrieben";
