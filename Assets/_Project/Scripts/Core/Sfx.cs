using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reborn.Core
{
    public enum SfxId
    {
        Swing, Hit, Hurt, Dodge, Cast, Pickup, Coin, Click, Step, LevelUp, Roar, Portal, Death, Potion, Defeat
    }

    /// <summary>
    /// Spielt Soundeffekte (Meilenstein 10b) ueber einen kleinen Pool von AudioSources, der Szenenwechsel ueberlebt.
    /// Lautstaerke = Effekt-Regler; der Gesamt-Regler wirkt ueber AudioListener.volume (GameSettings.Apply).
    /// Treffer und Tod kommen automatisch ueber Health.OnAnyDamaged, Knoepfe klicken nach jedem Szenenwechsel.
    /// </summary>
    public static class Sfx
    {
        const int Voices = 10;
        const float MinGap = 0.045f;

        static AudioSource[] _sources;
        static int _next;
        static readonly Dictionary<SfxId, float> LastTime = new Dictionary<SfxId, float>();
        static readonly Dictionary<SfxId, int> Counts = new Dictionary<SfxId, int>();

        public static int Played { get; private set; }
        /// <summary>Wegen Effekt-Lautstaerke 0 nicht gespielt</summary>
        public static int Muted { get; private set; }
        public static SfxId LastId { get; private set; }
        public static float LastVolume { get; private set; }
        public static int CountOf(SfxId id) => Counts.TryGetValue(id, out var c) ? c : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _sources = null;
            _next = 0;
            LastTime.Clear();
            Counts.Clear();
            Played = 0;
            Muted = 0;
            Combat.Health.OnAnyDamaged -= HandleDamaged;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            Combat.Health.OnAnyDamaged -= HandleDamaged;
            Combat.Health.OnAnyDamaged += HandleDamaged;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HookButtons();
        }

        public static void Play(SfxId id, float volume = 1f, float pitchJitter = 0.06f)
        {
            if (!Application.isPlaying) return;
            float now = Time.unscaledTime;
            if (LastTime.TryGetValue(id, out var last) && now - last < MinGap) return;
            LastTime[id] = now;
            float vol = GameSettings.Current.sfxVolume * volume;
            if (vol <= 0.001f) { Muted++; return; }
            EnsureSources();
            var src = _sources[_next];
            _next = (_next + 1) % Voices;
            src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            src.PlayOneShot(SoundBank.Get(id), vol);
            Played++;
            Counts[id] = CountOf(id) + 1;
            LastId = id;
            LastVolume = vol;
        }

        static void EnsureSources()
        {
            if (_sources != null && _sources[0] != null) return;
            var host = new GameObject("Sfx");
            Object.DontDestroyOnLoad(host);
            _sources = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++)
            {
                var s = host.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                _sources[i] = s;
            }
        }

        static void HandleDamaged(Combat.Health target, int amount)
        {
            if (target == null) return;
            bool player = target.CompareTag("Player");
            if (target.IsDead) Play(player ? SfxId.Death : SfxId.Defeat);
            else Play(player ? SfxId.Hurt : SfxId.Hit, player ? 1f : 0.85f);
        }

        static void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => HookButtons();

        /// <summary>Allen Knoepfen der Szene (auch versteckten) einen Klick-Ton geben</summary>
        public static void HookButtons()
        {
            foreach (var b in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (b.GetComponent<ClickSoundMarker>() != null) continue;
                b.gameObject.AddComponent<ClickSoundMarker>();
                b.onClick.AddListener(() => Play(SfxId.Click, 0.7f, 0.02f));
            }
        }
    }
}
