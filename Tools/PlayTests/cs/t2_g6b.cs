//#HEADER
inv.Clear();
int rest = inv.Add(db.Get("eisenschwert"), 24);
var chest = UnityEngine.Object.FindAnyObjectByType<Reborn.World.TreasureChest>();
var pk = Reborn.Loot.LootDropper.Spawn(chest.pickupPrefab, rb.position + new UnityEngine.Vector2(2f, 0f), 0f, db.Get("kupferring"), 1);
pk.name = "TestPickupVoll";
rb.position = pk.transform.position;
p.transform.position = rb.position;
return "Inventar voll (Rest " + rest + "), Spieler auf Kupferring";
