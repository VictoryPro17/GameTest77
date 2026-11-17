// Zustand nach einem Szenenwechsel
//#HEADER
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var book = p.GetComponent<Reborn.Economy.UpgradeBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
return "Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
    + " Pos=(" + p.transform.position.x.ToString("F1", ic) + "," + p.transform.position.y.ToString("F1", ic) + ")"
    + " Gold=" + wallet.Gold + " Stufe=" + book.Level("holzschwert") + " Angriff=" + st.Attack
    + " HP=" + hp.Current + "/" + hp.maxHealth + " Mana=" + UnityEngine.Mathf.FloorToInt(mana.Current) + "/" + mana.Max
    + " Reisen=" + Reborn.World.SceneTravel.TravelCount + " reist=" + Reborn.World.SceneTravel.IsTraveling
    + " Ankunft=" + Reborn.World.SpawnPoint.LastArrival;