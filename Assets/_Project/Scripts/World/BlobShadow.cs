using Reborn.Combat;
using UnityEngine;

namespace Reborn.World
{
    /// <summary>
    /// Weicher, ovaler Schatten unter einer Figur (Meilenstein 5).
    /// Liegt als eigenes Kind unter dem Fusspunkt und wird ueber dem Boden, aber unter allen Figuren gezeichnet.
    /// Stirbt die Figur, verblasst der Schatten.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlobShadow : MonoBehaviour
    {
        public const int SortingOrder = -1;
        [Range(0f, 1f)] public float alpha = 0.35f;
        public float fadeTime = 0.4f;

        SpriteRenderer _renderer;
        Health _health;
        float _fade = 1f;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.sortingOrder = SortingOrder;
            _health = GetComponentInParent<Health>();
            Apply();
        }

        void Update()
        {
            float target = _health != null && _health.IsDead ? 0f : 1f;
            if (Mathf.Approximately(_fade, target)) return;
            _fade = Mathf.MoveTowards(_fade, target, Time.deltaTime / Mathf.Max(0.01f, fadeTime));
            Apply();
        }

        void Apply()
        {
            var c = _renderer.color;
            c.a = alpha * _fade;
            _renderer.color = c;
        }
    }
}
