using UnityEngine;

namespace Reborn.Core
{
    /// <summary>Kamera folgt dem Ziel weich. Shake() laesst sie kurz wackeln (starke Zauber).</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.15f;
        public Vector3 offset = new Vector3(0f, 0.4f, -10f);

        static CameraFollow _instance;
        Vector3 _velocity;
        Vector3 _base;
        float _shakeAmplitude;
        float _shakeTime;
        float _shakeDuration;

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
            if (target != null) _base = target.position + offset;
            transform.position = _base;
        }

        /// <summary>Kamera kurz wackeln lassen (Staerke in Einheiten, Dauer in Sekunden).</summary>
        public static void Shake(float amplitude, float duration)
        {
            if (_instance == null) return;
            if (amplitude >= _instance._shakeAmplitude * (_instance._shakeTime / Mathf.Max(0.01f, _instance._shakeDuration)))
            {
                _instance._shakeAmplitude = amplitude;
                _instance._shakeDuration = duration;
                _instance._shakeTime = duration;
            }
        }

        void LateUpdate()
        {
            if (target == null) return;
            _base = Vector3.SmoothDamp(_base, target.position + offset, ref _velocity, smoothTime);
            Vector3 shake = Vector3.zero;
            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                float strength = _shakeAmplitude * Mathf.Clamp01(_shakeTime / Mathf.Max(0.01f, _shakeDuration));
                shake = (Vector3)(Random.insideUnitCircle * strength);
            }
            transform.position = _base + shake;
        }
    }
}
