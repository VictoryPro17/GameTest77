using Reborn.Combat;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Dornenwelle (Meilenstein 8b): ein Dornenring waechst vom Boss aus von startRadius bis maxRadius.
    /// Trifft den Spieler einmal, wenn der Ring ueber ihn hinweggeht (Ausweichen bzw. weit genug weg schuetzt).
    /// </summary>
    public class ThornWave : MonoBehaviour
    {
        public int damage = 25;
        public float startRadius = 0.6f;
        public float maxRadius = 4.5f;
        public float duration = 1.2f;
        /// <summary>Halbe Ringbreite, in der der Spieler getroffen wird</summary>
        public float thickness = 0.45f;
        /// <summary>Radius des Ring-Bildes bei Massstab 1 (in Einheiten)</summary>
        public float spriteRadius = 0.81f;
        public SpriteRenderer ring;
        public Sprite[] frames = new Sprite[0];

        public static int HitCount { get; private set; }
        public static int WaveCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { HitCount = 0; WaveCount = 0; }

        float _time;
        bool _hit;
        Transform _player;
        Health _playerHealth;

        void Start()
        {
            WaveCount++;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) { _player = p.transform; _playerHealth = p.GetComponent<Health>(); }
            Apply(startRadius);
        }

        void Apply(float radius)
        {
            float s = radius / Mathf.Max(0.01f, spriteRadius);
            transform.localScale = new Vector3(s, s, 1f);
        }

        void Update()
        {
            _time += Time.deltaTime;
            float k = Mathf.Clamp01(_time / duration);
            float radius = Mathf.Lerp(startRadius, maxRadius, k);
            Apply(radius);
            if (ring != null && frames.Length > 0) ring.sprite = frames[Mathf.Min(frames.Length - 1, (int)(k * frames.Length))];
            if (!_hit && _player != null && _playerHealth != null && !_playerHealth.IsDead)
            {
                float d = Vector2.Distance(_player.position, transform.position);
                if (Mathf.Abs(d - radius) <= thickness)
                {
                    _hit = true;   // pro Welle hoechstens ein Versuch: wer ausweicht, ist durch
                    if (_playerHealth.TakeDamage(damage, ((Vector2)(_player.position - transform.position)).normalized * 5f)) HitCount++;
                }
            }
            if (_time >= duration + 0.2f) Destroy(gameObject);
        }
    }
}
