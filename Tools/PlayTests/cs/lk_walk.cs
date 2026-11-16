var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad");
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(1f, 0f) });
return "laeuft nach rechts";
