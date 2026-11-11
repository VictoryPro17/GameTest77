//#HEADER
var pickups = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>();
if (pickups.Length == 0) return "keine Beute am Boden";
rb.position = (UnityEngine.Vector2)pickups[0].transform.position;
p.transform.position = rb.position;
return "zu " + pickups[0].item.itemName;
