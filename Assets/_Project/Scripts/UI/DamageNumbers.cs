using System.Collections.Generic;
using Reborn.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Schadenszahlen ueber Getroffenen: weiss, bei Zaubern in der Elementfarbe, beim Spieler rot.
    /// Steigen auf und blenden aus.
    /// </summary>
    public class DamageNumbers : MonoBehaviour
    {
        /// <summary>Vorlage (ausgeschaltet); wird fuer jede Zahl kopiert</summary>
        public Text template;
        public float duration = 0.8f;
        public float rise = 60f;
        public Color normalColor = Color.white;
        public Color playerColor = new Color(1f, 0.35f, 0.3f);

        /// <summary>Wird von Zaubern kurz vor dem Treffer gesetzt (Elementfarbe), danach zurueckgesetzt.</summary>
        public static Color? PendingColor;

        class Entry
        {
            public Text text;
            public Vector3 world;
            public float time;
        }

        readonly List<Entry> _active = new List<Entry>();
        readonly Stack<Text> _pool = new Stack<Text>();
        RectTransform _canvas;

        /// <summary>Fuer Tests: zuletzt angezeigte Zahl</summary>
        public string LastText { get; private set; } = "";
        public int ActiveCount => _active.Count;

        void Awake()
        {
            _canvas = (RectTransform)GetComponentInParent<Canvas>().transform;
            if (template != null) template.gameObject.SetActive(false);
            Health.OnAnyDamaged += Show;
        }

        void OnDestroy()
        {
            Health.OnAnyDamaged -= Show;
        }

        void Show(Health target, int amount)
        {
            if (template == null || target == null) return;
            var text = _pool.Count > 0 ? _pool.Pop() : Instantiate(template, template.transform.parent);
            text.gameObject.SetActive(true);
            text.text = amount.ToString();
            text.color = target.CompareTag("Player") ? playerColor : PendingColor ?? normalColor;
            LastText = text.text;
            var jitter = new Vector3(Random.Range(-0.15f, 0.15f), 0f, 0f);
            _active.Add(new Entry { text = text, world = target.transform.position + new Vector3(0f, 0.9f, 0f) + jitter, time = duration });
            Place(_active[_active.Count - 1]);
        }

        void LateUpdate()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var e = _active[i];
                e.time -= Time.deltaTime;
                if (e.time <= 0f)
                {
                    e.text.gameObject.SetActive(false);
                    _pool.Push(e.text);
                    _active.RemoveAt(i);
                    continue;
                }
                Place(e);
            }
        }

        void Place(Entry e)
        {
            var cam = Camera.main;
            if (cam == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, cam.WorldToScreenPoint(e.world), null, out var local);
            float progress = 1f - e.time / duration;
            e.text.rectTransform.anchoredPosition = local + Vector2.up * rise * progress;
            e.text.rectTransform.localScale = Vector3.one * (progress < 0.15f ? Mathf.Lerp(1.4f, 1f, progress / 0.15f) : 1f);
            var c = e.text.color;
            c.a = Mathf.Clamp01(e.time / 0.3f);
            e.text.color = c;
        }
    }
}
