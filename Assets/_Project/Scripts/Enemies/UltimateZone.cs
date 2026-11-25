using Reborn.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.Enemies
{
    /// <summary>
    /// Ultimate "Zorn des Waldes" (Meilenstein 8b): grosser roter Kreis um den Boss, waechst waehrend der Aufladung
    /// auf den vollen Radius und pulsiert; dazu der Hinweis "Zorn des Waldes!" im Bild. Den Schaden macht BossAI.
    /// </summary>
    public class UltimateZone : MonoBehaviour
    {
        public float radius = 5.5f;
        /// <summary>Radius des Kreis-Bildes bei Massstab 1 (in Einheiten)</summary>
        public float spriteRadius = 0.9f;
        public SpriteRenderer circle;
        public Text warningText;

        public static int ReleaseCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => ReleaseCount = 0;

        float _charge = 4f;
        float _time;
        bool _released;

        public void Setup(float chargeTime)
        {
            _charge = chargeTime;
            Scale(0.3f);
        }

        void Scale(float k)
        {
            float s = radius * k / Mathf.Max(0.01f, spriteRadius);
            if (circle != null) circle.transform.localScale = new Vector3(s, s, 1f);
        }

        void Update()
        {
            _time += Time.deltaTime;
            if (_released)
            {
                if (circle != null) circle.color = new Color(1f, 0.9f, 0.6f, Mathf.Max(0f, 1f - (_time - _charge) * 3f));
                if (_time > _charge + 0.4f) Destroy(gameObject);
                return;
            }
            float k = Mathf.Clamp01(_time / Mathf.Max(0.01f, _charge));
            Scale(Mathf.Lerp(0.3f, 1f, Mathf.Min(1f, k * 1.5f)));
            if (circle != null) circle.color = new Color(1f, 1f, 1f, 0.45f + 0.4f * Mathf.Abs(Mathf.Sin(_time * (4f + 10f * k))));
            if (warningText != null) warningText.text = Loc.F("Zorn des Waldes! {0:0.0}", Mathf.Max(0f, _charge - _time));
        }

        /// <summary>Ausloesen (BossAI): kurzer heller Blitz, dann weg</summary>
        public void Release()
        {
            _released = true;
            _time = _charge;
            ReleaseCount++;
            Scale(1f);
            if (warningText != null) warningText.text = "";
        }
    }
}
