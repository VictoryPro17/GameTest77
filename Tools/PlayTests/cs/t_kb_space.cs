var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(kb, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));
return "Taste Space gedrueckt";
