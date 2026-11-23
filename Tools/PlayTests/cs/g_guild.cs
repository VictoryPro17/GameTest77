// Gilde (Meilenstein 7c): join | accept | turnin | state | kill:<art>:<anzahl>:<zauber 0/1> | give:<id>:<n> | setstate:none
//#HEADER
var g = p.GetComponent<Reborn.Guilds.PlayerGuild>();
var w = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>();
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var book = p.GetComponent<Reborn.Economy.UpgradeBook>();
var parts = "%ACT%".Split(':');
string msg = "";
switch (parts[0])
{
    case "join": msg = w.guildView.Join(); break;
    case "accept": msg = w.guildView.Accept(); break;
    case "turnin": msg = w.guildView.TurnIn(); break;
    case "kill":
        var field = typeof(Reborn.Enemies.SlimeAI).GetField("OnAnyKilled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var ev = field != null ? field.GetValue(null) as System.Action<string, bool> : null;
        for (int i = 0; i < int.Parse(parts[2]); i++) if (ev != null) ev(parts[1], parts[3] == "1");
        msg = "besiegt " + parts[2] + "x " + parts[1]; break;
    case "give": inv.Add(db.Get(parts[1]), int.Parse(parts[2])); msg = "bekommen"; break;
    case "setstate": g.SetState(Reborn.Guilds.GuildId.None, 0, 0, false, 0); msg = "zurueckgesetzt"; break;
}
string weapon = g.Data != null && g.Data.weapon != null ? g.Data.weapon.id : "-";
return "Meldung=" + msg + " | Gilde=" + g.Guild + " Rang=" + g.Rank + " Quest=" + g.QuestIndex + " aktiv=" + g.QuestActive
    + " Fortschritt=" + g.CurrentProgress + "/" + g.Required + " Staerke=" + st.GetAttribute(Reborn.Progression.PlayerAttribute.Staerke)
    + " Intelligenz=" + st.GetAttribute(Reborn.Progression.PlayerAttribute.Intelligenz) + " Gold=" + wallet.Gold + " EP=" + prog.XP + " Lv=" + prog.Level
    + " Waffe=" + weapon + ":" + (weapon != "-" ? inv.Count(db.Get(weapon)) + "/+" + book.Level(weapon) : "0") + " HUD=" + g.HudText;