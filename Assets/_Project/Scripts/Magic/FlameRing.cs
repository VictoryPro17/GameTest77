using System.Collections.Generic;
using Reborn.Combat;
using Reborn.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reborn.Magic
{
    /// <summary>Flammenring: breitet sich um den Spieler aus, trifft jeden Gegner einmal und stoesst ihn weg.</summary>
    public class FlameRing : SpellEffect
    {
        public Transform ringVisual;
        public ParticleSystem flames;
        public Light2D glow;
        public float growTime = 0.35f;
        public float fadeTime = 0.45f;
        public float startRadius = 0.3f;

        readonly HashSet<Health> _hit = new HashSet<Health>();
        SpriteRenderer _ringRenderer;
        float _time;
        float _glowIntensity;

        protected override void OnLaunch()
        {
            if (ringVisual != null) _ringRenderer = ringVisual.GetComponent<SpriteRenderer>();
            if (glow != null) _glowIntensity = glow.intensity;
            CameraFollow.Shake(0.1f, 0.25f);
            Apply(startRadius, 1f);
        }

        float EndRadius => Cast.data.radius * Cast.scale;

        void Apply(float radius, float alpha)
        {
            if (ringVisual != null) ringVisual.localScale = Vector3.one * radius * 2f;
            if (_ringRenderer != null)
            {
                var c = _ringRenderer.color;
                c.a = alpha;
                _ringRenderer.color = c;
            }
            if (flames != null)
            {
                var shape = flames.shape;
                shape.radius = radius;
            }
            if (glow != null)
            {
                glow.pointLightOuterRadius = radius + 1f;
                glow.intensity = _glowIntensity * alpha;
            }
        }

        void Update()
        {
            _time += Time.deltaTime;
            float t = Mathf.Clamp01(_time / growTime);
            float radius = Mathf.Lerp(startRadius, EndRadius, 1f - (1f - t) * (1f - t));
            float alpha = _time <= growTime ? 1f : 1f - Mathf.Clamp01((_time - growTime) / fadeTime);
            Apply(radius, alpha);

            if (_time <= growTime)
            {
                Vector2 center = transform.position;
                foreach (var col in Physics2D.OverlapCircleAll(center, radius, Cast.enemyMask))
                {
                    var target = col.GetComponentInParent<Health>();
                    if (target == null || target.IsDead || !_hit.Add(target)) continue;
                    Vector2 away = (Vector2)target.transform.position - center;
                    if (away.sqrMagnitude < 0.0001f) away = Cast.direction;
                    Hit(target, Cast.damage, away.normalized * Cast.data.knockback, true);
                }
            }
            if (_time >= growTime + fadeTime)
            {
                ReleaseTrails();
                Destroy(gameObject);
            }
        }
    }
}
