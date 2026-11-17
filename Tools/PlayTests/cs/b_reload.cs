// Werte im Spiel verfaelschen, dann den Spielstand neu laden
//#HEADER
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var book = p.GetComponent<Reborn.Economy.UpgradeBook>();
wallet.SetGold(1);
book.SetAll(null);
bool ok = p.GetComponent<Reborn.Save.SaveSystem>().Load();
return "geladen=" + ok + " Gold=" + wallet.Gold + " Stufe=" + book.Level("holzschwert") + " Angriff=" + st.Attack;