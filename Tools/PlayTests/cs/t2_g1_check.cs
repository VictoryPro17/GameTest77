//#HEADER
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
var chest = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TreasureChest>();
var pickups = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>();
var names = "";
foreach (var pk in pickups) names += pk.item.itemName + " x" + pk.count + "; ";
return "Truhe offen=" + chest.IsOpen + " Beute am Boden=" + pickups.Length + " (" + names + ")";
