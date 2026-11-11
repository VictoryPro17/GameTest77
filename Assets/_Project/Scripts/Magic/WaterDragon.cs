using System.Collections.Generic;
using Reborn.Combat;
using Reborn.Core;
using UnityEngine;

namespace Reborn.Magic
{
    /// <summary>
    /// Wasserdrache: Kopf und Koerperteile folgen einer Schlangenlinie in Blickrichtung,
    /// durchdringen Gegner und treffen jeden genau einmal.
    /// </summary>
    public class WaterDragon : SpellEffect
    {
        public Transform head;
        public Transform[] segments = new Transform[0];
        public float wave = 0.6f;
        public float waveFrequency = 1.4f;
        public float spacing = 0.3f;

        readonly HashSet<Health> _hit = new HashSet<Health>();
        Vector2 _origin, _dir, _perp;
        float _distance;
        SpriteRenderer[] _renderers;

        protected override void OnLaunch()
        {
            _origin = Cast.origin;
            _dir = Cast.direction.sqrMagnitude > 0.01f ? Cast.direction.normalized : Vector2.down;
            _perp = new Vector2(-_dir.y, _dir.x);
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            transform.position = Vector3.zero;
            CameraFollow.Shake(0.12f, 0.3f);
            Place();
        }

        Vector2 Point(float s)
        {
            s = Mathf.Max(0f, s);
            float fadeIn = Mathf.Clamp01(s / 1.2f);
            return _origin + _dir * s + _perp * Mathf.Sin(s * waveFrequency) * wave * Cast.scale * fadeIn;
        }

        void Place()
        {
            float range = Cast.data.range;
            float headS = Mathf.Min(_distance, range);
            Vector2 headPos = Point(headS);
            Vector2 ahead = Point(headS + 0.05f) - headPos;
            if (head != null)
            {
                head.position = headPos;
                head.rotation = Quaternion.Euler(0f, 0f, Angle(ahead.sqrMagnitude > 0f ? ahead : _dir));
                float flip = ahead.x < 0f ? -1f : 1f;
                head.localScale = new Vector3(Cast.scale, Cast.scale * flip, 1f);
            }
            for (int k = 0; k < segments.Length; k++)
            {
                // Koerperteile laufen hinter dem Kopf her und sammeln sich am Ende der Strecke
                float s = Mathf.Min(_distance - (k + 1) * spacing * Cast.scale, range);
                bool visible = s > 0f;
                segments[k].gameObject.SetActive(visible);
                if (!visible) continue;
                segments[k].position = Point(s);
                float size = Cast.scale * (1f - k * 0.07f);
                segments[k].localScale = new Vector3(size, size, 1f);
            }
        }

        void Update()
        {
            _distance += Cast.data.speed * Time.deltaTime;
            Place();

            if (_distance <= Cast.data.range && head != null)
            {
                foreach (var col in Physics2D.OverlapCircleAll(head.position, Cast.data.radius * Cast.scale, Cast.enemyMask))
                {
                    var target = col.GetComponentInParent<Health>();
                    if (target == null || target.IsDead || !_hit.Add(target)) continue;
                    Hit(target, Cast.damage, _dir * Cast.data.knockback, true);
                }
            }

            // Der Schwanz ist am Ende angekommen: ausblenden und aufraeumen
            float tailDone = Cast.data.range + (segments.Length + 1) * spacing * Cast.scale;
            if (_distance > Cast.data.range)
            {
                float fade = 1f - Mathf.Clamp01((_distance - Cast.data.range) / (tailDone - Cast.data.range + 0.01f));
                foreach (var r in _renderers)
                {
                    var c = r.color;
                    c.a = fade;
                    r.color = c;
                }
            }
            if (_distance >= tailDone)
            {
                if (head != null) SpawnHit(head.position, 1.2f);
                ReleaseTrails();
                Destroy(gameObject);
            }
        }
    }
}
