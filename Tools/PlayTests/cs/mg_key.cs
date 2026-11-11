// Taste auf der virtuellen Tastatur halten (Platzhalter KEY, z. B. Digit1, M)
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(%TASTEN%));
return "gedrueckt: %TASTEN%";
