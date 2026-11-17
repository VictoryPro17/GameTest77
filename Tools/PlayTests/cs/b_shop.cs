// Haendler bedienen: buy:<id> | sell:<id> | gold:<n> | fill | clear
//#HEADER
var w = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>();
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var parts = "%ACT%".Split(':');
string msg = "";
Reborn.Loot.ItemData item = parts.Length > 1 && parts[0] != "gold" && parts[0] != "setgold" ? db.Get(parts[1]) : null;
switch (parts[0])
{
    case "buy": w.shopView.ShowTab(false); msg = w.shopView.Buy(item); break;
    case "sell": w.shopView.ShowTab(true); msg = w.shopView.Sell(inv.IndexOf(item)); break;
    case "gold": wallet.Add(int.Parse(parts[1])); msg = "Gold dazu"; break;
    case "setgold": wallet.SetGold(int.Parse(parts[1])); msg = "Gold gesetzt"; break;
    case "fill": for (int i = 0; i < Reborn.Inventory.PlayerInventory.Size; i++) inv.SetSlot(i, db.Get("eisenschwert"), 1); inv.NotifyChanged(); msg = "Inventar voll"; break;
    case "clear": inv.Clear(); msg = "Inventar leer"; break;
}
var count = item != null ? inv.Count(item) : 0;
return "Meldung=" + msg + " | Gold=" + wallet.Gold + " Anzahl=" + count;