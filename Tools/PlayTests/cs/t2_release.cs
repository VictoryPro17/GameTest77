// Alle Test-Tasten loslassen
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard") as UnityEngine.InputSystem.Keyboard;
if (pad != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
if (kb != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
return "losgelassen";
