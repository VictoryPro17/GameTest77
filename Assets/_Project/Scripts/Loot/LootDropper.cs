using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Wuerfelt eine Loot-Tabelle aus und legt Pickups auf den Boden.</summary>
    public class LootDropper : MonoBehaviour
    {
        public LootTable table;
        public Pickup pickupPrefab;
        public float scatterRadius = 0.5f;

        public void Drop()
        {
            if (table == null || pickupPrefab == null) return;
            foreach (var entry in table.entries)
            {
                if (entry == null || entry.item == null) continue;
                if (Random.value > entry.dropChance) continue;
                int amount = Random.Range(entry.min, entry.max + 1);
                if (amount <= 0) continue;
                Vector2 position = (Vector2)transform.position + Random.insideUnitCircle * scatterRadius;
                var pickup = Instantiate(pickupPrefab, position, Quaternion.identity);
                pickup.Setup(entry.item, amount);
            }
        }
    }
}
