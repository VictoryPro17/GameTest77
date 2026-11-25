using System.Collections.Generic;
using Reborn.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.UI
{
    /// <summary>
    /// Tastenbelegung (Meilenstein 10a): eine Zeile je Aktion mit Knopf fuer Tastatur und fuer Controller.
    /// Klick -> "Taste druecken..." -> neue Taste (Esc bricht ab). Doppelte Belegung gibt einen Hinweis.
    /// Die Zeilen entstehen beim Start aus rowTemplate (vom Builder angelegt, inaktiv).
    /// </summary>
    public class RebindingView : MonoBehaviour
    {
        public InputActionAsset actions;
        public RectTransform container;
        /// <summary>Vorlage: Kinder "Label" (Text), "Keyboard" und "Gamepad" (Button mit Text-Kind)</summary>
        public GameObject rowTemplate;
        public Text messageText;
        public Button resetButton;

        /// <summary>Aktion, Teil eines zusammengesetzten Eintrags (z. B. "up" bei Move) oder null, deutscher Name</summary>
        public static readonly (string action, string part, string label)[] Rows =
        {
            ("Move", "up", "Nach oben"), ("Move", "down", "Nach unten"), ("Move", "left", "Nach links"), ("Move", "right", "Nach rechts"),
            ("Attack", null, "Angriff"), ("Dodge", null, "Ausweichen"), ("Interact", null, "Sprechen / Benutzen"),
            ("Spell1", null, "Zauber 1"), ("Spell2", null, "Zauber 2"), ("Spell3", null, "Zauber 3"), ("Spell4", null, "Zauber 4"),
            ("UsePotion", null, "Heiltrank"), ("UseManaPotion", null, "Manatrank"), ("Meditate", null, "Meditieren"), ("Menu", null, "Menü"),
        };

        class Row
        {
            public InputAction action;
            public string part;
            public string label;
            public Text labelText;
            public Text keyboardText;
            public Text gamepadText;
            public int keyboardIndex = -1;
            public int gamepadIndex = -1;
        }

        readonly List<Row> _rows = new List<Row>();
        InputActionRebindingExtensions.RebindingOperation _operation;

        public bool IsRebinding => _operation != null;
        public string Message => messageText != null ? messageText.text : "";

        void Awake()
        {
            GameSettings.RegisterActions(actions);
            if (rowTemplate != null) rowTemplate.SetActive(false);
            if (resetButton != null) resetButton.onClick.AddListener(ResetAll);
            Build();
        }

        void OnEnable()
        {
            GameSettings.OnChanged += RefreshAll;
            RefreshAll();
        }

        void OnDisable()
        {
            GameSettings.OnChanged -= RefreshAll;
            Cancel();
        }

        void Build()
        {
            if (actions == null || rowTemplate == null || container == null) return;
            var map = actions.FindActionMap("Gameplay", true);
            foreach (var (actionName, part, label) in Rows)
            {
                var action = map.FindAction(actionName);
                if (action == null) continue;
                var row = new Row { action = action, part = part, label = label };
                row.keyboardIndex = FindBinding(action, part, "<Keyboard>");
                row.gamepadIndex = FindBinding(action, part, "<Gamepad>");
                var go = Instantiate(rowTemplate, container);
                go.name = "Row_" + actionName + (part != null ? "_" + part : "");
                go.SetActive(true);
                row.labelText = go.transform.Find("Label")?.GetComponent<Text>();
                var kb = go.transform.Find("Keyboard")?.GetComponent<Button>();
                var gp = go.transform.Find("Gamepad")?.GetComponent<Button>();
                row.keyboardText = kb != null ? kb.GetComponentInChildren<Text>() : null;
                row.gamepadText = gp != null ? gp.GetComponentInChildren<Text>() : null;
                if (kb != null) kb.onClick.AddListener(() => StartRebind(row, true));
                if (gp != null) gp.onClick.AddListener(() => StartRebind(row, false));
                _rows.Add(row);
            }
        }

        /// <summary>Index der ersten Belegung des Geraets (bei Move: Teil "up" usw. des ersten passenden Verbunds)</summary>
        static int FindBinding(InputAction action, string part, string device)
        {
            var b = action.bindings;
            for (int i = 0; i < b.Count; i++)
            {
                if (b[i].isComposite) continue;
                if (part != null && (!b[i].isPartOfComposite || b[i].name != part)) continue;
                if (part == null && b[i].isPartOfComposite) continue;
                if (b[i].path.StartsWith(device)) return i;
            }
            return -1;
        }

        static string Display(InputAction action, int index)
        {
            if (index < 0) return "-";
            string s = InputControlPath.ToHumanReadableString(action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            return string.IsNullOrEmpty(s) ? "-" : s;
        }

        void RefreshAll()
        {
            foreach (var r in _rows)
            {
                if (r.labelText != null) r.labelText.text = Loc.T(r.label);
                if (r.keyboardText != null) r.keyboardText.text = Display(r.action, r.keyboardIndex);
                if (r.gamepadText != null) r.gamepadText.text = Display(r.action, r.gamepadIndex);
            }
        }

        void StartRebind(Row row, bool keyboard)
        {
            int index = keyboard ? row.keyboardIndex : row.gamepadIndex;
            if (index < 0 || _operation != null) return;
            var label = keyboard ? row.keyboardText : row.gamepadText;
            if (label != null) label.text = Loc.T("Taste drücken …");
            SetMessage(Loc.T("Neue Taste drücken (Esc bricht ab)"));
            bool wasEnabled = row.action.enabled;
            row.action.Disable();
            _operation = row.action.PerformInteractiveRebinding(index)
                .WithControlsHavingToMatchPath(keyboard ? "<Keyboard>" : "<Gamepad>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op => Finish(row, index, wasEnabled, true))
                .OnCancel(op => Finish(row, index, wasEnabled, false))
                .Start();
        }

        void Finish(Row row, int index, bool wasEnabled, bool done)
        {
            _operation?.Dispose();
            _operation = null;
            if (wasEnabled) row.action.Enable();
            if (done)
            {
                GameSettings.StoreBindings();
                string dup = FindDuplicate(row.action, index);
                SetMessage(dup != null ? Loc.F("Achtung: auch belegt bei „{0}“", Loc.T(dup)) : Loc.T("Gespeichert"));
            }
            else SetMessage("");
            RefreshAll();
        }

        void Cancel()
        {
            if (_operation == null) return;
            _operation.Cancel();
            _operation?.Dispose();
            _operation = null;
        }

        /// <summary>Andere Zeile mit derselben Taste (deutscher Name), sonst null</summary>
        public string FindDuplicate(InputAction action, int index)
        {
            string path = action.bindings[index].effectivePath;
            foreach (var r in _rows)
            {
                foreach (int i in new[] { r.keyboardIndex, r.gamepadIndex })
                {
                    if (i < 0 || (r.action == action && i == index)) continue;
                    if (r.action.bindings[i].effectivePath == path) return r.label;
                }
            }
            return null;
        }

        /// <summary>Fuer Tests: Belegung direkt setzen wie nach dem Neubelegen (z. B. "Attack", null, true, "&lt;Keyboard&gt;/k")</summary>
        public string ApplyForTest(string actionName, string part, bool keyboard, string path)
        {
            foreach (var r in _rows)
            {
                if (r.action.name != actionName || r.part != part) continue;
                int index = keyboard ? r.keyboardIndex : r.gamepadIndex;
                if (index < 0) return "kein Eintrag";
                r.action.ApplyBindingOverride(index, path);
                Finish(r, index, r.action.enabled, true);
                return Display(r.action, index);
            }
            return "Zeile fehlt";
        }

        public int RowCount => _rows.Count;

        void ResetAll()
        {
            Cancel();
            GameSettings.ResetBindings();
            SetMessage(Loc.T("Standard-Belegung wiederhergestellt"));
            RefreshAll();
        }

        void SetMessage(string text)
        {
            if (messageText != null) messageText.text = text;
        }
    }
}
