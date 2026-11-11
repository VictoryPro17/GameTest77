// Controller loslassen und melden, wie weit der Spieler gelaufen ist
var ic = System.Globalization.CultureInfo.InvariantCulture;
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
if (pad != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
return "x=" + p.transform.position.x.ToString("F3", ic) + " (vorher " + UnityEngine.PlayerPrefs.GetFloat("_t_x").ToString("F3", ic) + ") clip=" + anim.CurrentClip + " facing=" + anim.Facing;
