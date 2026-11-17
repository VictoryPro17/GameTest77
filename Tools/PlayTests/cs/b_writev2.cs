// Alten Spielstand (Version 2, ohne Gold und Stufen) schreiben
var json = "{\"version\":2,\"level\":2,\"xp\":5,\"attributes\":[1,1,1,1,1],\"unspentPoints\":3,\"currentHealth\":40,"
    + "\"inventory\":[{\"index\":0,\"id\":\"heiltrank_klein\",\"count\":3}],\"equipment\":[],\"openedChests\":[\"room_test_truhe_1\"],"
    + "\"manaLevel\":1,\"manaXp\":0,\"currentMana\":-1.0,\"spells\":[]}";
System.IO.File.WriteAllText(Reborn.Save.SaveSystem.SavePath, json);
return "Version-2-Spielstand geschrieben";