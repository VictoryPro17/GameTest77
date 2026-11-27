using System.Collections;
using System.Collections.Generic;
using Reborn.Animation;
using Reborn.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.Combat
{
    /// <summary>Nahkampf-Angriff mit Trefferkreis vor dem Spieler.</summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAttack : MonoBehaviour
    {
        public InputActionAsset actions;
        public int damage = 10;
        public float cooldown = 0.4f;
        public float range = 0.55f;
        public float radius = 0.45f;
        public float knockback = 4f;
        public float hitDelay = 0.08f;
        public float lockTime = 0.25f;
        /// <summary>Mitte des Koerpers ueber dem Pivot (Fuesse)</summary>
        public Vector2 bodyOffset = new Vector2(0f, 0.35f);
        public LayerMask enemyMask;

        PlayerController _controller;
        Health _health;
        DirectionalSpriteAnimator _animator;
        InputAction _attack;
        float _cooldownTimer;
        readonly HashSet<Health> _hitThisSwing = new HashSet<Health>();

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _health = GetComponent<Health>();
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
            var map = actions.FindActionMap("Gameplay", true);
            _attack = map.FindAction("Attack", true);
        }

        void OnEnable()
        {
            _attack.actionMap.Enable();
        }

        void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (_health != null && _health.IsDead) return;
            if (World.SceneRules.Peaceful) return;   // Basis (Meilenstein 6): kein Kampf
            if (_attack.WasPressedThisFrame() && _cooldownTimer <= 0f && !_controller.Busy)
            {
                StartCoroutine(DoAttack());
            }
        }

        IEnumerator DoAttack()
        {
            _cooldownTimer = cooldown;
            _controller.Locked = true;
            if (_animator != null) _animator.Play("attack", true);
            Core.Sfx.Play(Core.SfxId.Swing, 0.8f);

            yield return new WaitForSeconds(hitDelay);

            Vector2 direction = _controller.FacingDirection;
            Vector2 center = (Vector2)transform.position + bodyOffset + direction * range;
            _hitThisSwing.Clear();
            foreach (var hit in Physics2D.OverlapCircleAll(center, radius, enemyMask))
            {
                var target = hit.GetComponentInParent<Health>();
                if (target == null || target == _health || !_hitThisSwing.Add(target)) continue;
                target.TakeDamage(damage, direction * knockback);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, lockTime - hitDelay));
            _controller.Locked = false;
        }

        void OnDrawGizmosSelected()
        {
            var controller = GetComponent<PlayerController>();
            Vector2 direction = controller != null && Application.isPlaying ? controller.FacingDirection : Vector2.down;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + bodyOffset + direction * range, radius);
        }
    }
}
