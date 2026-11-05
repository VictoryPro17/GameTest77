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

        public Vector2 FacingDirection { get; private set; } = Vector2.down;
        public bool IsDodging => _dodgeTimer > 0f;
        /// <summary>Wird waehrend eines Angriffs gesetzt.</summary>
        public bool Locked { get; set; }

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
            if (_health != null) _health.OnDamaged += HandleDamaged;
        }

        void OnDisable()
        {
            if (_health != null) _health.OnDamaged -= HandleDamaged;
        }

        void HandleDamaged(int amount, Vector2 knockback)
        {
            _knockback = knockback;
            _knockbackTimer = knockbackDuration;
        }

        void Update()
        {
            if (_health != null && _health.IsDead)
            {
                _input = Vector2.zero;
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
            if (_animator != null) _animator.Play(moving ? "walk" : "idle");
        }

        void StartDodge(Vector2 direction)
        {
            _dodgeDirection = direction;
            FacingDirection = direction;
            _dodgeTimer = dodgeDuration;
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
