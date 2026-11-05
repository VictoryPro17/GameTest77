using System.Collections;
using UnityEngine;

namespace Reborn.Combat
{
    /// <summary>Laesst die Figur kurz aufblitzen, wenn sie getroffen wird.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        public Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        public float duration = 0.1f;

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
            if (_health != null) _health.OnDamaged += HandleDamaged;
        }

        void OnDisable()
        {
            if (_health != null) _health.OnDamaged -= HandleDamaged;
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }

        IEnumerator Flash()
        {
            _renderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            _renderer.color = _normal;
        }
    }
}
