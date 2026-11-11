using Reborn.Progression;
using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Wuerfelt eine Loot-Tabelle aus und legt Pickups auf den Boden. Glueck des Spielers erhoeht die Chancen.</summary>
    public class LootDropper : MonoBehaviour
    {
        public LootTable table;
        public Pickup pickupPrefab;
        public float scatterRadius = 0.5f;

        public void Drop()
        {
            if (table == null || pickupPrefab == null) return;
            float multiplier = 1f;
            var player = GameObject.FindGameObjectWithTag("Player");
            var stats = player != null ? player.GetComponent<PlayerStats>() : null;
            if (stats != null) multiplier = stats.LootMultiplier;

            foreach (var entry in table.entries)
            {
                if (entry == null || entry.item == null) continue;
                if (Random.value > Mathf.Min(1f, entry.dropChance * multiplier)) continue;
                int amount = Random.Range(entry.min, entry.max + 1);
                Spawn(pickupPrefab, transform.position, scatterRadius, entry.item, amount);
            }
        }

        /// <summary>Legt einen Gegenstand verstreut um einen Punkt auf den Boden.</summary>
        public static Pickup Spawn(Pickup prefab, Vector2 center, float scatter, ItemData item, int amount)
        {
            if (prefab == null || item == null || amount <= 0) return null;
            Vector2 position = center + Random.insideUnitCircle * scatter;
            var pickup = Instantiate(prefab, position, Quaternion.identity);
            pickup.Setup(item, amount);
            return pickup;
        }
    }
}
