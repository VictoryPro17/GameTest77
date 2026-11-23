using Reborn.Animation;
using Reborn.Combat;
using Reborn.Loot;
using Reborn.Progression;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>
    /// Kreaturen-KI (Waldschleim, ab Meilenstein 7c auch Schattenwolf und Riesenspinne): wartet, verfolgt den Spieler
    /// und macht Beruehrungsschaden. Werte pro Art setzt der Builder; der Name bleibt fuer die Tests.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public class SlimeAI : MonoBehaviour
    {
        enum State { Idle, Chase, Hurt, Dead }

        /// <summary>Art fuer Quests: "schleim", "wolf", "spinne"</summary>
        public string creatureId = "schleim";
        /// <summary>Angriffs-Animation bei einem Treffer zeigen (Wolf, Spinne)</summary>
        public bool playAttack;
        public float attackDuration = 0.35f;

        /// <summary>Eine Kreatur wurde besiegt: Art, durch einen Zauber (Meilenstein 7c, fuer Quests)</summary>
        public static event System.Action<string, bool> OnAnyKilled;

        float _attackTimer;

        public float detectRadius = 5f;
        public float speed = 2f;
        public int contactDamage = 5;
        public float contactCooldown = 1f;
        public float contactKnockback = 5f;
        public float hurtDuration = 0.25f;
        public int xpReward = 30;
        /// <summary>Sterbe-Animation: 4 Bilder bei 12 Bildern/s = 0,33 s, danach kurz liegen bleiben</summary>
        public float despawnDelay = 0.45f;

        State _state = State.Idle;
        Rigidbody2D _body;
        Health _health;
        DirectionalSpriteAnimator _animator;
        Transform _player;
        Health _playerHealth;
        Vector2 _knockback;
        float _hurtTimer;
        float _contactTimer;

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
        }

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _player = player.transform;
                _playerHealth = player.GetComponent<Health>();
            }
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            if (_health.IsDead) return;
            _state = State.Hurt;
            _hurtTimer = hurtDuration;
            _knockback = knockback;
            if (_animator != null) _animator.Play("hurt", true);
        }

        void Update()
        {
            _contactTimer -= Time.deltaTime;
            if (_state == State.Dead) return;

            if (_state == State.Hurt)
            {
                _hurtTimer -= Time.deltaTime;
                if (_hurtTimer > 0f) return;
                _state = State.Idle;
            }

            bool playerAlive = _player != null && (_playerHealth == null || !_playerHealth.IsDead);
            _state = playerAlive && Vector2.Distance(transform.position, _player.position) <= detectRadius
                ? State.Chase
                : State.Idle;

            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
                if (_state == State.Chase && _animator != null) _animator.SetFacing(_player.position - transform.position);
                return;
            }
            if (_animator != null)
            {
                _animator.Play(_state == State.Chase ? "walk" : "idle");
                if (_state == State.Chase) _animator.SetFacing(_player.position - transform.position);
            }
        }

        void FixedUpdate()
        {
            switch (_state)
            {
                case State.Chase:
                    _body.linearVelocity = ((Vector2)(_player.position - transform.position)).normalized * speed;
                    break;
                case State.Hurt:
                    _body.linearVelocity = _knockback;
                    _knockback *= 0.8f;
                    break;
                default:
                    _body.linearVelocity = Vector2.zero;
                    break;
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (_state == State.Dead || _contactTimer > 0f) return;
            var target = collision.collider.GetComponentInParent<Health>();
            if (target == null || target != _playerHealth) return;
            Vector2 direction = ((Vector2)(collision.transform.position - transform.position)).normalized;
            if (target.TakeDamage(contactDamage, direction * contactKnockback))
            {
                _contactTimer = contactCooldown;
                if (playAttack && _animator != null)
                {
                    _animator.SetFacing(direction);
                    _animator.Play("attack", true);
                    _attackTimer = attackDuration;
                }
            }
        }

        void Die()
        {
            _state = State.Dead;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            if (_animator != null) _animator.Play("death", true);

            var dropper = GetComponent<LootDropper>();
            if (dropper != null) dropper.Drop();
            if (_player != null)
            {
                var progression = _player.GetComponent<PlayerProgression>();
                if (progression != null) progression.AddXP(xpReward);
            }
            OnAnyKilled?.Invoke(creatureId, _health.LastHitBySpell);
            Destroy(gameObject, despawnDelay);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}
