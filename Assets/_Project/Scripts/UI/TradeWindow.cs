using System.Collections;
using System.Collections.Generic;
using Reborn.Economy;
using Reborn.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Fenster fuer Haendler und Schmied (Meilenstein 6). In NPC-Naehe zeigt es "E: Sprechen";
    /// "Interact" oeffnet das Fenster. Offen pausiert es das Spiel und sperrt dieselben Eingaben wie das Menue.
    /// Schliessen: X-Knopf oder Menue-Taste (Tab / Esc / Start).
    /// </summary>
    public class TradeWindow : MonoBehaviour
    {
        public InputActionAsset actions;
        public GameObject root;
        public Text titleText;
        public Text goldText;
        public Text messageText;
        /// <summary>Hinweis "E: Sprechen" unten in der Mitte (auch ohne offenes Fenster)</summary>
        public Text promptText;
        public Button closeButton;
        public ShopView shopView;
        public SmithView smithView;

        public static bool IsOpen { get; private set; }
        /// <summary>Bild, in dem das Fenster zuletzt geschlossen wurde (das Menue soll dann nicht aufgehen)</summary>
        public static int ClosedFrame { get; private set; } = -1;
        /// <summary>NPC in Reichweite (null = keiner)</summary>
        public static Npc NearbyNpc { get; private set; }
        public Npc CurrentNpc { get; private set; }

        static readonly string[] BlockedActions = { "Move", "Attack", "Dodge", "UsePotion", "Spell1", "Spell2", "Spell3", "Spell4", "Meditate", "UseManaPotion" };

        InputAction _interact;
        InputAction _menu;
        readonly List<InputAction> _blocked = new List<InputAction>();
        Transform _player;
        PlayerWallet _wallet;
        float _messageTimer;

        void Awake()
        {
            var map = actions.FindActionMap("Gameplay", true);
            _interact = map.FindAction("Interact", true);
            _menu = map.FindAction("Menu", true);
            foreach (var name in BlockedActions)
            {
                var action = map.FindAction(name);
                if (action != null) _blocked.Add(action);
            }
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (root != null) root.SetActive(false);
            IsOpen = false;
            NearbyNpc = null;
        }

        void OnEnable() => _interact.actionMap.Enable();

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            _player = player.transform;
            _wallet = player.GetComponent<PlayerWallet>();
            if (_wallet != null) _wallet.OnChanged += HandleGold;
        }

        void OnDestroy()
        {
            if (_wallet != null) _wallet.OnChanged -= HandleGold;
            if (IsOpen)
            {
                IsOpen = false;
                Time.timeScale = 1f;
                foreach (var action in _blocked) action.Enable();
            }
            NearbyNpc = null;
        }

        void HandleGold(int gold)
        {
            if (goldText != null) goldText.text = "Gold: " + gold;
        }

        void Update()
        {
            if (_messageTimer > 0f)
            {
                _messageTimer -= Time.unscaledDeltaTime;
                if (_messageTimer <= 0f && messageText != null) messageText.text = "";
            }
            if (IsOpen)
            {
                if (_menu.WasPressedThisFrame()) Close();
                return;
            }
            NearbyNpc = _player != null && !GameMenu.IsOpen ? Npc.Nearest(_player.position) : null;
            if (promptText != null) promptText.text = NearbyNpc != null ? "E: Mit " + NearbyNpc.displayName + " sprechen" : "";
            if (NearbyNpc != null && _interact.WasPressedThisFrame()) Open(NearbyNpc);
        }

        public void Open(Npc npc)
        {
            if (IsOpen || npc == null || GameMenu.IsOpen) return;
            // Meilenstein 7b: Gildenmeister oeffnen kein Fenster, sie sagen einen Satz (Beitreten und Quests in 7c)
            if (npc.kind == NpcKind.Gildenmeister)
            {
                var bubble = npc.GetComponent<SpeechBubble>();
                if (bubble != null) bubble.Show(npc.greeting);
                return;
            }
            IsOpen = true;
            CurrentNpc = npc;
            if (root != null) root.SetActive(true);
            if (promptText != null) promptText.text = "";
            Time.timeScale = 0f;
            foreach (var action in _blocked) action.Disable();
            if (titleText != null) titleText.text = npc.displayName;
            HandleGold(_wallet != null ? _wallet.Gold : 0);
            if (messageText != null) messageText.text = "";
            bool shop = npc.kind == NpcKind.Haendler;
            if (shopView != null) shopView.gameObject.SetActive(shop);
            if (smithView != null) smithView.gameObject.SetActive(!shop);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (shop && shopView != null) shopView.Open(this, npc.shop);
            if (!shop && smithView != null) smithView.Open(this);
            // Erst im naechsten Bild etwas auswaehlen: die Taste, die das Fenster geoeffnet hat (Enter),
            // soll nicht gleich "Kaufen" ausloesen
            StartCoroutine(SelectNextFrame());
        }

        IEnumerator SelectNextFrame()
        {
            yield return null;
            if (!IsOpen) yield break;
            if (shopView != null && shopView.gameObject.activeSelf) shopView.SelectFirst();
            if (smithView != null && smithView.gameObject.activeSelf) smithView.SelectFirst();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            ClosedFrame = Time.frameCount;
            CurrentNpc = null;
            if (root != null) root.SetActive(false);
            Time.timeScale = 1f;
            foreach (var action in _blocked) action.Enable();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            SaveNow();
        }

        public void ShowMessage(string text)
        {
            if (messageText == null) return;
            messageText.text = text;
            _messageTimer = 2.5f;
        }

        /// <summary>Spielstand sofort speichern (nach Kauf, Verkauf, Verbessern, Schliessen)</summary>
        public void SaveNow()
        {
            var save = _player != null ? _player.GetComponent<Save.SaveSystem>() : null;
            if (save != null) save.Save();
        }
    }
}
