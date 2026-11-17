var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState());
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad");
if (pad != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
return "losgelassen";