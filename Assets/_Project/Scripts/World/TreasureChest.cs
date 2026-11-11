using System;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.World
{
    /// <summary>Schatztruhe: oeffnet sich, wenn der Spieler sie beruehrt, und verstreut ihre Beute davor.</summary>
    public class TreasureChest : MonoBehaviour
    {
        [Serializable]
        public class Content
        {
            public ItemData item;
            public int count = 1;
        }

        public string chestId = "truhe";
        public Sprite closedSprite;
        public Sprite openSprite;
        public Content[] contents = Array.Empty<Content>();
        public Pickup pickupPrefab;
        /// <summary>Wo die Beute landet (relativ zur Truhe, vor der Truhe)</summary>
        public Vector2 dropOffset = new Vector2(0f, -0.6f);
        public float scatter = 0.35f;

        public bool IsOpen { get; private set; }

        public static event Action<TreasureChest> OnAnyOpened;

        SpriteRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            UpdateSprite();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsOpen && collision.collider.CompareTag("Player")) Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            UpdateSprite();
            Vector2 center = (Vector2)transform.position + dropOffset;
            foreach (var content in contents)
                if (content != null) LootDropper.Spawn(pickupPrefab, center, scatter, content.item, content.count);
            OnAnyOpened?.Invoke(this);
        }

        /// <summary>Spielstand: Truhe ist schon offen, ohne Beute.</summary>
        public void SetOpenedSilently()
        {
            IsOpen = true;
            UpdateSprite();
        }

        void UpdateSprite()
        {
            if (_renderer == null) return;
            var sprite = IsOpen ? openSprite : closedSprite;
            if (sprite != null) _renderer.sprite = sprite;
        }
    }
}
