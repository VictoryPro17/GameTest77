using UnityEngine;

namespace Reborn.Magic
{
    /// <summary>Spielt Bilder im Kreis ab (Feuerball flackert).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteLoop : MonoBehaviour
    {
        public Sprite[] frames = new Sprite[0];
        public float fps = 10f;

        SpriteRenderer _renderer;
        float _time;

        void Awake() => _renderer = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (frames.Length == 0) return;
            _time += Time.deltaTime;
            _renderer.sprite = frames[(int)(_time * fps) % frames.Length];
        }
    }
}
