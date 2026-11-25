using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reborn.Core
{
    /// <summary>
    /// Einstellungen (Meilenstein 10a): Sprache, Ton, Grafik, Oberflaeche und Tastenbelegung.
    /// Getrennt vom Spielstand in PlayerPrefs gespeichert (funktioniert auch im Browser).
    /// Tests rufen UseSessionDefaults(): Standardwerte nur fuer diese Sitzung, nichts wird gespeichert.
    /// </summary>
    public static class GameSettings
    {
        public const string PrefsKey = "reborn.settings";
        public const string Version = "Beta 0.9.0";

        [Serializable]
        public class Data
        {
            public string language = "";
            public float masterVolume = 0.8f;
            public float sfxVolume = 0.8f;
            public bool fullscreen;
            /// <summary>0 niedrig, 1 mittel, 2 hoch</summary>
            public int quality = 2;
            /// <summary>30, 60 oder -1 (unbegrenzt)</summary>
            public int frameRate = 60;
            /// <summary>0,9 / 1 / 1,15</summary>
            public float uiScale = 1f;
            public bool damageNumbers = true;
            /// <summary>0 automatisch, 1 an, 2 aus</summary>
            public int mobileControls;
            public string bindings = "";
        }

        public static Data Current { get; private set; } = new Data();
        /// <summary>Tests: Standardwerte, Speichern aus</summary>
        public static bool SessionDefaults { get; private set; }
        public static InputActionAsset Actions { get; private set; }

        public static event Action OnChanged;

        public static string Language => Current.language == "en" ? "en" : "de";
        public static bool English => Language == "en";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Current = new Data();
            SessionDefaults = false;
            Actions = null;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            Load();
            SceneManager.sceneLoaded += HandleSceneLoaded;
            Apply();
        }

        static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyUiScale();

        public static void Load()
        {
            var data = new Data();
            if (!SessionDefaults)
            {
                try
                {
                    string json = PlayerPrefs.GetString(PrefsKey, "");
                    if (!string.IsNullOrEmpty(json)) data = JsonUtility.FromJson<Data>(json) ?? new Data();
                }
                catch (Exception e) { Debug.LogWarning("Einstellungen konnten nicht gelesen werden: " + e.Message); }
            }
            if (string.IsNullOrEmpty(data.language))
                data.language = Application.systemLanguage == SystemLanguage.German ? "de" : "en";
            Current = data;
        }

        public static void Save()
        {
            if (SessionDefaults) return;
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
        }

        /// <summary>Fuer Tests: Standardwerte (Deutsch), nur fuer diese Sitzung</summary>
        public static void UseSessionDefaults()
        {
            SessionDefaults = true;
            Current = new Data { language = "de" };
            if (Actions != null) Actions.RemoveAllBindingOverrides();
            Apply();
        }

        /// <summary>Aendern, anwenden und speichern</summary>
        public static void Change(Action<Data> change)
        {
            change(Current);
            Apply();
            Save();
        }

        public static void Apply()
        {
            AudioListener.volume = Mathf.Clamp01(Current.masterVolume);
            Application.targetFrameRate = Current.frameRate;
            QualitySettings.vSyncCount = 0;
            int levels = QualitySettings.names.Length;
            if (levels > 0)
            {
                int level = Mathf.Clamp(Mathf.RoundToInt(Current.quality / 2f * (levels - 1)), 0, levels - 1);
                if (QualitySettings.GetQualityLevel() != level) QualitySettings.SetQualityLevel(level, true);
            }
#if !UNITY_EDITOR
            if (Screen.fullScreen != Current.fullscreen) Screen.fullScreen = Current.fullscreen;
#endif
            ApplyUiScale();
            OnChanged?.Invoke();
        }

        /// <summary>Alle Canvas mit "an Bildschirm anpassen" bekommen die Oberflaechen-Groesse</summary>
        public static void ApplyUiScale()
        {
            float s = Mathf.Clamp(Current.uiScale, 0.8f, 1.3f);
            foreach (var scaler in UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include))
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    scaler.referenceResolution = new Vector2(1920f, 1080f) / s;
        }

        /// <summary>Steuerung anmelden (Menue und Hauptmenue rufen das auf): gespeicherte Belegung anwenden</summary>
        public static void RegisterActions(InputActionAsset actions)
        {
            if (actions == null || Actions == actions) return;
            Actions = actions;
            actions.RemoveAllBindingOverrides();
            if (!SessionDefaults && !string.IsNullOrEmpty(Current.bindings))
            {
                try { actions.LoadBindingOverridesFromJson(Current.bindings); }
                catch (Exception e) { Debug.LogWarning("Tastenbelegung konnte nicht geladen werden: " + e.Message); }
            }
        }

        /// <summary>Aktuelle Belegung speichern (nach dem Neubelegen)</summary>
        public static void StoreBindings()
        {
            if (Actions == null) return;
            Current.bindings = Actions.SaveBindingOverridesAsJson();
            Save();
            OnChanged?.Invoke();
        }

        public static void ResetBindings()
        {
            if (Actions != null) Actions.RemoveAllBindingOverrides();
            Current.bindings = "";
            Save();
            OnChanged?.Invoke();
        }

        /// <summary>Handy-Knoepfe zeigen? (automatisch: nur mit Touchscreen)</summary>
        public static bool ShowMobileControls(bool autoValue) => Current.mobileControls == 1 || (Current.mobileControls == 0 && autoValue);
    }
}
