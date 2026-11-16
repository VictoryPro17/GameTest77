using System.Collections;
using UnityEngine;

namespace Reborn.Combat
{
    /// <summary>Laesst die Figur kurz aufblitzen: rot bei Treffer, gruen bei Heilung.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        public Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        public Color healColor = new Color(0.45f, 1f, 0.45f, 1f);
        public float duration = 0.1f;
        public float healDuration = 0.25f;

        SpriteRenderer _renderer;
        Health _health;
        Color _normal;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _normal = _renderer.color;
            _health = GetComponentInParent<Health>();
        }

        void OnEnable()
        {
            if (_health == null) return;
            _health.OnDamaged += HandleDamaged;
            _health.OnHealed += HandleHealed;
        }

        void OnDisable()
        {
            if (_health == null) return;
            _health.OnDamaged -= HandleDamaged;
            _health.OnHealed -= HandleHealed;
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            StopAllCoroutines();
            StartCoroutine(Flash(flashColor, duration));
        }

        void HandleHealed(int amount)
        {
            StopAllCoroutines();
            StartCoroutine(Flash(healColor, healDuration));
        }

        IEnumerator Flash(Color color, float time)
        {
            var layers = GetComponent<Reborn.Animation.EquipmentVisuals>();
            _renderer.color = color;
            if (layers != null) layers.SyncColor();
            yield return new WaitForSecondsRealtime(time);
            _renderer.color = _normal;
            if (layers != null) layers.SyncColor();
        }
    }
}
