using System;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Golem-Boss "Waldwaechter" (Meilenstein 8). Schlaeft, bis der Spieler naeher als wakeRadius kommt.
    /// Nah: Schlag (Ausholen, dann Treffer im Kreis vor ihm). Fern: Wurzelangriff (Warnflaeche unter dem Spieler, ausweichbar).
    /// 50 % Leben: Bruellen, Woelfe, schneller (Phase 2). 25 %: Wut, Wurzeln an 3 Stellen, schnellerer Schlag (Phase 3).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public class BossAI : MonoBehaviour
    {
        enum State { Sleep, Chase, Windup, Strike, Root, Roar, Dead }

        public string bossId = "waldwaechter";
        public string displayName = "Waldwächter";
        public float wakeRadius = 4f;
        public float speed = 1.2f;
        public float speedPhase2 = 1.8f;
        public int contactDamage = 20;
        public float contactCooldown = 1f;
        public int slamDamage = 30;
        public float slamRange = 1.7f;
        public float slamRadius = 1.2f;
        public float slamWindup = 0.5f;
        public float slamWindupRage = 0.3f;
        public float slamCooldown = 2.2f;
        public float slamCooldownPhase2 = 1.4f;
        public float rootMinDistance = 2.4f;
        public float rootCooldown = 4f;
        public float rootCooldownPhase2 = 2.6f;
        public int xpReward = 600;
        /// <summary>Mitte des Koerpers ueber dem Fusspunkt (fuer den Schlag)</summary>
        public Vector2 bodyOffset = new Vector2(0f, 0.6f);
        public RootSpike rootSpikePrefab;
        public GameObject wolfPrefab;
        public int wolvesPhase2 = 3;
        public float despawnDelay = 3f;

        /// <summary>Wacher, lebender Boss in der Szene (fuer die Lebensleiste), sonst null</summary>
        public static BossAI Active { get; private set; }
        /// <summary>Boss besiegt: ID (SaveSystem merkt es sich)</summary>
        public static event Action<string> OnDefeated;

        public int Phase { get; private set; } = 1;
        public bool IsAwake => _state != State.Sleep;
        public string StateName => _state.ToString();
        public Health Health => _health;
        public int SlamCount { get; private set; }
        public int RootCount { get; private set; }

        State _state = State.Sleep;
        Rigidbody2D _body;
        Health _health;
        DirectionalSpriteAnimator _animator;
        Transform _player;
        Health _playerHealth;
        float _timer;
        float _slamTimer;
        float _rootTimer = 1.5f;
        float _contactTimer;
        Vector2 _slamDirection = Vector2.down;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = null;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
        }

        void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            _health.OnDied += Die;
        }

        void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            _health.OnDied -= Die;
            if (Active == this) Active = null;
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _player = player.transform;
            _playerHealth = player.GetComponent<Health>();
        }

        /// <summary>Schon besiegt (Spielstand): Boss entfernen, ohne Beute</summary>
        public void RemoveDefeated()
        {
            _state = State.Dead;
            if (Active == this) Active = null;
            Destroy(gameObject);
        }

        /// <summary>Sofort aufwecken (auch fuer Tests)</summary>
        public void Wake()
        {
            if (_state != State.Sleep) return;
            Active = this;
            StartRoar();
        }

        void StartRoar()
        {
            _state = State.Roar;
            _timer = 1.0f;
            _body.linearVelocity = Vector2.zero;
            if (_animator != null) _animator.Play("roar", true);
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            if (_health.IsDead) return;
            if (_state == State.Sleep) Wake();
            float ratio = (float)_health.Current / _health.maxHealth;
            if (Phase == 1 && ratio <= 0.5f)
            {
                Phase = 2;
                StartRoar();
                SpawnWolves();
            }
            else if (Phase == 2 && ratio <= 0.25f)
            {
                Phase = 3;
                StartRoar();
            }
        }

        void SpawnWolves()
        {
            if (wolfPrefab == null) return;
            for (int i = 0; i < wolvesPhase2; i++)
            {
                float a = i * Mathf.PI * 2f / wolvesPhase2;
                var pos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.6f - 0.4f) * 1.8f;
                var wolf = Instantiate(wolfPrefab, pos, Quaternion.identity);
                wolf.name = "Boss_Wolf_" + i;
            }
        }

        void Update()
        {
            _contactTimer -= Time.deltaTime;
            _slamTimer -= Time.deltaTime;
            _rootTimer -= Time.deltaTime;
            if (_state == State.Dead) return;
            bool playerAlive = _player != null && (_playerHealth == null || !_playerHealth.IsDead);
            Vector2 toPlayer = playerAlive ? (Vector2)(_player.position - transform.position) : Vector2.zero;
            float dist = toPlayer.magnitude;

            switch (_state)
            {
                case State.Sleep:
                    if (_animator != null) _animator.Play("idle");
                    if (playerAlive && dist <= wakeRadius) Wake();
                    return;
                case State.Roar:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f) _state = State.Chase;
                    return;
                case State.Windup:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f) Strike();
                    return;
                case State.Strike:
                case State.Root:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f) _state = State.Chase;
                    return;
            }

            // Chase
            if (!playerAlive)
            {
                if (_animator != null) _animator.Play("idle");
                return;
            }
            if (_animator != null)
            {
                _animator.SetFacing(toPlayer);
                _animator.Play("walk");
            }
            if (dist <= slamRange && _slamTimer <= 0f) StartSlam(toPlayer);
            else if (dist >= rootMinDistance && _rootTimer <= 0f) StartRoots();
        }

        void StartSlam(Vector2 toPlayer)
        {
            _state = State.Windup;
            _slamDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : Vector2.down;
            _timer = Phase >= 3 ? slamWindupRage : slamWindup;
            if (_animator != null)
            {
                _animator.SetFacing(_slamDirection);
                _animator.Play("attack", true);
            }
        }

        /// <summary>Mitte des Schlag-Kreises (fuer Tests und Gizmos)</summary>
        public Vector2 SlamCenter => (Vector2)transform.position + bodyOffset * 0.5f + _slamDirection * 0.9f;

        void Strike()
        {
            _state = State.Strike;
            _timer = 0.35f;
            SlamCount++;
            _slamTimer = Phase >= 2 ? slamCooldownPhase2 : slamCooldown;
            if (_playerHealth == null) return;
            Vector2 playerBody = (Vector2)_player.position + new Vector2(0f, 0.3f);
            if (Vector2.Distance(playerBody, SlamCenter) <= slamRadius)
                _playerHealth.TakeDamage(slamDamage, _slamDirection * 7f);
        }

        void StartRoots()
        {
            _state = State.Root;
            _timer = 0.6f;
            RootCount++;
            _rootTimer = Phase >= 2 ? rootCooldownPhase2 : rootCooldown;
            if (_animator != null) _animator.Play("rootattack", true);
            if (rootSpikePrefab == null || _player == null) return;
            Instantiate(rootSpikePrefab, _player.position, Quaternion.identity);
            if (Phase >= 3)
                foreach (var off in new[] { new Vector2(1.3f, 0.6f), new Vector2(-1.3f, 0.6f) })
                    Instantiate(rootSpikePrefab, (Vector2)_player.position + off, Quaternion.identity);
        }

        void FixedUpdate()
        {
            if (_state == State.Chase && _player != null && (_playerHealth == null || !_playerHealth.IsDead))
                _body.linearVelocity = ((Vector2)(_player.position - transform.position)).normalized * (Phase >= 2 ? speedPhase2 : speed);
            else
                _body.linearVelocity = Vector2.zero;
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (_state == State.Dead || _state == State.Sleep || _contactTimer > 0f) return;
            var target = collision.collider.GetComponentInParent<Health>();
            if (target == null || target != _playerHealth) return;
            Vector2 direction = ((Vector2)(collision.transform.position - transform.position)).normalized;
            if (target.TakeDamage(contactDamage, direction * 6f)) _contactTimer = contactCooldown;
        }

        void Die()
        {
            _state = State.Dead;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            if (Active == this) Active = null;
            if (_animator != null) _animator.Play("death", true);
            var dropper = GetComponent<LootDropper>();
            if (dropper != null) dropper.Drop();
            if (_player != null)
            {
                var progression = _player.GetComponent<PlayerProgression>();
                if (progression != null) progression.AddXP(xpReward);
            }
            OnDefeated?.Invoke(bossId);
            Destroy(gameObject, despawnDelay);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, wakeRadius);
            Gizmos.DrawWireSphere(SlamCenter, slamRadius);
        }
    }
}
