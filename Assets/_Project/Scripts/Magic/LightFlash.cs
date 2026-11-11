using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Reborn.Magic
{
    /// <summary>Laesst ein 2D-Licht kurz aufblitzen und ausblenden (Explosion, Zauber-Leuchten).</summary>
    [RequireComponent(typeof(Light2D))]
    public class LightFlash : MonoBehaviour
    {
        public float duration = 0.4f;

        Light2D _light;
        float _start;
        float _time;

        void Awake()
        {
            _light = GetComponent<Light2D>();
            _start = _light.intensity;
        }

        void Update()
        {
            _time += Time.deltaTime;
            _light.intensity = _start * Mathf.Clamp01(1f - _time / duration);
        }
    }
}
