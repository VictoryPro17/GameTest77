// Tod des Spielers
//#HEADER
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
int before = wallet.Gold;
hp.invulnerable = false;
hp.TakeDamage(9999, UnityEngine.Vector2.zero);
return "tot=" + hp.IsDead + " Gold " + before + "->" + wallet.Gold + " Verlust=" + p.GetComponent<Reborn.Combat.DeathHandler>().LastLoss;