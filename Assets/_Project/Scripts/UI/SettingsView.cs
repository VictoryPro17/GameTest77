using System;
using Reborn.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Einstellungs-Seite (Meilenstein 10a), im Hauptmenue und im Pause-Menue. Jede Option ist ein Knopf,
    /// der den Wert weiterschaltet (gut mit Maus, Tastatur, Controller und Touch). Aenderungen gelten sofort und
    /// werden gespeichert. Unten die Tastenbelegung (RebindingView) und "Zurueck".
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        public GameObject root;
        public Button languageButton;
        public Text languageValue;
        public Button masterButton;
        public Text masterValue;
        public Button sfxButton;
        public Text sfxValue;
        public Button fullscreenButton;
        public Text fullscreenValue;
        public Button qualityButton;
        public Text qualityValue;
        public Button frameRateButton;
        public Text frameRateValue;
        public Button uiScaleButton;
        public Text uiScaleValue;
        public Button damageNumbersButton;
        public Text damageNumbersValue;
        public Button mobileButton;
        public Text mobileValue;
        public Button backButton;
        public RebindingView rebinding;

        public static bool AnyOpen { get; private set; }
        public bool IsOpen => root != null && root.activeSelf;

        static readonly float[] Volumes = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        static readonly int[] FrameRates = { 30, 60, -1 };
        static readonly float[] UiScales = { 0.9f, 1f, 1.15f };

        Action _onClose;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => AnyOpen = false;

        void Awake()
        {
            Bind(languageButton, () => GameSettings.Change(d => d.language = d.language == "en" ? "de" : "en"));
            Bind(masterButton, () => GameSettings.Change(d => d.masterVolume = Next(Volumes, d.masterVolume)));
            Bind(sfxButton, () => GameSettings.Change(d => d.sfxVolume = Next(Volumes, d.sfxVolume)));
            Bind(fullscreenButton, () => GameSettings.Change(d => d.fullscreen = !d.fullscreen));
            Bind(qualityButton, () => GameSettings.Change(d => d.quality = (d.quality + 1) % 3));
            Bind(frameRateButton, () => GameSettings.Change(d => d.frameRate = FrameRates[(Array.IndexOf(FrameRates, d.frameRate) + 1) % FrameRates.Length]));
            Bind(uiScaleButton, () => GameSettings.Change(d => d.uiScale = Next(UiScales, d.uiScale)));
            Bind(damageNumbersButton, () => GameSettings.Change(d => d.damageNumbers = !d.damageNumbers));
            Bind(mobileButton, () => GameSettings.Change(d => d.mobileControls = (d.mobileControls + 1) % 3));
            if (backButton != null) backButton.onClick.AddListener(Close);
            if (root != null) root.SetActive(false);
        }

        static void Bind(Button b, Action a)
        {
            if (b != null) b.onClick.AddListener(() => a());
        }

        static float Next(float[] values, float current)
        {
            int best = 0;
            for (int i = 0; i < values.Length; i++)
                if (Mathf.Abs(values[i] - current) < Mathf.Abs(values[best] - current)) best = i;
            return values[(best + 1) % values.Length];
        }

        void OnEnable() => GameSettings.OnChanged += Refresh;
        void OnDisable() => GameSettings.OnChanged -= Refresh;

        void OnDestroy()
        {
            if (IsOpen) AnyOpen = false;
        }

        public void Open(Action onClose = null)
        {
            _onClose = onClose;
            if (root != null) root.SetActive(true);
            AnyOpen = true;
            Refresh();
            if (EventSystem.current != null && languageButton != null) EventSystem.current.SetSelectedGameObject(languageButton.gameObject);
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (rebinding != null && rebinding.IsRebinding) return;
            root.SetActive(false);
            AnyOpen = false;
            var cb = _onClose;
            _onClose = null;
            cb?.Invoke();
        }

        /// <summary>Menue-Taste: schliessen, ausser waehrend gerade eine Taste neu belegt wird</summary>
        public void HandleBack()
        {
            if (rebinding != null && rebinding.IsRebinding) return;
            Close();
        }

        static string OnOff(bool v) => Loc.T(v ? "An" : "Aus");

        public void Refresh()
        {
            var d = GameSettings.Current;
            Set(languageValue, GameSettings.English ? "English" : "Deutsch");
            Set(masterValue, Mathf.RoundToInt(d.masterVolume * 100f) + " %");
            Set(sfxValue, Mathf.RoundToInt(d.sfxVolume * 100f) + " %");
            Set(fullscreenValue, OnOff(d.fullscreen));
            Set(qualityValue, Loc.T(d.quality == 0 ? "Niedrig" : d.quality == 1 ? "Mittel" : "Hoch"));
            Set(frameRateValue, d.frameRate < 0 ? Loc.T("Unbegrenzt") : d.frameRate + " FPS");
            Set(uiScaleValue, Mathf.RoundToInt(d.uiScale * 100f) + " %");
            Set(damageNumbersValue, OnOff(d.damageNumbers));
            Set(mobileValue, Loc.T(d.mobileControls == 1 ? "An" : d.mobileControls == 2 ? "Aus" : "Automatisch"));
        }

        static void Set(Text t, string s)
        {
            if (t != null) t.text = s;
        }
    }
}
