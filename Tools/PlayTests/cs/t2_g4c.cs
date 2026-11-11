//#HEADER
hp.Heal(999);
var pot = db.Get("heiltrank_klein");
UnityEngine.PlayerPrefs.SetInt("_t_pot", inv.Count(pot));
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Q));
return "volles Leben " + hp.Current + "/" + hp.maxHealth + ", Q gedrueckt";
