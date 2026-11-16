var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.West));
return "Angriff";
