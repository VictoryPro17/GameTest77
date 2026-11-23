using UnityEngine;
using UnityEngine.UI;

namespace Reborn.World
{
    /// <summary>
    /// Sprechblase ueber einem NPC (Meilenstein 7b). Show() zeigt einen Satz fuer duration Sekunden.
    /// Die Blase ist ein kleines Welt-Canvas ueber dem Kopf und wird beim ersten Zeigen angelegt.
    /// </summary>
    public class SpeechBubble : MonoBehaviour
    {
        public float height = 1.15f;
        public float duration = 3f;

        GameObject _root;
        Text _text;
        float _timer;

        public bool IsVisible => _root != null && _root.activeSelf;
        public string CurrentText => IsVisible ? _text.text : "";

        public void Show(string message)
        {
            Build();
            _text.text = message;
            _root.SetActive(true);
            _timer = duration;
        }

        void Update()
        {
            if (!IsVisible) return;
            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f) _root.SetActive(false);
        }

        void Build()
        {
            if (_root != null) return;
            _root = new GameObject("SpeechBubble", typeof(RectTransform));
            _root.transform.SetParent(transform, false);
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;
            var rt = (RectTransform)_root.transform;
            rt.sizeDelta = new Vector2(420f, 130f);
            rt.localScale = Vector3.one * 0.006f;
            rt.localPosition = new Vector3(0f, height, 0f);

            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(_root.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var img = bg.AddComponent<Image>();
            img.color = new Color(0.98f, 0.95f, 0.86f, 0.95f);
            var outline = bg.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.14f, 0.1f, 1f);
            outline.effectDistance = new Vector2(4f, -4f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(_root.transform, false);
            var tRt = (RectTransform)textGo.transform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = new Vector2(-30f, -20f);
            _text = textGo.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontSize = 30;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.color = new Color(0.15f, 0.1f, 0.08f, 1f);
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
            _root.SetActive(false);
        }
    }
}
