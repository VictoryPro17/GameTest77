var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var hp = p.GetComponent<Reborn.Combat.Health>();
return "HP " + hp.Current + "/" + hp.maxHealth + " tot=" + hp.IsDead + " t=" + UnityEngine.Time.time.ToString("F2");
