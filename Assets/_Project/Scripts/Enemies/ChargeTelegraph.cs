using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>Ziel-Linie des Ansturms (Meilenstein 8b): roter Streifen am Boden in Laufrichtung, blinkt bis zum Start.</summary>
    public class ChargeTelegraph : MonoBehaviour
    {
        public SpriteRenderer line;
        /// <summary>Laenge des Streifen-Bildes bei Massstab 1 (in Einheiten)</summary>
        public float spriteLength = 1.9f;

        float _life;
        float _time;

        public void Setup(Vector2 direction, float length, float life)
        {
            _life = life;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            float s = length / Mathf.Max(0.01f, spriteLength);
            transform.localScale = new Vector3(s, 1f, 1f);
        }

        void Update()
        {
            _time += Time.deltaTime;
            if (line != null) line.color = new Color(1f, 1f, 1f, 0.55f + 0.4f * Mathf.Abs(Mathf.Sin(_time * 12f)));
            if (_time >= _life) Destroy(gameObject);
        }
    }
}
