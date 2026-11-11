using Reborn.Inventory;
using UnityEngine;

namespace Reborn.Loot
{
    /// <summary>Gegenstand am Boden. Wird bei Beruehrung durch den Spieler eingesammelt, soweit Platz ist.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Pickup : MonoBehaviour
    {
        public ItemData item;
        public int count = 1;
        public float pickupDelay = 0.3f;

        SpriteRenderer _renderer;
        float _spawnTime;

        void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _spawnTime = Time.time;
        }

        public void Setup(ItemData data, int amount)
        {
            item = data;
            count = amount;
            if (_renderer != null && data != null && data.icon != null) _renderer.sprite = data.icon;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time - _spawnTime < pickupDelay) return;
            var inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null || item == null) return;
            int rest = inventory.Add(item, count);
            if (rest <= 0) Destroy(gameObject);
            else count = rest;
        }
    }
}
