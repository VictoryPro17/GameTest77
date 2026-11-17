// Schmied: equip:<id> | open | up:<id> | gel:<n>
//#HEADER
var w = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>();
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var book = p.GetComponent<Reborn.Economy.UpgradeBook>();
var parts = "%ACT%".Split(':');
string msg = "";
Reborn.Loot.ItemData item = parts.Length > 1 && parts[0] != "gel" ? db.Get(parts[1]) : db.Get("holzschwert");
switch (parts[0])
{
    case "equip": inv.Add(item, 1); eq.Equip(inv.IndexOf(item)); msg = "angelegt"; break;
    case "open":
        foreach (var n in UnityEngine.Object.FindObjectsByType<Reborn.World.Npc>()) if (n.kind == Reborn.World.NpcKind.Schmied) w.Open(n);
        msg = "Liste=" + w.smithView.Items.Count; break;
    case "up": msg = w.smithView.Upgrade(item); break;
    case "gel": inv.Add(db.Get("schleimgel"), int.Parse(parts[1])); msg = "Gel dazu"; break;
}
return "Meldung=" + msg + " | Stufe=" + book.Level(item) + " Angriff=" + st.Attack + " Gold=" + wallet.Gold
    + " Gel=" + inv.Count(db.Get("schleimgel")) + " Name=" + Reborn.UI.ItemDetailView.DisplayName(item);