//#HEADER
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var book = p.GetComponent<Reborn.Economy.UpgradeBook>();
int ups = 0; foreach (var kv in book.All) ups++;
return "Lv" + prog.Level + " Gold=" + wallet.Gold + " Stufen=" + ups + " Heiltraenke=" + inv.Count(db.Get("heiltrank_klein"));