using Reborn.Combat;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Wurzelangriff des Golems (Meilenstein 8): erst eine rote Warnflaeche, nach delay brechen Wurzeln heraus.
    /// Steht der Spieler dann im Radius, bekommt er Schaden. Ausweichen schuetzt (Unverwundbarkeit beim Ausweichen).
    /// </summary>
    public class RootSpike : MonoBehaviour
    {
        public int damage = 25;
        public float delay = 0.8f;
        public float radius = 0.7f;
        public float lifetime = 1.5f;
        public SpriteRenderer warning;
        public SpriteRenderer spike;
        public Sprite[] spikeFrames = new Sprite[0];

        /// <summary>Wurzel-Treffer seit Spielstart (fuer Tests)</summary>
        public static int HitCount { get; private set; }
        /// <summary>Ausgeloeste Wurzeln seit Spielstart (fuer Tests)</summary>
        public static int StrikeCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { HitCount = 0; StrikeCount = 0; }

        float _time;
        bool _struck;

        void Start()
        {
            if (warning != null) warning.enabled = true;
            if (spike != null) spike.enabled = false;
        }

        void Update()
        {
            _time += Time.deltaTime;
            if (!_struck && _time >= delay) Strike();
            if (_struck && spike != null && spikeFrames.Length > 0)
                spike.sprite = spikeFrames[Mathf.Min(spikeFrames.Length - 1, (int)((_time - delay) / 0.06f))];
            if (_time >= lifetime) Destroy(gameObject);
        }

        void Strike()
        {
            _struck = true;
            StrikeCount++;
            if (warning != null) warning.enabled = false;
            if (spike != null) spike.enabled = true;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            if (Vector2.Distance(player.transform.position, transform.position) > radius) return;
            var hp = player.GetComponent<Health>();
            if (hp != null && hp.TakeDamage(damage, Vector2.zero)) HitCount++;
        }
    }
}
