using Reborn.Animation;
using Reborn.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Player
{
    /// <summary>Bewegung und Ausweichen des Spielers (Top-Down).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public InputActionAsset actions;
        public float moveSpeed = 5f;
        public float dodgeSpeed = 12f;
        public float dodgeDuration = 0.2f;
        public float dodgeCooldown = 0.6f;
        public float knockbackDuration = 0.15f;
        /// <summary>So lange zeigt die Figur nach einem Treffer die Treffer-Animation</summary>
        public float hurtDuration = 0.2f;

        public Vector2 FacingDirection { get; private set; } = Vector2.down;
        public bool IsDodging => _dodgeTimer > 0f;
        /// <summary>Wird waehrend eines Angriffs gesetzt.</summary>
        public bool Locked { get; set; }
        /// <summary>Wird von PlayerMana gesetzt: Spieler steht still und kann nichts anderes tun.</summary>
        public bool Meditating { get; set; }
        public bool IsCasting => _castTimer > 0f;
        /// <summary>Gerade mit etwas anderem beschaeftigt (Angriff, Ausweichen, Zaubern, Meditieren)</summary>
        public bool Busy => Locked || IsDodging || IsCasting || Meditating;

        float _castTimer;

        /// <summary>Kurz stehen bleiben, waehrend die Zauber-Animation laeuft.</summary>
        public void StartCast(float duration)
        {
            _castTimer = Mathf.Max(_castTimer, duration);
            _input = Vector2.zero;
        }

        Rigidbody2D _body;
        Health _health;
        DirectionalSpriteAnimator _animator;
        InputActionMap _map;
        InputAction _move;
        InputAction _dodge;

        Vector2 _input;
        Vector2 _dodgeDirection;
        Vector2 _knockback;
        float _dodgeTimer;
        float _cooldownTimer;
        float _knockbackTimer;
        float _hurtTimer;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
            _map = actions.FindActionMap("Gameplay", true);
            _move = _map.FindAction("Move", true);
            _dodge = _map.FindAction("Dodge", true);
        }

        void OnEnable()
        {
            _map.Enable();
            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
                _health.OnDied += HandleDied;
            }
        }

        void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
                _health.OnDied -= HandleDied;
            }
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            _knockback = knockback;
            _knockbackTimer = knockbackDuration;
            // Treffer-Animation (nicht beim Sterben, das uebernimmt HandleDied)
            if (_health != null && !_health.IsDead && _animator != null)
            {
                _hurtTimer = hurtDuration;
                _animator.Play("hurt", true);
            }
        }

        /// <summary>Tod: Sterbe-Animation, letztes Bild bleibt stehen.</summary>
        void HandleDied()
        {
            _hurtTimer = 0f;
            _input = Vector2.zero;
            if (_animator != null) _animator.Play("death", true);
        }

        void Update()
        {
            if (_health != null && _health.IsDead)
            {
                _input = Vector2.zero;
                return;
            }
            if (_hurtTimer > 0f) _hurtTimer -= Time.deltaTime;

            if (_castTimer > 0f || Meditating)
            {
                if (_castTimer > 0f) _castTimer -= Time.deltaTime;
                _input = Vector2.zero;
                if (Meditating && _animator != null) _animator.Play("idle");
                return;
            }

            _input = Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);
            bool moving = _input.sqrMagnitude > 0.01f;
            _cooldownTimer -= Time.deltaTime;

            if (_dodgeTimer > 0f)
            {
                _dodgeTimer -= Time.deltaTime;
                if (_dodgeTimer <= 0f && _health != null) _health.invulnerable = false;
            }
            else if (_dodge.WasPressedThisFrame() && _cooldownTimer <= 0f && !Locked)
            {
                StartDodge(moving ? _input.normalized : FacingDirection);
                return;
            }

            if (IsDodging || Locked) return;

            if (moving)
            {
                FacingDirection = _input.normalized;
                if (_animator != null) _animator.SetFacing(FacingDirection);
            }
            if (_animator != null && _hurtTimer <= 0f) _animator.Play(moving ? "walk" : "idle");
        }

        void StartDodge(Vector2 direction)
        {
            _dodgeDirection = direction;
            FacingDirection = direction;
            _dodgeTimer = dodgeDuration;
            Core.Sfx.Play(Core.SfxId.Dodge, 0.7f);
            _cooldownTimer = dodgeDuration + dodgeCooldown;
            if (_health != null) _health.invulnerable = true;
            if (_animator != null)
            {
                _animator.SetFacing(direction);
                _animator.Play("dodge", true);
            }
        }

        void FixedUpdate()
        {
            Vector2 velocity;
            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= Time.fixedDeltaTime;
                velocity = _knockback;
            }
            else if (IsDodging) velocity = _dodgeDirection * dodgeSpeed;
            else if (Locked) velocity = _input * (moveSpeed * 0.3f);
            else velocity = _input * moveSpeed;
            _body.linearVelocity = velocity;
        }
    }
}
