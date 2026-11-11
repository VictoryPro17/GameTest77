using Reborn.Combat;
using Reborn.Core;
using UnityEngine;

namespace Reborn.Magic
{
    /// <summary>Wasserkugel und Feuerball: fliegt geradeaus, trifft den ersten Gegner; der Feuerball explodiert.</summary>
    public class SpellProjectile : SpellEffect
    {
        public bool explode;
        /// <summary>Wird in Flugrichtung gedreht</summary>
        public Transform visual;
        public float spinSpeed;

        Vector2 _dir;
        float _travelled;
        bool _done;

        protected override void OnLaunch()
        {
            _dir = Cast.direction.sqrMagnitude > 0.01f ? Cast.direction.normalized : Vector2.down;
            transform.localScale = Vector3.one * Cast.scale;
            if (visual != null) visual.rotation = Quaternion.Euler(0f, 0f, Angle(_dir));
        }

        void Update()
        {
            if (_done) return;
            float step = Cast.data.speed * Time.deltaTime;
            transform.position += (Vector3)(_dir * step);
            _travelled += step;
            if (visual != null && spinSpeed != 0f) visual.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            Vector2 pos = transform.position;
            foreach (var col in Physics2D.OverlapCircleAll(pos, Cast.data.radius * Cast.scale, Cast.enemyMask))
            {
                var target = col.GetComponentInParent<Health>();
                if (target != null && !target.IsDead) { Impact(target); return; }
            }
            if (Physics2D.OverlapPoint(pos, Cast.wallMask) != null || _travelled >= Cast.data.range) Impact(null);
        }

        void Impact(Health target)
        {
            _done = true;
            Vector2 pos = transform.position;
            if (target != null) Hit(target, Cast.damage, _dir * Cast.data.knockback, true);
            if (explode)
            {
                foreach (var col in Physics2D.OverlapCircleAll(pos, Cast.data.splashRadius * Cast.scale, Cast.enemyMask))
                {
                    var other = col.GetComponentInParent<Health>();
                    if (other == null || other == target || other.IsDead) continue;
                    Vector2 away = ((Vector2)other.transform.position - pos).normalized;
                    Hit(other, Cast.splashDamage, away * Cast.data.knockback, false);
                }
                SpawnHit(pos, 1.6f);
                CameraFollow.Shake(0.08f, 0.2f);
            }
            else if (target == null)
            {
                SpawnHit(pos, 0.7f);
            }
            ReleaseTrails();
            Destroy(gameObject);
        }
    }
}
