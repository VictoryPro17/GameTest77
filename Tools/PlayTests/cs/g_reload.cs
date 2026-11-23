// Gilde im Spiel loeschen, dann Spielstand neu laden
//#HEADER
var g = p.GetComponent<Reborn.Guilds.PlayerGuild>();
g.SetState(Reborn.Guilds.GuildId.None, 0, 0, false, 0);
bool ok = p.GetComponent<Reborn.Save.SaveSystem>().Load();
return "geladen=" + ok + " Gilde=" + g.Guild + " Rang=" + g.Rank + " Quest=" + g.QuestIndex + " aktiv=" + g.QuestActive + " Fortschritt=" + g.Progress;