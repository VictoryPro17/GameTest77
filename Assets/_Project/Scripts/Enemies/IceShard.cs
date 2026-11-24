using Reborn.Combat;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Eissplitter des Frostgeists (Meilenstein 9a): fliegt geradeaus und trifft den Spieler einmal.
    /// Ohne Physik: Abstand zum Koerper des Spielers wird jedes Bild geprueft. Ausweichen (Unverwundbarkeit) schuetzt.
    /// </summary>
    public class IceShard : MonoBehaviour
    {
        public int damage = 10;
        public float speed = 6f;
        public float lifetime = 2.5f;
        public float hitRadius = 0.4f;

        public static int ShotCount { get; private set; }
        public static int HitCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { ShotCount = 0; HitCount = 0; }

        Vector2 _direction = Vector2.right;
        float _time;
        Transform _player;
        Health _playerHealth;

        public void Launch(Vector2 direction, int dmg)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            damage = dmg;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
            ShotCount++;
        }

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            _player = p.transform;
            _playerHealth = p.GetComponent<Health>();
        }

        void Update()
        {
            _time += Time.deltaTime;
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);
            if (_player != null && _playerHealth != null && !_playerHealth.IsDead)
            {
                Vector2 body = (Vector2)_player.position + new Vector2(0f, 0.3f);
                if (Vector2.Distance(body, transform.position) <= hitRadius)
                {
                    if (_playerHealth.TakeDamage(damage, _direction * 4f)) HitCount++;
                    Destroy(gameObject);
                    return;
                }
            }
            if (_time >= lifetime) Destroy(gameObject);
        }
    }
}
