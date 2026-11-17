//#HEADER
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
var chest = chestById("room_test_truhe_1");
var pickups = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>();
var names = "";
foreach (var pk in pickups) names += (pk.item != null ? pk.item.itemName : pk.gold + " Gold") + " x" + pk.count + "; ";
return "Truhe offen=" + chest.IsOpen + " Beute am Boden=" + pickups.Length + " (" + names + ")";
