// Stick loslassen und Position melden
//#HEADER
if (pad != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
return "x=" + p.transform.position.x.ToString("F3", ic) + " y=" + p.transform.position.y.ToString("F3", ic);
