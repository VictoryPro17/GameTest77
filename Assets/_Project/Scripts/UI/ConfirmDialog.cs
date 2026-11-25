using System;
using Reborn.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>Nachfrage mit Ja/Nein (Meilenstein 10a), z. B. "Neues Spiel? Dein Spielstand wird geloescht."</summary>
    public class ConfirmDialog : MonoBehaviour
    {
        public GameObject root;
        public Text messageText;
        public Button yesButton;
        public Text yesLabel;
        public Button noButton;
        public Text noLabel;

        public bool IsOpen => root != null && root.activeSelf;
        public string Message => messageText != null ? messageText.text : "";

        Action _onYes;
        Action _onNo;
        GameObject _returnFocus;

        void Awake()
        {
            if (yesButton != null) yesButton.onClick.AddListener(() => Answer(true));
            if (noButton != null) noButton.onClick.AddListener(() => Answer(false));
            if (root != null) root.SetActive(false);
        }

        /// <summary>Texte auf Deutsch uebergeben; sie werden uebersetzt</summary>
        public void Show(string message, string yes, string no, Action onYes, Action onNo = null)
        {
            _onYes = onYes;
            _onNo = onNo;
            _returnFocus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (messageText != null) messageText.text = Loc.T(message);
            if (yesLabel != null) yesLabel.text = Loc.T(yes);
            if (noLabel != null) noLabel.text = Loc.T(no);
            if (root != null) root.SetActive(true);
            if (EventSystem.current != null && noButton != null) EventSystem.current.SetSelectedGameObject(noButton.gameObject);
        }

        /// <summary>Antworten (auch fuer Tests)</summary>
        public void Answer(bool yes)
        {
            if (!IsOpen) return;
            root.SetActive(false);
            var action = yes ? _onYes : _onNo;
            _onYes = _onNo = null;
            if (EventSystem.current != null && _returnFocus != null && _returnFocus.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_returnFocus);
            action?.Invoke();
        }
    }
}
