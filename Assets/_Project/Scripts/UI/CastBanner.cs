using Reborn.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Grosser Ausruf in der Bildmitte bei starken Zaubern (Wasserdrache, Flammenring).</summary>
    public class CastBanner : MonoBehaviour
    {
        public Text title;
        public Text subtitle;
        public float duration = 1.6f;

        SpellBook _book;
        float _time;

        public string CurrentText => _time > 0f && title != null ? title.text : "";

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _book = player.GetComponent<SpellBook>();
            if (_book != null) _book.OnCast += Show;
            SetVisible(false);
        }

        void OnDestroy()
        {
            if (_book != null) _book.OnCast -= Show;
        }

        void Show(SpellData spell, int level)
        {
            if (!spell.bigChant) return;
            var color = SpellData.ElementColor(spell.element);
            if (title != null)
            {
                title.text = spell.spellName.ToUpperInvariant() + "!";
                title.color = color;
            }
            if (subtitle != null)
            {
                subtitle.text = SpellData.ElementPrefix(spell.element) + "  ·  Stufe " + level;
                subtitle.color = Color.Lerp(color, Color.white, 0.5f);
            }
            _time = duration;
            SetVisible(true);
        }

        void SetVisible(bool visible)
        {
            if (title != null) title.enabled = visible;
            if (subtitle != null) subtitle.enabled = visible;
        }

        void Update()
        {
            if (_time <= 0f) return;
            _time -= Time.deltaTime;
            if (_time <= 0f)
            {
                SetVisible(false);
                return;
            }
            float age = duration - _time;
            float pop = age < 0.2f ? Mathf.Lerp(1.5f, 1f, age / 0.2f) : 1f;
            float alpha = Mathf.Clamp01(_time / 0.4f);
            if (title != null)
            {
                title.rectTransform.localScale = Vector3.one * pop;
                var c = title.color; c.a = alpha; title.color = c;
            }
            if (subtitle != null)
            {
                var c = subtitle.color; c.a = alpha; subtitle.color = c;
            }
        }
    }
}
