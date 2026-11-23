using System.IO;
using Reborn.Magic;
using Reborn.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 4: Oberflaeche im Stil "dunkles Fantasy" (9-teilige Rahmen per Code, Schriften, neues HUD).</summary>
    public static partial class RebornBuilder
    {
        const string SpritesTheme = Root + "/Sprites/Theme";
        const string FontsDir = Root + "/Fonts";
        /// <summary>Pixel pro Einheit der Rahmen: 50 = jedes Pixel erscheint doppelt so gross (Canvas-Referenz 100)</summary>
        const float ThemePpu = 50f;

        static UiTheme T;

        public enum PanelStyle { Window, Inset, Button }

        static Color32 Px(int r, int g, int b, int a = 255) => new Color32((byte)Mathf.Clamp(r, 0, 255), (byte)Mathf.Clamp(g, 0, 255), (byte)Mathf.Clamp(b, 0, 255), (byte)Mathf.Clamp(a, 0, 255));

        static int Hash(int x, int y, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
            h = (h ^ (h >> 13)) * 1274126177;
            return (int)((h ^ (h >> 16)) & 255);
        }

        /// <summary>Quadratisches 9-teiliges Sprite schreiben (Rand = border Pixel).</summary>
        static Sprite NineSlice(string name, int size, int border, System.Func<int, int, int, Color32> pixel)
        {
            var path = WritePng(SpritesTheme, name, size, (x, y) =>
            {
                int d = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                return pixel(x, y, d);
            }, ThemePpu);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Color32 Stone(int x, int y, int baseR, int baseG, int baseB, int seed)
        {
            int n = Hash(x / 2, y / 2, seed) % 9 - 4;
            return Px(baseR + n, baseG + n, baseB + n + 1);
        }

        static UiTheme CreateTheme()
        {
            Directory.CreateDirectory(SpritesTheme);
            var theme = LoadOrCreate<UiTheme>($"{SpritesTheme}/UiTheme.asset");
            theme.headingFont = AssetDatabase.LoadAssetAtPath<Font>($"{FontsDir}/Cinzel.ttf");
            theme.bodyFont = AssetDatabase.LoadAssetAtPath<Font>($"{FontsDir}/AlegreyaSans-Regular.ttf");
            theme.bodyBoldFont = AssetDatabase.LoadAssetAtPath<Font>($"{FontsDir}/AlegreyaSans-Bold.ttf");

            Color32 outline = Px(10, 8, 12);
            // Fenster: Stein innen, Bronze-Rahmen, Gold-Ecken mit Rubin
            theme.window = NineSlice("Theme_Window", 48, 14, (x, y, d) =>
            {
                bool corner = (x < 11 || x > 36) && (y < 11 || y > 36);
                if (d == 0) return outline;
                if (corner && d <= 5)
                {
                    int cx = x < 24 ? 5 : 42, cy = y < 24 ? 5 : 42;
                    int dist = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
                    if (dist <= 1) return Px(190, 40, 55);
                    if (dist == 2) return Px(120, 20, 35);
                    return y > 24 || x < 24 ? Px(236, 196, 104) : Px(196, 150, 70);
                }
                if (d <= 3) return y >= 44 - 0 || x <= 3 ? Px(184, 134, 70) : Px(150 - d * 6, 104 - d * 4, 52);
                if (d == 4) return Px(38, 28, 20);
                var s = Stone(x, y, 36, 34, 42, 3);
                if (d <= 6) return Px(s.r - 10, s.g - 10, s.b - 10);
                return s;
            });
            theme.inset = NineSlice("Theme_Inset", 32, 8, (x, y, d) =>
            {
                if (d == 0) return Px(14, 12, 16);
                if (d <= 2) return y > 16 ? Px(122, 88, 48) : Px(96, 68, 38);
                if (d == 3) return Px(26, 21, 18);
                return Stone(x, y, 27, 26, 33, 7);
            });
            theme.button = NineSlice("Theme_Button", 32, 8, (x, y, d) =>
            {
                if (d == 0) return outline;
                if (d <= 2) return y > 22 ? Px(244, 210, 128) : y < 10 ? Px(160, 118, 54) : Px(210, 168, 86);
                int grain = Hash(0, y, 11) % 18 + (Hash(x / 6, y, 12) % 7);
                int shade = (y - 3) * 2;
                return Px(86 + shade + grain / 2, 56 + shade / 2 + grain / 3, 32 + grain / 4);
            });
            theme.tab = theme.button;
            Color32[] rarity = { Px(150, 150, 150), Px(70, 130, 230), Px(170, 80, 220), Px(240, 150, 40) };
            Sprite Slot(string name, Color32? gem)
            {
                return NineSlice(name, 32, 7, (x, y, d) =>
                {
                    if (d == 0) return outline;
                    if (d <= 2)
                    {
                        if (gem.HasValue) { var g = gem.Value; return y > 16 ? Px(g.r, g.g, g.b) : Px(g.r * 3 / 4, g.g * 3 / 4, g.b * 3 / 4); }
                        return y > 16 ? Px(104, 88, 66) : Px(80, 68, 52);
                    }
                    if (d == 3) return Px(18, 15, 14);
                    if (gem.HasValue && x >= 24 && x <= 26 && y >= 24 && y <= 26)
                    {
                        var g = gem.Value;
                        return x == 24 && y == 26 ? Px(255, 255, 255) : Px(g.r, g.g, g.b);
                    }
                    var s = Stone(x, y, 22, 21, 27, 5);
                    int v = Mathf.Abs(x - 16) + Mathf.Abs(y - 16);
                    return Px(s.r - v / 4, s.g - v / 4, s.b - v / 4);
                });
            }
            theme.slotEmpty = Slot("Theme_Slot_Leer", null);
            string[] rn = { "Gewoehnlich", "Selten", "Episch", "Legendaer" };
            theme.slotRarity = new Sprite[4];
            for (int i = 0; i < 4; i++) theme.slotRarity[i] = Slot("Theme_Slot_" + rn[i], rarity[i]);
            theme.selection = NineSlice("Theme_Selection", 40, 9, (x, y, d) =>
            {
                if (d > 4) return Px(0, 0, 0, 0);
                int[] a = { 255, 235, 170, 100, 45 };
                return Px(255, 226, 130, a[d]);
            });
            theme.barFrame = NineSlice("Theme_BarFrame", 32, 8, (x, y, d) =>
            {
                if (d == 0) return outline;
                if (d <= 2) return y > 16 ? Px(196, 150, 78) : Px(140, 100, 50);
                if (d == 3) return Px(30, 22, 16);
                return Px(14, 12, 18);
            });
            theme.barFill = NineSlice("Theme_BarFill", 32, 0, (x, y, d) =>
            {
                int v = 150 + y * 3;
                if (y >= 26 && y <= 28) v = 255;
                return Px(v, v, v);
            });
            theme.portrait = NineSlice("Theme_Portrait", 48, 14, (x, y, d) =>
            {
                if (d == 0) return outline;
                if (d <= 4) return (x + y) % 7 == 0 ? Px(255, 232, 150) : y > 24 ? Px(232, 190, 98) : Px(178, 132, 60);
                if (d == 5) return Px(40, 28, 16);
                int v = Mathf.Abs(x - 24) + Mathf.Abs(y - 24);
                return Px(46 - v / 2, 42 - v / 2, 56 - v / 2);
            });
            theme.badge = NineSlice("Theme_Badge", 24, 8, (x, y, d) =>
            {
                if (d == 0) return outline;
                if (d <= 2) return y > 12 ? Px(255, 226, 140) : Px(190, 140, 60);
                return Px(62, 26, 30);
            });
            EditorUtility.SetDirty(theme);
            return theme;
        }

        // ------------------------------------------------------------ Neues HUD
        static Image ThemedBar(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color fill, out Text label, int fontSize)
        {
            var frame = Rect(name, parent, anchor, pos, size).gameObject.AddComponent<Image>();
            frame.sprite = T.barFrame;
            frame.type = Image.Type.Sliced;
            var fillImg = Rect("Fill", frame.transform, new Vector2(0f, 0.5f), new Vector2(7, 0), size - new Vector2(14, 12)).gameObject.AddComponent<Image>();
            fillImg.sprite = T.barFill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            fillImg.color = fill;
            label = Label("Text", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize, TextAnchor.MiddleCenter);
            label.font = T.bodyBoldFont != null ? T.bodyBoldFont : label.font;
            return fillImg;
        }

        static Image ThemedSlot(string name, Transform parent, Vector2 anchor, Vector2 pos, float size, string key, out Image icon, out Text count)
        {
            var frame = Rect(name, parent, anchor, pos, new Vector2(size, size)).gameObject.AddComponent<Image>();
            frame.sprite = T.slotEmpty;
            frame.type = Image.Type.Sliced;
            icon = Rect("Icon", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.7f, size * 0.7f)).gameObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var keyText = Label("Key", frame.transform, new Vector2(0, 1), new Vector2(8, -4), new Vector2(40, 28), 20, TextAnchor.UpperLeft);
            keyText.text = key;
            keyText.color = T.gold;
            count = Label("Count", frame.transform, new Vector2(1, 0), new Vector2(-8, 4), new Vector2(70, 28), 22, TextAnchor.LowerRight);
            return frame;
        }

        /// <summary>HUD im neuen Stil: Portraet mit Level, drei Leisten, Aktionsleiste unten, Ausruf, Banner, Schadenszahlen.</summary>
        static GameObject BuildThemedHud(Sprite portraitSprite, Sprite goldSprite)
        {
            var canvas = NewCanvas("HUD", 10, true);
            var hud = canvas.gameObject.AddComponent<HUD>();

            // ---- oben links: Portraet + Leisten
            var portrait = Rect("Portrait", canvas.transform, new Vector2(0, 1), new Vector2(24, -24), new Vector2(132, 132)).gameObject.AddComponent<Image>();
            portrait.sprite = T.portrait;
            portrait.type = Image.Type.Sliced;
            var face = Rect("Face", portrait.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -6), new Vector2(150, 150)).gameObject.AddComponent<Image>();
            face.sprite = portraitSprite;
            face.preserveAspect = true;
            var mask = portrait.gameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4(10, 10, 10, 10);
            var badge = Rect("LevelBadge", canvas.transform, new Vector2(0, 1), new Vector2(52, -138), new Vector2(76, 34)).gameObject.AddComponent<Image>();
            badge.sprite = T.badge;
            badge.type = Image.Type.Sliced;
            hud.levelText = Label("LevelText", badge.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76, 34), 20, TextAnchor.MiddleCenter);
            hud.levelText.font = T.headingFont;
            hud.levelText.color = T.gold;

            hud.healthFill = ThemedBar("HealthBar", canvas.transform, new Vector2(0, 1), new Vector2(170, -28), new Vector2(400, 40), T.health, out var hpText, 22);
            hud.healthText = hpText;
            hud.manaFill = ThemedBar("ManaBar", canvas.transform, new Vector2(0, 1), new Vector2(170, -72), new Vector2(360, 32), T.mana, out var manaText, 18);
            hud.manaText = manaText;
            hud.xpFill = ThemedBar("XPBar", canvas.transform, new Vector2(0, 1), new Vector2(170, -108), new Vector2(320, 22), T.xp, out var xpText, 1);
            xpText.text = "";
            hud.pointsHint = Label("PointsHint", canvas.transform, new Vector2(0, 1), new Vector2(170, -136), new Vector2(300, 34), 24, TextAnchor.MiddleLeft);
            hud.pointsHint.font = T.headingFont;
            hud.pointsHint.color = T.gold;
            hud.pointsHint.text = "";

            // ---- Gold unter dem Portraet (Meilenstein 6)
            var goldIcon = Rect("GoldIcon", canvas.transform, new Vector2(0, 1), new Vector2(30, -182), new Vector2(40, 40)).gameObject.AddComponent<Image>();
            goldIcon.sprite = goldSprite;
            goldIcon.preserveAspect = true;
            goldIcon.raycastTarget = false;
            hud.goldText = Label("GoldText", canvas.transform, new Vector2(0, 1), new Vector2(76, -182), new Vector2(200, 40), 26, TextAnchor.MiddleLeft);
            hud.goldText.font = T.headingFont;
            hud.goldText.color = T.gold;
            hud.goldText.text = "0";

            // ---- Quest-Hinweis oben rechts (Meilenstein 7c)
            hud.questText = Label("QuestText", canvas.transform, new Vector2(1, 1), new Vector2(-30, -175), new Vector2(620, 44), 26, TextAnchor.MiddleRight);
            hud.questText.font = T.bodyBoldFont != null ? T.bodyBoldFont : hud.questText.font;
            hud.questText.color = T.gold;
            hud.questText.text = "";

            // ---- unten Mitte: Aktionsleiste (4 Zauber, Heiltrank Q, Manatrank R)
            var bar = Rect("ActionBar", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(720, 124)).gameObject.AddComponent<Image>();
            bar.sprite = T.window;
            bar.type = Image.Type.Sliced;
            string[] keys = { "1", "2", "3", "4" };
            hud.spellIcons = new Image[SpellBook.SlotCount];
            hud.spellCooldowns = new Image[SpellBook.SlotCount];
            hud.spellLevels = new Text[SpellBook.SlotCount];
            for (int i = 0; i < SpellBook.SlotCount; i++)
            {
                var frame = ThemedSlot($"SpellSlot_{i}", bar.transform, new Vector2(0.5f, 0.5f), new Vector2(-282 + i * 102, 0), 92, keys[i], out var icon, out var lvl);
                hud.spellIcons[i] = icon;
                hud.spellLevels[i] = lvl;
                var cd = Rect("Cooldown", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78, 78)).gameObject.AddComponent<Image>();
                cd.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                cd.color = new Color(0f, 0f, 0f, 0.65f);
                cd.type = Image.Type.Filled;
                cd.fillMethod = Image.FillMethod.Radial360;
                cd.fillOrigin = (int)Image.Origin360.Top;
                cd.fillClockwise = false;
                cd.fillAmount = 0f;
                cd.raycastTarget = false;
                hud.spellCooldowns[i] = cd;
            }
            ThemedSlot("PotionSlot", bar.transform, new Vector2(0.5f, 0.5f), new Vector2(176, 0), 92, "Q", out var potIcon, out var potCount);
            hud.potionIcon = potIcon; hud.potionIcon.enabled = false;
            hud.potionCountText = potCount; potCount.text = "0";
            ThemedSlot("ManaPotionSlot", bar.transform, new Vector2(0.5f, 0.5f), new Vector2(278, 0), 92, "R", out var manaIcon, out var manaCount);
            hud.manaPotionIcon = manaIcon; hud.manaPotionIcon.enabled = false;
            hud.manaPotionCountText = manaCount; manaCount.text = "0";

            // ---- Ausruf, Banner, Schadenszahlen
            var chantText = Label("ChantText", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 50), 30, TextAnchor.MiddleCenter);
            chantText.horizontalOverflow = HorizontalWrapMode.Overflow;
            chantText.font = T.bodyBoldFont;
            canvas.gameObject.AddComponent<ChantText>().text = chantText;
            var banner = canvas.gameObject.AddComponent<CastBanner>();
            banner.title = Label("BannerTitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 330), new Vector2(1600, 120), 84, TextAnchor.MiddleCenter);
            banner.title.horizontalOverflow = HorizontalWrapMode.Overflow;
            banner.title.font = T.headingFont;
            banner.subtitle = Label("BannerSubtitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 255), new Vector2(1600, 50), 30, TextAnchor.MiddleCenter);
            banner.subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            var dmgTemplate = Label("DamageNumber", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 50), 34, TextAnchor.MiddleCenter);
            dmgTemplate.font = T.bodyBoldFont;
            var numbers = canvas.gameObject.AddComponent<DamageNumbers>();
            numbers.template = dmgTemplate;
            return SavePrefab(canvas.gameObject, "HUD");
        }
    }
}
