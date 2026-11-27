using System.IO;
using Reborn.Core;
using Reborn.Save;
using Reborn.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Hauptmenue (Meilenstein 10a), eigene Startszene "MainMenu": Weiter (nur mit Spielstand), Neues Spiel (mit Nachfrage),
    /// Einstellungen, Neuigkeiten, Mitwirkende, Beenden (nicht im Browser). "Neues Spiel" fordert die Einfuehrung an (10b).
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        public const string SceneName = "MainMenu";

        public InputActionAsset actions;
        public GameObject mainPanel;
        public Button continueButton;
        public Button newGameButton;
        public Button settingsButton;
        public Button newsButton;
        public Button creditsButton;
        public Button quitButton;
        public Text versionText;
        public SettingsView settings;
        public ConfirmDialog confirm;
        /// <summary>Textseite fuer Neuigkeiten bzw. Mitwirkende</summary>
        public GameObject textPanel;
        public Text textTitle;
        public Text textBody;
        public Button textBackButton;

        /// <summary>Neues Spiel ueber das Menue gestartet: Einfuehrung zeigen (10b liest und loescht das)</summary>
        public static bool TutorialRequested { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => TutorialRequested = false;

        InputAction _menu;

        public bool TextOpen => textPanel != null && textPanel.activeSelf;

        void Awake()
        {
            Time.timeScale = 1f;
            GameSettings.RegisterActions(actions);
            if (actions != null)
            {
                _menu = actions.FindActionMap("Gameplay", true).FindAction("Menu", true);
                _menu.actionMap.Enable();
            }
            if (continueButton != null) continueButton.onClick.AddListener(Continue);
            if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (newsButton != null) newsButton.onClick.AddListener(() => ShowText("Neuigkeiten", "News"));
            if (creditsButton != null) creditsButton.onClick.AddListener(() => ShowText("Mitwirkende", "Credits"));
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(Application.Quit);
                quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            }
            if (textBackButton != null) textBackButton.onClick.AddListener(CloseText);
            if (textPanel != null) textPanel.SetActive(false);
        }

        void Start()
        {
            Refresh();
            GameSettings.OnChanged += Refresh;
            SelectDefault();
        }

        void OnDestroy() => GameSettings.OnChanged -= Refresh;

        void Refresh()
        {
            if (continueButton != null) continueButton.gameObject.SetActive(SaveSystem.Exists);
            if (versionText != null) versionText.text = (Demo.Active ? Loc.T(Demo.Label) + " · " : "") + GameSettings.Version + "  ·  " + Loc.T("Ein Mini-Projekt von Victory");
        }

        void SelectDefault()
        {
            if (EventSystem.current == null) return;
            var b = SaveSystem.Exists && continueButton != null ? continueButton : newGameButton;
            if (b != null) EventSystem.current.SetSelectedGameObject(b.gameObject);
        }

        void Update()
        {
            if (_menu == null || !_menu.WasPressedThisFrame()) return;
            if (confirm != null && confirm.IsOpen) confirm.Answer(false);
            else if (settings != null && settings.IsOpen) settings.HandleBack();
            else if (TextOpen) CloseText();
        }

        public void Continue()
        {
            if (!SaveSystem.Exists) return;
            TutorialRequested = false;
            SceneTravel.Go(SceneTravel.BaseScene, null);
        }

        public void NewGame()
        {
            if (SaveSystem.Exists && confirm != null)
                confirm.Show(Loc.T("Neues Spiel beginnen? Dein bisheriger Spielstand wird gelöscht."), Loc.T("Ja, neu beginnen"), Loc.T("Abbrechen"), StartNew);
            else StartNew();
        }

        void StartNew()
        {
            try { if (SaveSystem.Exists) File.Delete(SaveSystem.SavePath); }
            catch (System.Exception e) { Debug.LogWarning("Spielstand konnte nicht geloescht werden: " + e.Message); }
            WorldFlags.SetAll(null);
            TutorialRequested = true;
            SceneTravel.Go(SceneTravel.BaseScene, null);
        }

        public void OpenSettings()
        {
            if (settings == null) return;
            if (mainPanel != null) mainPanel.SetActive(false);
            settings.Open(() =>
            {
                if (mainPanel != null) mainPanel.SetActive(true);
                if (EventSystem.current != null && settingsButton != null) EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
            });
        }

        /// <summary>Neuigkeiten bzw. Mitwirkende aus Resources/Text/&lt;name&gt;_de bzw. _en</summary>
        public void ShowText(string title, string resource)
        {
            if (textPanel == null) return;
            var asset = Resources.Load<TextAsset>("Text/" + resource + "_" + GameSettings.Language) ?? Resources.Load<TextAsset>("Text/" + resource + "_de");
            if (textTitle != null) textTitle.text = Loc.T(title);
            if (textBody != null) textBody.text = asset != null ? asset.text : "";
            if (mainPanel != null) mainPanel.SetActive(false);
            textPanel.SetActive(true);
            if (EventSystem.current != null && textBackButton != null) EventSystem.current.SetSelectedGameObject(textBackButton.gameObject);
        }

        public void CloseText()
        {
            if (textPanel != null) textPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
            SelectDefault();
        }
    }
}
