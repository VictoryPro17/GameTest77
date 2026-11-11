// W3: nah an die linke Mauer setzen und nach links ausweichen
//#HEADER
rb.position = new UnityEngine.Vector2(-8.3f, 2f);
p.transform.position = rb.position;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(-1f, 0f) }.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.South));
return "Start (-8.3, 2), Ausweichen nach links";
