using System.IO;
using Reborn.Core;
using Reborn.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 10a: Hauptmenue-Szene, Einstellungen (mit Tastenbelegung) und Ja/Nein-Nachfrage.</summary>
    public static partial class RebornBuilder
    {
        const string MainMenuScenePath = Root + "/Scenes/MainMenu.unity";
        const string SpritesMenu = Root + "/Sprites/Menu";

        static readonly Color MenuDim = new Color(0f, 0f, 0f, 0.7f);
        static readonly Color MenuTabColor = new Color(0.25f, 0.23f, 0.3f, 1f);
        static readonly Color MenuRedColor = new Color(0.55f, 0.2f, 0.22f, 1f);

        /// <summary>Feste Beschriftung: macht aus dem Text eine uebersetzbare Beschriftung (nur fuer Texte, die kein Skript schreibt).</summary>
        static Text Localize(Text t)
        {
            if (t != null && t.GetComponent<LocalizedText>() == null) t.gameObject.AddComponent<LocalizedText>();
            return t;
        }

        /// <summary>Ueberschrift im Heading-Font in Gold</summary>
        static Text MenuHeading(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, string text, bool localize = true)
        {
            var t = MenuText(name, parent, pos, size, fontSize, align, text, localize);
            if (T != null && T.headingFont != null) t.font = T.headingFont;
            if (T != null) t.color = T.gold;
            return t;
        }

        /// <summary>Vollbild-Ebene mit abgedunkeltem Hintergrund (faengt Klicks ab)</summary>
        static RectTransform DimOverlay(string name, Transform parent, float alpha = 0.7f)
        {
            var rt = Stretch(name, parent);
            rt.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, alpha);
            return rt;
        }

        // ------------------------------------------------------------ Einstellungen
        /// <summary>Einstellungs-Fenster (Optionen links, Tastenbelegung rechts). Der Container bleibt aktiv, die Ebene (root) ist aus.</summary>
        static SettingsView BuildSettingsPanel(Transform canvas, InputActionAsset actions)
        {
            var holder = Stretch("SettingsView", canvas);
            var view = holder.gameObject.AddComponent<SettingsView>();
            var root = DimOverlay("Overlay", holder);
            view.root = root.gameObject;
            var panel = Panel("Panel", root, Vector2.zero, new Vector2(1500, 900), new Color(0.11f, 0.1f, 0.14f, 0.97f), PanelStyle.Window).transform;
            MenuHeading("Title", panel, new Vector2(0, 392), new Vector2(700, 64), 44, TextAnchor.MiddleCenter, "Einstellungen");

            // ---- links: 9 Optionen (Beschriftung + Wert-Knopf)
            Button Option(string label, int index, out Text value)
            {
                float y = 300 - index * 70;
                MenuText("Label_" + index, panel, new Vector2(-565, y), new Vector2(270, 58), 28, TextAnchor.MiddleLeft, label, true);
                var b = MenuButton("Option_" + index, panel, new Vector2(-270, y), new Vector2(320, 58), "", 26, MenuTabColor);
                value = b.GetComponentInChildren<Text>();
                return b;
            }
            view.languageButton = Option("Sprache", 0, out view.languageValue);
            view.masterButton = Option("Gesamtlautstärke", 1, out view.masterValue);
            view.sfxButton = Option("Effekte", 2, out view.sfxValue);
            view.fullscreenButton = Option("Vollbild", 3, out view.fullscreenValue);
            view.qualityButton = Option("Qualität", 4, out view.qualityValue);
            view.frameRateButton = Option("Bildrate", 5, out view.frameRateValue);
            view.uiScaleButton = Option("Oberflächen-Größe", 6, out view.uiScaleValue);
            view.damageNumbersButton = Option("Schadenszahlen", 7, out view.damageNumbersValue);
            view.mobileButton = Option("Handy-Knöpfe", 8, out view.mobileValue);

            // ---- rechts: Steuerung
            var controls = Rect("Controls", panel, new Vector2(0.5f, 0.5f), new Vector2(340, 20), new Vector2(780, 700));
            var rebinding = controls.gameObject.AddComponent<RebindingView>();
            rebinding.actions = actions;
            view.rebinding = rebinding;
            MenuHeading("Header", controls, new Vector2(0, 328), new Vector2(780, 50), 36, TextAnchor.MiddleCenter, "Steuerung");
            var headColor = new Color(0.85f, 0.85f, 0.9f);
            MenuText("HeadAction", controls, new Vector2(-228, 282), new Vector2(300, 36), 24, TextAnchor.MiddleLeft, "Aktion", true).color = headColor;
            MenuText("HeadKeyboard", controls, new Vector2(40, 282), new Vector2(200, 36), 24, TextAnchor.MiddleCenter, "Tastatur", true).color = headColor;
            MenuText("HeadGamepad", controls, new Vector2(260, 282), new Vector2(200, 36), 24, TextAnchor.MiddleCenter, "Controller", true).color = headColor;

            // Scrollbereich (senkrecht, mit Maske)
            var scrollBox = Panel("Scroll", controls, new Vector2(0, 25), new Vector2(780, 460), new Color(0.16f, 0.15f, 0.2f, 1f));
            var scroll = scrollBox.gameObject.AddComponent<ScrollRect>();
            var viewport = Stretch("Viewport", scrollBox.transform);
            viewport.offsetMin = new Vector2(6, 6);
            viewport.offsetMax = new Vector2(-6, -6);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);   // fuer Ziehen/Mausrad
            var content = Rect("Content", viewport, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.sizeDelta = Vector2.zero;
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.padding = new RectOffset(0, 0, 4, 4);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            rebinding.container = content;

            // Zeilen-Vorlage (inaktiv): Label, Keyboard, Gamepad
            var row = Rect("RowTemplate", content, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(768, 56));
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 56;
            var rowLabel = MenuText("Label", row, new Vector2(-228, 0), new Vector2(300, 52), 26, TextAnchor.MiddleLeft);
            rowLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            MenuButton("Keyboard", row, new Vector2(40, 0), new Vector2(200, 50), "-", 24, MenuTabColor);
            MenuButton("Gamepad", row, new Vector2(260, 0), new Vector2(200, 50), "-", 24, MenuTabColor);
            row.gameObject.SetActive(false);
            rebinding.rowTemplate = row.gameObject;

            rebinding.messageText = MenuText("Message", controls, new Vector2(0, -255), new Vector2(780, 44), 24, TextAnchor.MiddleCenter);
            rebinding.messageText.color = new Color(1f, 0.85f, 0.4f);
            rebinding.resetButton = MenuButton("Reset", controls, new Vector2(0, -312), new Vector2(380, 58), "Standard-Belegung", 26, MenuRedColor, true);

            // ---- unten: Zurueck
            view.backButton = MenuButton("Back", panel, new Vector2(0, -388), new Vector2(320, 62), "Zurück", 30, MenuTabColor, true);

            root.gameObject.SetActive(false);
            return view;
        }

        // ------------------------------------------------------------ Nachfrage
        /// <summary>Kleines Ja/Nein-Fenster ueber allem anderen. Der Container bleibt aktiv, die Ebene (root) ist aus.</summary>
        static ConfirmDialog BuildConfirmDialog(Transform canvas)
        {
            var holder = Stretch("ConfirmDialog", canvas);
            var dialog = holder.gameObject.AddComponent<ConfirmDialog>();
            var root = DimOverlay("Overlay", holder, 0.75f);
            dialog.root = root.gameObject;
            var panel = Panel("Panel", root, Vector2.zero, new Vector2(900, 420), new Color(0.11f, 0.1f, 0.14f, 0.98f), PanelStyle.Window).transform;
            dialog.messageText = MenuText("Message", panel, new Vector2(0, 70), new Vector2(780, 190), 34, TextAnchor.MiddleCenter);
            dialog.messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialog.yesButton = MenuButton("Yes", panel, new Vector2(-200, -125), new Vector2(340, 80), "Ja", 30, new Color(0.3f, 0.5f, 0.32f, 1f));
            dialog.yesLabel = dialog.yesButton.GetComponentInChildren<Text>();
            dialog.noButton = MenuButton("No", panel, new Vector2(200, -125), new Vector2(340, 80), "Nein", 30, MenuRedColor);
            dialog.noLabel = dialog.noButton.GetComponentInChildren<Text>();
            root.gameObject.SetActive(false);
            return dialog;
        }

        // ------------------------------------------------------------ Hauptmenue-Szene
        static Sprite LoadMenuSprite(string file)
        {
            var path = $"{SpritesMenu}/{file}";
            if (!File.Exists(path)) return null;
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path, 100f, new Vector2(0.5f, 0.5f));
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void BuildMainMenuScene(InputActionAsset actions)
        {
            // Wie bei den Spielszenen: NewScene gibt nicht benutzte Assets frei, deshalb alles danach frisch laden
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var reloaded = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (reloaded != null) actions = reloaded;
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>($"{SpritesTheme}/UiTheme.asset");
            if (theme != null) T = theme;
            var background = LoadMenuSprite("title_background.png");
            var logoFrame = LoadMenuSprite("logo_frame.png");

            // ---- Kamera (nur Hintergrundfarbe, die Oberflaeche liegt im Overlay-Canvas)
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.07f);
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<UniversalAdditionalCameraData>();

            var canvas = NewCanvas("MainMenuCanvas", 0);
            var ct = canvas.transform;

            // ---- Hintergrund
            var bg = Stretch("Background", ct).gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
            if (background != null) { bg.sprite = background; bg.preserveAspect = false; bg.color = Color.white; }
            else bg.color = new Color(0.07f, 0.08f, 0.14f);

            // ---- Logo oben Mitte
            var logoRoot = Rect("Logo", ct, new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(900, 270));
            var logoImg = logoRoot.gameObject.AddComponent<Image>();
            logoImg.raycastTarget = false;
            if (logoFrame != null) logoImg.sprite = logoFrame;
            else if (T != null) { logoImg.sprite = T.window; logoImg.type = Image.Type.Sliced; }
            else logoImg.color = new Color(0.1f, 0.1f, 0.14f, 0.85f);
            var logoText = Label("LogoText", logoRoot, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 170), 120, TextAnchor.MiddleCenter);
            logoText.text = "RE:BORN";
            logoText.raycastTarget = false;
            logoText.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (T != null) { logoText.font = T.headingFont != null ? T.headingFont : logoText.font; logoText.color = T.gold; }
            var logoOutline = logoText.GetComponent<Outline>();
            logoOutline.effectColor = new Color(0.05f, 0.03f, 0.02f, 0.95f);
            logoOutline.effectDistance = new Vector2(4, -4);
            var subtitle = Label("Subtitle", ct, new Vector2(0.5f, 1f), new Vector2(0, -325), new Vector2(900, 50), 32, TextAnchor.MiddleCenter);
            subtitle.text = "Ein Mini-Projekt von Victory";
            subtitle.raycastTarget = false;
            Localize(subtitle);

            // ---- Hauptknoepfe links
            var mainPanel = Stretch("MainPanel", ct);
            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            menu.actions = actions;
            menu.mainPanel = mainPanel.gameObject;
            var btnColor = new Color(0.3f, 0.38f, 0.5f, 1f);
            Button MainButton(string name, int index, string label, Color color)
            {
                var b = MenuButton(name, mainPanel, new Vector2(-660, 200 - index * 104), new Vector2(440, 84), label, 40, color, true);
                var nav = b.navigation;
                nav.mode = Navigation.Mode.Vertical;
                b.navigation = nav;
                return b;
            }
            menu.continueButton = MainButton("Continue", 0, "Weiter", new Color(0.3f, 0.5f, 0.32f, 1f));
            menu.newGameButton = MainButton("NewGame", 1, "Neues Spiel", btnColor);
            menu.settingsButton = MainButton("Settings", 2, "Einstellungen", btnColor);
            menu.newsButton = MainButton("News", 3, "Neuigkeiten", btnColor);
            menu.creditsButton = MainButton("Credits", 4, "Mitwirkende", btnColor);
            menu.quitButton = MainButton("Quit", 5, "Beenden", MenuRedColor);

            // ---- Version unten rechts
            menu.versionText = Label("Version", ct, new Vector2(1f, 0f), new Vector2(-30, 20), new Vector2(800, 40), 24, TextAnchor.LowerRight);
            menu.versionText.raycastTarget = false;
            menu.versionText.text = "";

            // ---- Textseite (Neuigkeiten / Mitwirkende)
            var textRoot = DimOverlay("TextPanel", ct, 0.8f);
            menu.textPanel = textRoot.gameObject;
            var tp = Panel("Panel", textRoot, Vector2.zero, new Vector2(1300, 900), new Color(0.11f, 0.1f, 0.14f, 0.97f), PanelStyle.Window).transform;
            menu.textTitle = MenuHeading("Title", tp, new Vector2(0, 392), new Vector2(900, 64), 44, TextAnchor.MiddleCenter, "", false);
            var box = Panel("Scroll", tp, new Vector2(0, 5), new Vector2(1180, 660), new Color(0.16f, 0.15f, 0.2f, 1f));
            var scroll = box.gameObject.AddComponent<ScrollRect>();
            var vp = Stretch("Viewport", box.transform);
            vp.offsetMin = new Vector2(6, 6);
            vp.offsetMax = new Vector2(-6, -6);
            vp.gameObject.AddComponent<RectMask2D>();
            vp.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            var content = Rect("Content", vp, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.sizeDelta = Vector2.zero;
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 20, 20);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = vp;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            menu.textBody = MenuText("Body", content, Vector2.zero, new Vector2(1100, 100), 30, TextAnchor.UpperLeft);
            menu.textBody.raycastTarget = false;
            menu.textBackButton = MenuButton("Back", tp, new Vector2(0, -388), new Vector2(320, 62), "Zurück", 30, MenuTabColor, true);
            textRoot.gameObject.SetActive(false);

            // ---- Einstellungen und Nachfrage (ganz oben)
            menu.settings = BuildSettingsPanel(ct, actions);
            menu.confirm = BuildConfirmDialog(ct);

            // ---- EventSystem wie in den Spielszenen
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            module.deselectOnBackgroundClick = false;

            Directory.CreateDirectory(Root + "/Scenes");
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }
    }
}
