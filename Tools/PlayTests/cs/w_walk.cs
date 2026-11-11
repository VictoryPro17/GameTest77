// Spieler an eine Stelle setzen und den Stick in eine Richtung halten
//#HEADER
rb.position = new UnityEngine.Vector2(%PX%f, %PY%f);
p.transform.position = rb.position;
rb.linearVelocity = UnityEngine.Vector2.zero;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(%SX%f, %SY%f) });
return "Start (%PX%, %PY%), Stick (%SX%, %SY%)";
