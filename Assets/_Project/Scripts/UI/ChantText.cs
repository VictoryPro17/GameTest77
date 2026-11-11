using Reborn.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Ausruf ueber dem Kopf des Spielers, z. B. "Wasser-Erschaffungsmagie: Wasserkugel!". Steigt auf und blendet aus.</summary>
    public class ChantText : MonoBehaviour
    {
        public Text text;
        public float duration = 1.5f;
        public float rise = 40f;
        public Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);

        SpellBook _book;
        Transform _player;
        RectTransform _canvas;
        float _time;

        /// <summary>Gerade sichtbarer Ausruf (leer, wenn keiner)</summary>
        public string CurrentText => _time > 0f && text != null ? text.text : "";

        void Start()
        {
            _canvas = (RectTransform)GetComponentInParent<Canvas>().transform;
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _player = player.transform;
                _book = player.GetComponent<SpellBook>();
            }
            if (_book != null) _book.OnCast += Show;
            if (text != null) text.enabled = false;
        }

        void OnDestroy()
        {
            if (_book != null) _book.OnCast -= Show;
        }

        void Show(SpellData spell, int level)
        {
            if (text == null) return;
            text.text = spell.Chant;
            text.color = SpellData.ElementColor(spell.element);
            text.enabled = true;
            _time = duration;
            Place();
        }

        void LateUpdate()
        {
            if (_time <= 0f || text == null) return;
            _time -= Time.deltaTime;
            if (_time <= 0f)
            {
                text.enabled = false;
                return;
            }
            Place();
        }

        void Place()
        {
            var cam = Camera.main;
            if (cam == null || _player == null) return;
            Vector2 screen = cam.WorldToScreenPoint(_player.position + worldOffset);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, screen, null, out var local);
            float progress = 1f - _time / duration;
            text.rectTransform.anchoredPosition = local + Vector2.up * rise * progress;
            var c = text.color;
            c.a = Mathf.Clamp01(_time / 0.4f);
            text.color = c;
        }
    }
}
