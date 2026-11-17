using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reborn.World
{
    /// <summary>
    /// Szenenwechsel mit Abdunkeln (Meilenstein 6): speichern, schwarz werden, Szene laden, wieder aufhellen.
    /// Der Spieler erscheint am SpawnPoint mit der ID PendingSpawn. Ueberlebt den Szenenwechsel (DontDestroyOnLoad).
    /// </summary>
    public class SceneTravel : MonoBehaviour
    {
        public const string BaseScene = "Base";
        public const string FightScene = "Room_Test";

        /// <summary>Ziel-SpawnPoint in der naechsten Szene (null = Standardposition)</summary>
        public static string PendingSpawn { get; set; }
        public static bool IsTraveling { get; private set; }
        /// <summary>Anzahl abgeschlossener Wechsel (fuer Tests)</summary>
        public static int TravelCount { get; private set; }

        Image _black;
        Text _message;

        /// <summary>Das Projekt laedt den Code beim Play-Start nicht neu: statische Werte hier zuruecksetzen</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            PendingSpawn = null;
            IsTraveling = false;
            TravelCount = 0;
        }

        /// <summary>Wechselt die Szene. message wird vor dem Abdunkeln delay Sekunden gezeigt (z. B. beim Tod).</summary>
        public static void Go(string scene, string spawn, float fadeTime = 0.4f, string message = null, float delay = 0f)
        {
            if (IsTraveling) return;
            IsTraveling = true;
            var go = new GameObject("SceneTravel");
            DontDestroyOnLoad(go);
            var travel = go.AddComponent<SceneTravel>();
            travel.Build();
            travel.StartCoroutine(travel.Run(scene, spawn, fadeTime, message, delay));
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            var black = new GameObject("Black", typeof(RectTransform));
            black.transform.SetParent(transform, false);
            var rt = (RectTransform)black.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            _black = black.AddComponent<Image>();
            _black.color = new Color(0f, 0f, 0f, 0f);
            var text = new GameObject("Message", typeof(RectTransform));
            text.transform.SetParent(transform, false);
            ((RectTransform)text.transform).sizeDelta = new Vector2(1600, 160);
            _message = text.AddComponent<Text>();
            _message.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _message.fontSize = 84;
            _message.alignment = TextAnchor.MiddleCenter;
            _message.color = new Color(0.9f, 0.25f, 0.25f, 1f);
            _message.text = "";
            var outline = text.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        IEnumerator Run(string scene, string spawn, float fadeTime, string message, float delay)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _message.text = message;
                yield return new WaitForSecondsRealtime(delay);
            }
            var player = GameObject.FindGameObjectWithTag("Player");
            var save = player != null ? player.GetComponent<Save.SaveSystem>() : null;
            if (save != null) save.Save();

            yield return Fade(0f, 1f, fadeTime);
            PendingSpawn = spawn;
            Time.timeScale = 1f;
            var load = SceneManager.LoadSceneAsync(scene);
            while (load != null && !load.isDone) yield return null;
            _message.text = "";
            TravelCount++;
            IsTraveling = false;
            yield return Fade(1f, 0f, fadeTime * 0.6f);
            Destroy(gameObject);
        }

        IEnumerator Fade(float from, float to, float time)
        {
            for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
            {
                _black.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / time));
                yield return null;
            }
            _black.color = new Color(0f, 0f, 0f, to);
        }

        void OnDestroy()
        {
            // Falls die Reise abgebrochen wurde (Play-Modus beendet): Zustand freigeben
            if (IsTraveling && _black != null && _black.color.a < 1f) IsTraveling = false;
        }
    }
}
