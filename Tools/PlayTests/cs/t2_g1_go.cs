//#HEADER
var chest = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TreasureChest>();
rb.position = (UnityEngine.Vector2)chest.transform.position + new UnityEngine.Vector2(0f, -0.75f);
p.transform.position = rb.position;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(0f, 1f) });
return "Spieler vor Truhe, laeuft nach oben; Truhe offen=" + chest.IsOpen;
