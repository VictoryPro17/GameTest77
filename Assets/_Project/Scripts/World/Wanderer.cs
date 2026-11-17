using Reborn.Animation;
using UnityEngine;

namespace Reborn.World
{
    /// <summary>
    /// Stadtbewohner (Meilenstein 6.1): laeuft langsam zwischen festen Wegpunkten hin und her, wartet an jedem Punkt
    /// kurz und spielt dabei "walk" bzw. "idle". Ohne Collider, damit er den Spieler nie einsperrt.
    /// </summary>
    public class Wanderer : MonoBehaviour
    {
        /// <summary>Wegpunkte in Weltkoordinaten (der Startpunkt ist der erste)</summary>
        public Vector2[] waypoints = new Vector2[0];
        public float speed = 1.1f;
        public Vector2 waitTime = new Vector2(1.5f, 3.5f);
        public DirectionalSpriteAnimator animator;

        /// <summary>Strecke seit dem Start (fuer Tests)</summary>
        public float Travelled { get; private set; }

        int _target;
        float _wait;

        void Start()
        {
            if (animator == null) animator = GetComponentInChildren<DirectionalSpriteAnimator>();
            if (waypoints.Length > 0) transform.position = waypoints[0];
            _target = waypoints.Length > 1 ? 1 : 0;
            _wait = Random.Range(0f, waitTime.y);
            if (animator != null) animator.Play("idle");
        }

        void Update()
        {
            if (waypoints.Length < 2) return;
            if (_wait > 0f)
            {
                _wait -= Time.deltaTime;
                if (animator != null) animator.Play("idle");
                return;
            }
            Vector2 pos = transform.position;
            Vector2 goal = waypoints[_target];
            Vector2 delta = goal - pos;
            float step = speed * Time.deltaTime;
            if (delta.magnitude <= step)
            {
                transform.position = goal;
                Travelled += delta.magnitude;
                _target = (_target + 1) % waypoints.Length;
                _wait = Random.Range(waitTime.x, waitTime.y);
                return;
            }
            Vector2 move = delta.normalized * step;
            transform.position = pos + move;
            Travelled += step;
            if (animator != null)
            {
                animator.SetFacing(move);
                animator.Play("walk");
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
                Gizmos.DrawLine(waypoints[i], waypoints[(i + 1) % waypoints.Length]);
        }
    }
}
