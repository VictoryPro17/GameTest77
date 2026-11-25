using Reborn.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Spielmenue mit den Reitern Inventar und Charakter. Pausiert das Spiel, solange es offen ist.</summary>
    public class GameMenu : MonoBehaviour
    {
        public InputActionAsset actions;
        public GameObject root;
        public GameObject inventoryPage;
        public GameObject characterPage;
        public Button inventoryTabButton;
        public Button characterTabButton;
        public Button magicTabButton;
        public GameObject magicPage;
        public MagicView magicView;
        public Button closeButton;
        public InventoryView inventoryView;
        public CharacterView characterView;
        [Header("Meilenstein 10a: Pause-Menue")]
        public Button settingsButton;
        public Button mainMenuButton;
        public SettingsView settingsView;
        public ConfirmDialog confirm;
        public Color tabActive = new Color(1f, 0.93f, 0.72f, 1f);
        public Color tabInactive = new Color(0.5f, 0.48f, 0.5f, 1f);

        public static bool IsOpen { get; private set; }
        public static event Action OnOpened;
        public static event Action OnClosed;

        public int Page { get; private set; }

        static readonly string[] BlockedActions = { "Move", "Attack", "Dodge", "UsePotion", "Spell1", "Spell2", "Spell3", "Spell4", "Meditate", "UseManaPotion" };

        InputAction _menu;
        readonly List<InputAction> _blocked = new List<InputAction>();

        void Awake()
        {
            var map = actions.FindActionMap("Gameplay", true);
            _menu = map.FindAction("Menu", true);
            foreach (var name in BlockedActions)
            {
                var action = map.FindAction(name);
                if (action != null) _blocked.Add(action);
            }
            if (inventoryTabButton != null) inventoryTabButton.onClick.AddListener(() => ShowPage(0));
            if (characterTabButton != null) characterTabButton.onClick.AddListener(() => ShowPage(1));
            if (magicTabButton != null) magicTabButton.onClick.AddListener(() => ShowPage(2));
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(AskMainMenu);
            Core.GameSettings.RegisterActions(actions);
            if (root != null) root.SetActive(false);
            IsOpen = false;
        }

        void OnEnable()
        {
            _menu.actionMap.Enable();
        }

        void OnDestroy()
        {
            if (IsOpen)
            {
                Time.timeScale = 1f;
                IsOpen = false;
                foreach (var action in _blocked) action.Enable();
            }
        }

        void Update()
        {
            // Haendler-/Schmiede-Fenster offen oder gerade mit derselben Taste geschlossen: Menue nicht oeffnen
            if (TradeWindow.IsOpen || TradeWindow.ClosedFrame == Time.frameCount) return;
            if (!_menu.WasPressedThisFrame()) return;
            // Meilenstein 10a: zuerst Nachfrage bzw. Einstellungen schliessen, dann das Menue
            if (confirm != null && confirm.IsOpen) { confirm.Answer(false); return; }
            if (settingsView != null && settingsView.IsOpen) { settingsView.HandleBack(); return; }
            Toggle();
        }

        /// <summary>Einstellungen ueber dem Menue oeffnen (Menue bleibt offen und pausiert)</summary>
        public void OpenSettings()
        {
            if (settingsView == null) return;
            if (!IsOpen) Open();
            if (root != null) root.SetActive(false);
            settingsView.Open(() =>
            {
                if (IsOpen && root != null) root.SetActive(true);
                if (EventSystem.current != null && settingsButton != null) EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
            });
        }

        public void AskMainMenu()
        {
            if (confirm != null) confirm.Show(Loc.T("Zum Hauptmenü? Dein Spiel wird vorher gespeichert."), Loc.T("Ja"), Loc.T("Nein"), GoToMainMenu);
            else GoToMainMenu();
        }

        /// <summary>Speichern (macht SceneTravel) und ins Hauptmenue</summary>
        public void GoToMainMenu()
        {
            Close();
            World.SceneTravel.Go(MainMenu.SceneName, null);
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (root != null) root.SetActive(true);
            Time.timeScale = 0f;
            foreach (var action in _blocked) action.Disable();
            ShowPage(0);
            OnOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (root != null) root.SetActive(false);
            Time.timeScale = 1f;
            foreach (var action in _blocked) action.Enable();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            OnClosed?.Invoke();
        }

        public void ShowPage(int page)
        {
            Page = Mathf.Clamp(page, 0, 2);
            if (inventoryPage != null) inventoryPage.SetActive(Page == 0);
            if (characterPage != null) characterPage.SetActive(Page == 1);
            if (magicPage != null) magicPage.SetActive(Page == 2);
            SetTabColor(inventoryTabButton, Page == 0);
            SetTabColor(characterTabButton, Page == 1);
            SetTabColor(magicTabButton, Page == 2);
            if (Page == 0 && inventoryView != null) inventoryView.SelectFirst();
            if (Page == 1 && characterView != null) characterView.SelectFirst();
            if (Page == 2 && magicTabButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(magicTabButton.gameObject);
        }

        void SetTabColor(Button button, bool active)
        {
            if (button == null) return;
            var img = button.targetGraphic as Image;
            if (img != null) img.color = active ? tabActive : tabInactive;
        }
    }
}
