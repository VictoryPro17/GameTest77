// Virtueller Controller: linker Stick nach rechts (gleicher Weg wie der Touch-Joystick)
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>("TestPad");
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
UnityEngine.PlayerPrefs.SetFloat("_t_x", p.transform.position.x);
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(1f, 0f) });
return "start x=" + p.transform.position.x;
