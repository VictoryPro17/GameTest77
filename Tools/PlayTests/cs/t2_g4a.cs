//#HEADER
hp.SetCurrent(20);
var pot = db.Get("heiltrank_klein");
UnityEngine.PlayerPrefs.SetInt("_t_pot", inv.Count(pot));
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Q));
return "HP auf " + hp.Current + ", Traenke " + inv.Count(pot) + ", Q gedrueckt";
