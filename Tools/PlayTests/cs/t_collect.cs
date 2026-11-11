// Stellt den Spieler auf die naechste Beute am Boden
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var pickups = UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>();
if (pickups.Length == 0) return "keine Beute am Boden";
var rb = p.GetComponent<UnityEngine.Rigidbody2D>();
rb.position = (UnityEngine.Vector2)pickups[0].transform.position;
p.transform.position = rb.position;
return "zu Beute " + pickups[0].item.itemName + " (" + pickups.Length + " am Boden)";
