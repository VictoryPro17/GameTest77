using System.Collections.Generic;
using Reborn.Core;
using Reborn.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Meilenstein 10a: zeigt beim Betreten einer Szene gross den Ortsnamen (blendet ein und aus)
    /// und unten rechts kurz "Gespeichert", wenn das Spiel speichert.
    /// </summary>
    public class LocationBanner : MonoBehaviour
    {
        public Text titleText;
        public Text savedText;
        public float holdTime = 2.2f;
        public float fadeTime = 0.6f;

        public static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { "Base", "Die Stadt" }, { "Room_Test", "Kampfraum" }, { "Cave", "Höhle" }, { "Forest", "Wald" }, { "Frost", "Frostgipfel" },
            { "Guild_Warrior", "Kriegergilde" }, { "Guild_Mage", "Magiergilde" }, { "Guild_Assassin", "Assassinengilde" },
        };

        float _titleTime = -1f;
        float _savedTime = -1f;

        public string CurrentTitle => titleText != null ? titleText.text : "";
        public bool SavedVisible => savedText != null && savedText.color.a > 0.01f;

        void OnEnable() => SaveSystem.OnSaved += HandleSaved;
        void OnDisable() => SaveSystem.OnSaved -= HandleSaved;

        void Start()
        {
            if (titleText != null)
            {
                titleText.text = Names.TryGetValue(SceneManager.GetActiveScene().name, out var n) ? Loc.T(n) : "";
                _titleTime = titleText.text != "" ? 0f : -1f;
            }
            SetAlpha(titleText, 0f);
            SetAlpha(savedText, 0f);
        }

        void HandleSaved()
        {
            if (savedText == null) return;
            savedText.text = Loc.T("Gespeichert");
            _savedTime = 0f;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_titleTime >= 0f)
            {
                _titleTime += dt;
                SetAlpha(titleText, Curve(_titleTime));
                if (_titleTime > holdTime + fadeTime * 2f) _titleTime = -1f;
            }
            if (_savedTime >= 0f)
            {
                _savedTime += dt;
                SetAlpha(savedText, Curve(_savedTime * 1.6f));
                if (_savedTime * 1.6f > holdTime + fadeTime * 2f) _savedTime = -1f;
            }
        }

        float Curve(float t) => t < fadeTime ? t / fadeTime : t < fadeTime + holdTime ? 1f : Mathf.Max(0f, 1f - (t - fadeTime - holdTime) / fadeTime);

        static void SetAlpha(Text t, float a)
        {
            if (t == null) return;
            var c = t.color;
            c.a = a;
            t.color = c;
        }
    }
}
