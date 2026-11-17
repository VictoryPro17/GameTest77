// Taste auf der Test-Tastatur druecken (bleibt gedrueckt bis b_keyup)
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.%KEY%));
return "Taste %KEY%";