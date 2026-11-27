using Reborn.Animation;
using Reborn.Core;
using Reborn.Loot;
using Reborn.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.World
{
    /// <summary>
    /// Einfuehrung (Meilenstein 10b): Mentor Aldric begleitet den Spieler nach "Neues Spiel" durch die Stadt.
    /// 7 Schritte: Laufen, Angreifen, Ausweichen, Zauber, Menue, Haendler, Weg zur Hoehle. Hinweiszeile oben,
    /// Zielpfeil am Spieler, Sprechblase am Mentor. Ueberspringen per Knopf. Belohnung 50 Gold + 3 kleine Heiltraenke,
    /// danach Welt-Schalter "tutorial_done" (gespeichert) - die Einfuehrung laeuft nur einmal.
    /// </summary>
    public class TutorialGuide : MonoBehaviour
    {
        public const string DoneFlag = "tutorial_done";
        public const int StepCount = 7;
        public const int RewardGold = 50;
        public const int RewardPotions = 3;

        public InputActionAsset actions;
        public ItemData rewardPotion;
        public Sprite arrowSprite;
        public SpeechBubble bubble;
        /// <summary>Namen der Ziele in der Szene (Haendler, Hoehlentuer)</summary>
        public string merchantName = "Npc_haendler";
        public string caveName = "Tuer_Hoehle";
        public float followDistance = 2.2f;
        public float followSpeed = 4.4f;
        public float caveReach = 2.6f;

        /// <summary>Aktueller Schritt (0..6), -1 = nicht aktiv. Bleibt beim Szenenwechsel erhalten.</summary>
        public static int CurrentStep { get; private set; } = -1;
        public static bool Active => CurrentStep >= 0;
        public static bool Completed => WorldFlags.Has(DoneFlag);
        public static int RewardCount { get; private set; }
        public static bool Skipped { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            CurrentStep = -1;
            RewardCount = 0;
            Skipped = false;
        }

        /// <summary>Texte der Schritte (deutsch = Schluessel fuer Loc), {0} = Taste</summary>
        static readonly string[] Hints =
        {
            "Laufe ein paar Schritte ({0}).",
            "Greife an ({0}). Hier in der Stadt nur zur Übung.",
            "Weiche aus ({0}). Beim Ausweichen bist du kurz unverwundbar.",
            "Wirke einen Zauber ({0}).",
            "Öffne das Menü ({0}) und schließe es wieder.",
            "Sprich mit dem Händler am Marktplatz ({0}).",
            "Folge dem Pfeil zur Höhle im Nordwesten."
        };

        static readonly string[] Speech =
        {
            "Willkommen in der Stadt! Ich bin Aldric. Lass uns ein paar Schritte gehen.",
            "Gut so! Jetzt zeig mir deinen Schwertschlag.",
            "Stark! Draußen musst du auch ausweichen können.",
            "Sehr gut. Magie hilft dir gegen viele Gegner.",
            "Im Menü findest du Inventar, Werte, Zauber und Einstellungen.",
            "Beim Händler kaufst und verkaufst du. Komm mit zum Marktplatz!",
            "Du bist bereit. In der Höhle warten Schleime – und der Weg in den Wald."
        };

        const string DoneSpeech = "Hier, nimm das für deine Reise. Viel Glück, Abenteurer!";
        const string IdleSpeech = "Viel Erfolg, Abenteurer!";
        public const string SkipLabel = "Einführung überspringen";

        // Spieler
        Transform _player;
        PlayerController _controller;
        Magic.SpellBook _book;
        Inventory.PlayerInventory _inventory;
        Economy.PlayerWallet _wallet;
        Save.SaveSystem _save;
        DirectionalSpriteAnimator _animator;
        Rigidbody2D _body;

        // Eingabe
        InputAction _move, _attack, _dodge, _spell, _menu, _interact;

        // Oberflaeche
        GameObject _panel;
        Text _hintText;
        Text _stepText;
        Button _skipButton;
        Transform _arrow;

        Vector3 _home;
        Vector2 _lastPos;
        float _walked;
        int _attacks;
        bool _wasDodging;
        int _castStart;
        bool _menuSeen;
        bool _started;
        float _startDelay = 0.6f;
        bool _wasNearIdle;

        public string HintText => _hintText != null && _panel != null && _panel.activeSelf ? _hintText.text : "";
        public bool PanelVisible => _panel != null && _panel.activeSelf;
        public bool ArrowVisible => _arrow != null && _arrow.gameObject.activeSelf;
        public Transform ArrowTarget { get; private set; }
        public Button SkipButton => _skipButton;

        void Awake()
        {
            _home = transform.position;
            _animator = GetComponentInChildren<DirectionalSpriteAnimator>();
            _body = GetComponent<Rigidbody2D>();
            if (bubble == null) bubble = GetComponent<SpeechBubble>();
            if (actions != null)
            {
                var map = actions.FindActionMap("Gameplay", true);
                _move = map.FindAction("Move", true);
                _attack = map.FindAction("Attack", true);
                _dodge = map.FindAction("Dodge", true);
                _spell = map.FindAction("Spell1", false);
                _menu = map.FindAction("Menu", true);
                _interact = map.FindAction("Interact", true);
                map.Enable();
            }
        }

        void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                _player = p.transform;
                _controller = p.GetComponent<PlayerController>();
                _book = p.GetComponent<Magic.SpellBook>();
                _inventory = p.GetComponent<Inventory.PlayerInventory>();
                _wallet = p.GetComponent<Economy.PlayerWallet>();
                _save = p.GetComponent<Save.SaveSystem>();
            }
            UI.GameMenu.OnOpened += HandleMenuOpened;
            GameSettings.OnChanged += RefreshTexts;
        }

        void OnDestroy()
        {
            UI.GameMenu.OnOpened -= HandleMenuOpened;
            GameSettings.OnChanged -= RefreshTexts;
            SceneRules.TrainingAllowed = false;
        }

        void HandleMenuOpened() => _menuSeen = true;

        void Update()
        {
            if (_player == null) return;
            if (!_started)
            {
                // kurz warten: der Spielstand (Welt-Schalter) wird nach Start geladen
                _startDelay -= Time.deltaTime;
                if (_startDelay > 0f) return;
                _started = true;
                if (!Completed && (Active || UI.MainMenu.TutorialRequested)) Begin(Mathf.Max(0, CurrentStep));
            }
            if (!Active)
            {
                IdleUpdate();
                return;
            }
            Follow();
            CheckStep();
            UpdateArrow();
        }

        // ------------------------------------------------------------ Ablauf

        /// <summary>Einfuehrung (ab einem Schritt) starten - auch fuer Tests</summary>
        public void Begin(int step = 0)
        {
            if (Completed) return;
            BuildUi();
            Skipped = false;
            GoToStep(Mathf.Clamp(step, 0, StepCount - 1));
        }

        void GoToStep(int step)
        {
            CurrentStep = step;
            _walked = 0f;
            _lastPos = _player != null ? (Vector2)_player.position : Vector2.zero;
            _attacks = 0;
            _wasDodging = false;
            _castStart = _book != null ? _book.CastCount : 0;
            _menuSeen = false;
            SceneRules.TrainingAllowed = step == 1 || step == 3;
            ArrowTarget = step == 5 ? Find(merchantName) : step == 6 ? Find(caveName) : null;
            Say(Speech[step]);
            RefreshTexts();
        }

        void CheckStep()
        {
            switch (CurrentStep)
            {
                case 0:
                    Vector2 pos = _player.position;
                    _walked += Vector2.Distance(pos, _lastPos);
                    _lastPos = pos;
                    if (_walked >= 3f) Next();
                    break;
                case 1:
                    if (Ready && _attack != null && _attack.WasPressedThisFrame() && ++_attacks >= 2) Next();
                    break;
                case 2:
                    bool dodging = _controller != null && _controller.IsDodging;
                    if (dodging && !_wasDodging) Next();
                    else _wasDodging = dodging;
                    break;
                case 3:
                    if (_book != null && _book.CastCount > _castStart) Next();
                    break;
                case 4:
                    if (_menuSeen && !UI.GameMenu.IsOpen) Next();
                    break;
                case 5:
                    if (UI.TradeWindow.IsOpen) Next();
                    break;
                case 6:
                    if (ArrowTarget == null || Vector2.Distance(_player.position, ArrowTarget.position) <= caveReach || SceneTravel.IsTraveling)
                        Finish(false);
                    break;
            }
        }

        bool Ready => !UI.GameMenu.IsOpen && !UI.TradeWindow.IsOpen;

        /// <summary>Naechster Schritt (auch fuer Tests)</summary>
        public void Next()
        {
            if (!Active) return;
            Sfx.Play(SfxId.Step);
            if (CurrentStep + 1 >= StepCount) Finish(false);
            else GoToStep(CurrentStep + 1);
        }

        public void Skip()
        {
            if (!Active) return;
            Skipped = true;
            Finish(true);
        }

        void Finish(bool skipped)
        {
            CurrentStep = -1;
            SceneRules.TrainingAllowed = false;
            UI.MainMenu.TutorialRequested = false;
            ArrowTarget = null;
            if (_panel != null) _panel.SetActive(false);
            if (_arrow != null) _arrow.gameObject.SetActive(false);
            if (Completed) return;
            WorldFlags.Set(DoneFlag);
            if (_wallet != null) _wallet.Add(RewardGold);
            if (_inventory != null && rewardPotion != null) _inventory.Add(rewardPotion, RewardPotions);
            RewardCount++;
            Sfx.Play(SfxId.LevelUp);
            Say(skipped ? IdleSpeech : DoneSpeech);
            if (_save != null) _save.Save();
        }

        void Say(string text)
        {
            if (bubble != null) bubble.Show(Loc.T(text));
        }

        void IdleUpdate()
        {
            // nach der Einfuehrung: zurueck zum Platz, gruesst, wenn man vorbeikommt
            MoveTowards(_home, 0.05f);
            bool near = Vector2.Distance(_player.position, transform.position) <= 1.6f;
            if (near && !_wasNearIdle) Say(IdleSpeech);
            _wasNearIdle = near;
            if (_arrow != null && _arrow.gameObject.activeSelf) _arrow.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ Begleiten

        void Follow()
        {
            float d = Vector2.Distance(transform.position, _player.position);
            if (d > 9f) transform.position = _player.position + new Vector3(-1.2f, 0.4f, 0f);   // z. B. nach Teleport
            else if (d > followDistance) MoveTowards(_player.position, followDistance * 0.8f);
            else SetAnim(Vector2.zero);
        }

        void MoveTowards(Vector3 target, float stop)
        {
            Vector2 to = target - transform.position;
            if (to.magnitude <= stop) { SetAnim(Vector2.zero); return; }
            Vector2 step = to.normalized * Mathf.Min(followSpeed * Time.deltaTime, to.magnitude - stop);
            if (_body != null && _body.bodyType != RigidbodyType2D.Static) _body.MovePosition(_body.position + step);
            else transform.position += (Vector3)step;
            SetAnim(to.normalized);
        }

        void SetAnim(Vector2 dir)
        {
            if (_animator == null) return;
            if (dir.sqrMagnitude > 0.01f)
            {
                _animator.SetFacing(dir);
                _animator.Play("walk");
            }
            else _animator.Play("idle");
        }

        // ------------------------------------------------------------ Oberflaeche

        static Transform Find(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.transform : null;
        }

        string KeyFor(int step)
        {
            switch (step)
            {
                case 0: return KeyName(_move, "WASD");
                case 1: return KeyName(_attack, "Leertaste");
                case 2: return KeyName(_dodge, "Umschalt");
                case 3: return KeyName(_spell, "1");
                case 4: return KeyName(_menu, "Esc");
                case 5: return KeyName(_interact, "E");
                default: return "";
            }
        }

        /// <summary>Tastenname der ersten Tastatur- bzw. Maus-Belegung (beruecksichtigt Neubelegungen)</summary>
        static string KeyName(InputAction action, string fallback)
        {
            if (action == null) return fallback;
            if (action.name == "Move") return "WASD";
            foreach (var b in action.bindings)
            {
                if (b.isComposite || b.isPartOfComposite) continue;
                string path = b.effectivePath;
                if (string.IsNullOrEmpty(path) || !(path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) continue;
                return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
            return fallback;
        }

        void RefreshTexts()
        {
            if (!Active || _hintText == null) return;
            _hintText.text = Loc.F(Hints[CurrentStep], KeyFor(CurrentStep));
            if (_stepText != null) _stepText.text = Loc.F("Einführung {0}/{1}", CurrentStep + 1, StepCount);
            var label = _skipButton != null ? _skipButton.GetComponentInChildren<Text>() : null;
            if (label != null) label.text = Loc.T(SkipLabel);
        }

        void UpdateArrow()
        {
            if (_arrow == null) return;
            bool show = ArrowTarget != null && Vector2.Distance(_player.position, ArrowTarget.position) > 1.4f;
            if (_arrow.gameObject.activeSelf != show) _arrow.gameObject.SetActive(show);
            if (!show) return;
            Vector2 dir = ((Vector2)(ArrowTarget.position - _player.position)).normalized;
            float bob = 1.05f + Mathf.Sin(Time.time * 5f) * 0.08f;
            _arrow.position = _player.position + new Vector3(0f, 0.45f, 0f) + (Vector3)(dir * bob);
            // Pfeil-Bild zeigt nach rechts
            _arrow.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        void BuildUi()
        {
            if (_panel != null) { _panel.SetActive(true); return; }
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("TutorialCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f) / Mathf.Max(0.5f, GameSettings.Current.uiScale);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panel = new GameObject("TutorialPanel", typeof(RectTransform));
            _panel.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)_panel.transform;
            // unten ueber der Zauberleiste, damit die Sprechblase des Mentors frei bleibt
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 175f);
            rt.sizeDelta = new Vector2(900f, 118f);
            var bg = _panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.07f, 0.1f, 0.86f);
            var outline = _panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.7f, 0.35f, 0.9f);
            outline.effectDistance = new Vector2(3f, -3f);

            _stepText = MakeText(_panel.transform, font, 22, new Color(0.95f, 0.8f, 0.45f), TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -10f), new Vector2(400f, 30f));
            _hintText = MakeText(_panel.transform, font, 30, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, -14f), new Vector2(-300f, -40f));
            var hintRt = (RectTransform)_hintText.transform;
            hintRt.pivot = new Vector2(0f, 0.5f);
            hintRt.anchoredPosition = new Vector2(20f, -14f);

            var btnGo = new GameObject("SkipButton", typeof(RectTransform));
            btnGo.transform.SetParent(_panel.transform, false);
            var brt = (RectTransform)btnGo.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(1f, 0.5f);
            brt.pivot = new Vector2(1f, 0.5f);
            brt.anchoredPosition = new Vector2(-16f, -6f);
            brt.sizeDelta = new Vector2(260f, 56f);
            var bimg = btnGo.AddComponent<Image>();
            bimg.color = new Color(0.3f, 0.24f, 0.18f, 1f);
            _skipButton = btnGo.AddComponent<Button>();
            _skipButton.targetGraphic = bimg;
            _skipButton.onClick.AddListener(Skip);
            MakeText(btnGo.transform, font, 22, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var arrowGo = new GameObject("TutorialArrow");
            arrowGo.transform.SetParent(transform.parent, false);
            var sr = arrowGo.AddComponent<SpriteRenderer>();
            sr.sprite = arrowSprite;
            sr.sortingOrder = 500;
            sr.color = new Color(1f, 0.9f, 0.4f, 0.95f);
            arrowGo.transform.localScale = Vector3.one * 0.6f;
            _arrow = arrowGo.transform;
            arrowGo.SetActive(false);
        }

        static Text MakeText(Transform parent, Font font, int size, Color color, TextAnchor anchor,
            Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 sizeDelta)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
