var ic = System.Globalization.CultureInfo.InvariantCulture;
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
return "x=" + p.transform.position.x.ToString("F3", ic) + " (vorher " + UnityEngine.PlayerPrefs.GetFloat("_t_x").ToString("F3", ic) + ") clip=" + anim.CurrentClip + " facing=" + anim.Facing;
