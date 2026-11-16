using System.IO;
using System.Linq;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Core;
using System.Collections.Generic;
using Reborn.Enemies;
using Reborn.Inventory;
using Reborn.Loot;
using Reborn.Player;
using Reborn.Progression;
using Reborn.Save;
using Reborn.UI;
using Reborn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Baut das Spiel automatisch (Meilenstein 1 + 2): Einstellungen, Sprites, Daten, Prefabs und die Test-Szene.</summary>
    public static partial class RebornBuilder
    {
        const string Root = "Assets/_Project";
        const string SpritesPlayer = Root + "/Sprites/Player";
        const string SpritesSlime = Root + "/Sprites/Slime";
        const string SpritesTiles = Root + "/Sprites/Tiles";
        const string SpritesItems = Root + "/Sprites/Items";
        const string SpritesProps = Root + "/Sprites/Props";
        const string SpritesUI = Root + "/Sprites/UI";
        const string DataAnims = Root + "/Data/Animations";
        const string DataItems = Root + "/Data/Items";
        const string Prefabs = Root + "/Prefabs";
        const string ScenePath = Root + "/Scenes/Room_Test.unity";
        const string InputPath = Root + "/Input/RebornControls.inputactions";

        const int LayerPlayer = 6, LayerEnemy = 7, LayerPickup = 8, LayerWalls = 9;
        static readonly string[] Directions = { "down", "up", "left", "right" };

        [MenuItem("Reborn/Spiel neu bauen")]
        public static string BuildAll()
        {
            AssetDatabase.Refresh();
            ConfigureProject();
            ConfigureSprites(SpritesPlayer, new Vector2(0.5f, 0.3f));
            ConfigureSprites(SpritesSlime, new Vector2(0.5f, 0.41f));
            ConfigureSprites(SpritesProps, new Vector2(0.5f, 0.25f));
            var tiles = CreateTiles();
            var icons = CreateItemIcons();
            T = CreateTheme();
            var ui = CreateUiSprites();
            var playerAnim = CreateAnimationSet("Player_Anim", SpritesPlayer,
                ("idle", 3f, true, 2), ("walk", 8f, true, 4), ("attack", 12f, false, 3), ("dodge", 15f, false, 3), ("cast", 12f, false, 3));
            var magic = CreateMagic();
            var equipSets = CreateEquipmentSets();
            var slimeAnim = CreateAnimationSet("Slime_Anim", SpritesSlime,
                ("idle", 6f, true, 4), ("hurt", 8f, false, 1), ("death", 10f, false, 3));
            var (items, database, lootTable) = CreateItems(icons, equipSets);
            var pickup = CreatePickupPrefab(icons["schleimgel"]);
            var player = CreatePlayerPrefab(playerAnim, database, magic);
            var slime = CreateSlimePrefab(slimeAnim, lootTable, pickup);
            var chest = CreateChestPrefab(pickup, "Chest", "room_test_truhe_1", new[]
            {
                new TreasureChest.Content { item = items["holzschwert"], count = 1 },
                new TreasureChest.Content { item = items["lederweste"], count = 1 },
                new TreasureChest.Content { item = items["holzschild"], count = 1 },
                new TreasureChest.Content { item = items["heiltrank_klein"], count = 2 },
            });
            var chest2 = CreateChestPrefab(pickup, "Chest_2", "room_test_truhe_2", new[]
            {
                new TreasureChest.Content { item = items["eichenstab"], count = 1 },
                new TreasureChest.Content { item = items["eisenhelm"], count = 1 },
                new TreasureChest.Content { item = items["saphirring"], count = 1 },
                new TreasureChest.Content { item = items["manatrank_klein"], count = 2 },
            });
            var hud = BuildThemedHud(AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPlayer}/idle_down_0.png"));
            var mobile = CreateMobileControlsPrefab();
            var menu = CreateMenuPrefab(ui);
            BuildScene(tiles, player, slime, hud, mobile, chest, chest2, menu);
            ConfigureBuild();
            AssetDatabase.SaveAssets();
            return "Meilenstein 4 gebaut: " + ScenePath + " (" + database.items.Count + " Gegenstaende, " + magic.spells.Length + " Zauber, "
                + equipSets.Count + " Ausruestungs-Ebenen)";
        }

        // ------------------------------------------------------------ Projekt
        static void ConfigureProject()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            layers.GetArrayElementAtIndex(LayerPlayer).stringValue = "Player";
            layers.GetArrayElementAtIndex(LayerEnemy).stringValue = "Enemy";
            layers.GetArrayElementAtIndex(LayerPickup).stringValue = "Pickup";
            layers.GetArrayElementAtIndex(LayerWalls).stringValue = "Walls";
            tagManager.ApplyModifiedProperties();

            Physics2D.gravity = Vector2.zero;
            for (int i = 0; i < 32; i++) Physics2D.IgnoreLayerCollision(LayerPickup, i, i != LayerPlayer);

            // Sortierung von oben nach unten (wer weiter unten steht, wird vorne gezeichnet)
            var renderer2D = AssetDatabase.LoadAssetAtPath<Object>("Assets/Settings/Renderer2D.asset");
            if (renderer2D != null)
            {
                var so = new SerializedObject(renderer2D);
                var mode = so.FindProperty("m_TransparencySortMode");
                var axis = so.FindProperty("m_TransparencySortAxis");
                if (mode != null) mode.intValue = (int)TransparencySortMode.CustomAxis;
                if (axis != null) axis.vector3Value = new Vector3(0f, 1f, 0f);
                so.ApplyModifiedProperties();
            }
        }

        /// <summary>Figuren-Sprites: 96 x 96 bei 96 Pixel pro Einheit (gleiche Weltgroesse wie frueher 64/64).</summary>
        static void ConfigureSprites(string folder, Vector2 pivot, float ppu = 96f)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ConfigureTexture(path, ppu, pivot);
            }
        }

        static void ConfigureTexture(string path, float ppu, Vector2 pivot)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) return;
            // Schon richtig eingestellt? Dann nicht neu importieren (spart bei ~1000 Sprites viel Zeit)
            var current = new TextureImporterSettings();
            importer.ReadTextureSettings(current);
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
                && Mathf.Approximately(importer.spritePixelsPerUnit, ppu) && importer.filterMode == FilterMode.Point
                && importer.textureCompression == TextureImporterCompression.Uncompressed && !importer.mipmapEnabled
                && current.spriteAlignment == (int)SpriteAlignment.Custom && current.spritePivot == pivot
                && current.spriteMeshType == SpriteMeshType.FullRect)
                return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------ Kacheln + Icons (einfache Pixel-Grafik)
        static string WritePng(string folder, string name, int size, System.Func<int, int, Color32> pixel, float ppu)
        {
            Directory.CreateDirectory(folder);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    texture.SetPixel(x, y, pixel(x, y));
            var path = $"{folder}/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            ConfigureTexture(path, ppu, new Vector2(0.5f, 0.5f));
            return path;
        }

        static Color32 Noise(Color32 a, Color32 b, int x, int y, int seed)
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177;
            return (h & 7) == 0 ? b : a;
        }

        static (Tile ground, Tile wall) CreateTiles()
        {
            var groundPath = WritePng(SpritesTiles, "Ground_Grass", 32,
                (x, y) => Noise(new Color32(74, 122, 58, 255), new Color32(92, 145, 70, 255), x, y, 1), 32);
            var wallPath = WritePng(SpritesTiles, "Wall_Stone", 32, (x, y) =>
            {
                bool mortar = y % 16 == 0 || (x + (y / 16 % 2) * 8) % 16 == 0;
                return mortar ? new Color32(52, 48, 58, 255) : Noise(new Color32(108, 104, 116, 255), new Color32(90, 86, 98, 255), x, y, 2);
            }, 32);
            return (CreateTile("Tile_Ground", groundPath), CreateTile("Tile_Wall", wallPath));
        }

        static Tile CreateTile(string name, string spritePath)
        {
            var path = $"{SpritesTiles}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            tile.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // Pixel-Karten 16 x 16 (oberste Zeile zuerst). '.' = durchsichtig, 'k' = Umriss, andere Zeichen laut Palette.
        static readonly string[] MapSword =
        {
            "............kkk.", "...........kwwk.", "..........kwwk..", ".........kwwk...",
            "........kwwk....", ".......kwwk.....", "..kk..kwwk......", "..kgkkwwk.......",
            "...kgwwk........", "....kgk.........", "...kgkgk........", "..khk.kgk.......",
            ".khk...kk.......", "khk.............", "kk..............", "................"
        };
        static readonly string[] MapShield =
        {
            "................", "..kkkkkkkkkkkk..", "..kwwwwmmwwwwk..", "..kwwwwmmwwwwk..",
            "..kwwwwmmwwwwk..", "..kmmmmmmmmmmk..", "..kwwwwmmwwwwk..", "..kwwwwmmwwwwk..",
            "...kwwwmmwwwk...", "...kwwwmmwwwk...", "....kwwmmwwk....", ".....kwmmwk.....",
            "......kmmk......", ".......kk.......", "................", "................"
        };
        static readonly string[] MapCap =
        {
            "................", "................", "................", ".....kkkkkk.....",
            "....kllllllk....", "...kllllhlllk...", "..kllllllhlllk..", "..klllllllllllk.",
            ".kllllllllllllk.", ".kddddddddddddk.", ".kkkkkkkkkkkkkk.", "................",
            "................", "................", "................", "................"
        };
        static readonly string[] MapVest =
        {
            "................", "...kkk....kkk...", "..klllk..klllk..", ".kllllkkkkllllk.",
            ".klllllddlllllk.", ".kkllllddllllkk.", "..kllllddllllk..", "..klhllddllhlk..",
            "..kllllddllllk..", "..kllllddllllk..", "..kllllddllllk..", "..kddddddddddk..",
            "..kkkkkkkkkkkk..", "................", "................", "................"
        };
        static readonly string[] MapPants =
        {
            "................", "................", "...kkkkkkkkkk...", "...kddddddddk...",
            "...kllllllllk...", "...kllllllllk...", "...klllkklllk...", "...klllkklllk...",
            "...kllk..kllk...", "...kllk..kllk...", "...kllk..kllk...", "...kllk..kllk...",
            "...kddk..kddk...", "...kkkk..kkkk...", "................", "................"
        };
        static readonly string[] MapBoots =
        {
            "................", "................", "..kkkk...kkkk...", "..kllk...kllk...",
            "..kllk...kllk...", "..kllk...kllk...", "..khlk...khlk...", "..kllk...kllk...",
            "..kllkk..kllkk..", "..klllkk.klllkk.", "..kllllk.kllllk.", "..kddddk.kddddk.",
            "..kkkkkk.kkkkkk.", "................", "................", "................"
        };
        static readonly string[] MapRing =
        {
            "................", "................", ".......kk.......", "......kggk......",
            "......kgwk......", ".....kkkkkk.....", "....kcckkcck....", "...kck....kck...",
            "...kc......ck...", "...kc......ck...", "...kck....kck...", "....kcckkcck....",
            ".....kkkkkk.....", "................", "................", "................"
        };
        static readonly string[] MapNecklace =
        {
            "................", "..kk........kk..", "..kck......kck..", "...kc......ck...",
            "...kc......ck...", "....kc....ck....", ".....kc..ck.....", "......kcck......",
            "......kttk......", "......kttk......", "......kttk......", ".......kk.......",
            "................", "................", "................", "................"
        };
        static readonly string[] MapBigPotion =
        {
            "................", "......kkkk......", "......kppk......", ".......kk.......",
            "......kwwk......", ".....kwwwwk.....", "....krrrrrrk....", "...krrhrrrrrk...",
            "...krhrrrrrrk...", "...krrrrrrrrk...", "...krrrrrrrrk...", "...krrrrrrrrk...",
            "....krrrrrrk....", ".....kkkkkk.....", "................", "................"
        };
        static readonly string[] MapGlove =
        {
            "................", "................", "....kk.kk.kk....", "...kllkllkllk...",
            "...kllkllkllk...", "...kllllllllk.kk", "...kllllllllkllk", "...klllllllllllk",
            "...kllllllllllk.", "...klllllllllk..", "....kllllllllk..", "....kddddddddk..",
            "....kkkkkkkkkk..", "................", "................", "................"
        };

        static Color32 C(byte r, byte g, byte b) => new Color32(r, g, b, 255);
        static readonly Color32 OutlineColor = C(30, 22, 18);

        static string WritePixelMap(string folder, string name, string[] rows, Dictionary<char, Color32> palette, Color32? mono = null)
        {
            Color32 clear = new Color32(0, 0, 0, 0);
            return WritePng(folder, name, 16, (x, y) =>
            {
                string row = rows[15 - y].PadRight(16, '.');
                char c = row[x];
                if (c == '.') return clear;
                if (mono.HasValue) return mono.Value;
                if (c == 'k' || palette == null) return OutlineColor;
                return palette.TryGetValue(c, out var col) ? col : OutlineColor;
            }, 32);
        }

        /// <summary>Item-Icons aus Blender (build_equipment.py): Sprites/Items/Item_&lt;id&gt;.png, 32 px.</summary>
        static Dictionary<string, Sprite> CreateItemIcons()
        {
            var icons = new Dictionary<string, Sprite>();
            foreach (var guid in AssetDatabase.FindAssets("Item_ t:Texture2D", new[] { SpritesItems }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var file = Path.GetFileNameWithoutExtension(path);
                if (!file.StartsWith("Item_")) continue;
                // 32 px bei 64 Pixel pro Einheit = 0,5 Einheiten am Boden (wie bisher)
                ConfigureTexture(path, 64, new Vector2(0.5f, 0.5f));
                icons[file.Substring(5)] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return icons;
        }
        public class UiSprites
        {
            public Sprite emptyFrame;
            public Sprite[] rarityFrames = new Sprite[4];
            public Sprite selection;
            /// <summary>Platz-Symbole in der Reihenfolge von EquipSlots.Positions</summary>
            public Sprite[] slotIcons = new Sprite[PlayerEquipment.SlotCount];
        }

        static UiSprites CreateUiSprites()
        {
            var ui = new UiSprites();

            var mono = new Color32(255, 255, 255, 70);
            Sprite Silhouette(string name, string[] map) => AssetDatabase.LoadAssetAtPath<Sprite>(WritePixelMap(SpritesUI, name, map, null, mono));
            var helm = Silhouette("Slot_Helm", MapCap);
            var brust = Silhouette("Slot_Brust", MapVest);
            var hose = Silhouette("Slot_Hose", MapPants);
            var schuhe = Silhouette("Slot_Schuhe", MapBoots);
            var hand = Silhouette("Slot_Handschuhe", MapGlove);
            var waffe = Silhouette("Slot_Hauptwaffe", MapSword);
            var neben = Silhouette("Slot_Nebenhand", MapShield);
            var kette = Silhouette("Slot_Halskette", MapNecklace);
            var ring = Silhouette("Slot_Ring", MapRing);
            ui.slotIcons = new[] { helm, brust, hose, schuhe, hand, waffe, neben, kette, ring, ring };
            // Plaetze im Theme-Stil (Stein mit Edelstein in der Seltenheitsfarbe)
            ui.emptyFrame = T.slotEmpty;
            ui.rarityFrames = (Sprite[])T.slotRarity.Clone();
            ui.selection = T.selection;
            return ui;
        }

        // ------------------------------------------------------------ Animationen + Daten
        static SpriteAnimationSet CreateAnimationSet(string name, string folder, params (string clip, float fps, bool loop, int frames)[] clips)
        {
            var path = $"{DataAnims}/{name}.asset";
            var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<SpriteAnimationSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            set.clips = clips.Select(c =>
            {
                Sprite[] Load(string dir) => Enumerable.Range(0, c.frames)
                    .Select(i => AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{c.clip}_{dir}_{i}.png")).ToArray();
                return new SpriteAnimationSet.Clip
                {
                    name = c.clip, fps = c.fps, loop = c.loop,
                    down = Load("down"), up = Load("up"), left = Load("left"), right = Load("right")
                };
            }).ToArray();
            EditorUtility.SetDirty(set);
            return set;
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static (Dictionary<string, ItemData> items, ItemDatabase database, LootTable table) CreateItems(Dictionary<string, Sprite> icons,
            Dictionary<string, SpriteAnimationSet> equipSets)
        {
            var items = new Dictionary<string, ItemData>();
            ItemData Item(string id, string file, string name, string description, ItemRarity rarity, ItemCategory category,
                EquipSlot slot, StatBlock stats, int heal, int sell, int stack, int mana = 0)
            {
                var item = LoadOrCreate<ItemData>($"{DataItems}/{file}.asset");
                item.id = id;
                item.itemName = name;
                item.description = description;
                item.icon = icons[id];
                item.rarity = rarity;
                item.category = category;
                item.equipSlot = slot;
                item.stats = stats;
                item.healAmount = heal;
                item.manaAmount = mana;
                item.sellValue = sell;
                item.maxStack = stack;
                item.visual = equipSets.TryGetValue(id, out var set) ? set : null;
                EditorUtility.SetDirty(item);
                items[id] = item;
                return item;
            }
            const ItemRarity G = ItemRarity.Gewoehnlich, S = ItemRarity.Selten, E = ItemRarity.Episch;
            const ItemCategory Mat = ItemCategory.Material, Use = ItemCategory.Verbrauch, Gear = ItemCategory.Ausruestung;
            var none = new StatBlock();

            Item("schleimgel", "Item_Schleimgel", "Schleimgel", "Klebriges Material eines Waldschleims. Wird zum Herstellen gebraucht.",
                G, Mat, EquipSlot.None, none, 0, 2, 99);
            Item("heiltrank_klein", "Item_KleinerHeiltrank", "Kleiner Heiltrank", "Ein kleiner roter Trank aus Waldkräutern.",
                G, Use, EquipSlot.None, none, 20, 5, 10);
            Item("heiltrank", "Item_Heiltrank", "Heiltrank", "Ein kräftiger Trank aus seltenen Kräutern.",
                S, Use, EquipSlot.None, none, 40, 15, 10);
            Item("holzschwert", "Item_Holzschwert", "Holzschwert", "Ein einfaches Übungsschwert aus Eichenholz.",
                G, Gear, EquipSlot.Hauptwaffe, new StatBlock { attack = 3 }, 0, 10, 1);
            Item("eisenschwert", "Item_Eisenschwert", "Eisenschwert", "Solide geschmiedete Klinge aus Eisen.",
                S, Gear, EquipSlot.Hauptwaffe, new StatBlock { attack = 6 }, 0, 40, 1);
            Item("holzschild", "Item_Holzschild", "Holzschild", "Ein leichter Schild aus Brettern mit Eisenrand.",
                G, Gear, EquipSlot.Nebenhand, new StatBlock { defense = 2 }, 0, 10, 1);
            Item("lederkappe", "Item_Lederkappe", "Lederkappe", "Schützt den Kopf vor Ästen und Schleim.",
                G, Gear, EquipSlot.Helm, new StatBlock { defense = 1 }, 0, 6, 1);
            Item("lederweste", "Item_Lederweste", "Lederweste", "Robuste Weste aus gegerbtem Leder.",
                G, Gear, EquipSlot.Brust, new StatBlock { defense = 2, maxHealth = 5 }, 0, 12, 1);
            Item("lederhose", "Item_Lederhose", "Lederhose", "Bequem und widerstandsfähig.",
                G, Gear, EquipSlot.Hose, new StatBlock { defense = 1 }, 0, 8, 1);
            Item("lederstiefel", "Item_Lederstiefel", "Lederstiefel", "Leichte Stiefel für schnelle Schritte.",
                G, Gear, EquipSlot.Schuhe, new StatBlock { defense = 1, dexterity = 1 }, 0, 8, 1);
            Item("kupferring", "Item_Kupferring", "Kupferring", "Ein schlichter Ring. Man sagt, er bringt Glück.",
                S, Gear, EquipSlot.Ring, new StatBlock { luck = 2 }, 0, 25, 1);
            Item("wolfszahnkette", "Item_Wolfszahnkette", "Wolfszahnkette", "Kette aus den Zähnen eines Schattenwolfs.",
                E, Gear, EquipSlot.Halskette, new StatBlock { strength = 2, attack = 1 }, 0, 60, 1);

            // ---- Meilenstein 4: neue Beute
            Item("lederhandschuhe", "Item_Lederhandschuhe", "Lederhandschuhe", "Weiche Handschuhe mit festem Griff.",
                G, Gear, EquipSlot.Handschuhe, new StatBlock { defense = 1 }, 0, 6, 1);
            Item("eisenhelm", "Item_Eisenhelm", "Eisenhelm", "Ein schwerer Helm mit Nasenschutz und rotem Kamm.",
                S, Gear, EquipSlot.Helm, new StatBlock { defense = 3 }, 0, 35, 1);
            Item("eisenbrust", "Item_Eisenbrustplatte", "Eisenbrustplatte", "Geschmiedete Platte mit Schulterschutz.",
                S, Gear, EquipSlot.Brust, new StatBlock { defense = 4, maxHealth = 10 }, 0, 60, 1);
            Item("eisenhose", "Item_Eisenbeinschienen", "Eisenbeinschienen", "Beinschienen mit Kniekacheln.",
                S, Gear, EquipSlot.Hose, new StatBlock { defense = 3 }, 0, 40, 1);
            Item("eisenstiefel", "Item_Eisenstiefel", "Eisenstiefel", "Schwer, aber unverwüstlich.",
                S, Gear, EquipSlot.Schuhe, new StatBlock { defense = 2 }, 0, 30, 1);
            Item("eisenhandschuhe", "Item_Eisenhandschuhe", "Eisenhandschuhe", "Panzerhandschuhe für kräftige Schläge.",
                S, Gear, EquipSlot.Handschuhe, new StatBlock { defense = 2, strength = 1 }, 0, 30, 1);
            Item("eichenstab", "Item_Eichenstab", "Eichenstab", "Ein Zauberstab mit leuchtendem Kristall.",
                S, Gear, EquipSlot.Hauptwaffe, new StatBlock { attack = 1, intelligence = 3 }, 0, 45, 1);
            Item("manatrank_klein", "Item_KleinerManatrank", "Kleiner Manatrank", "Ein kleiner blauer Trank, der nach Minze duftet.",
                G, Use, EquipSlot.None, none, 0, 6, 10, 20);
            Item("manatrank", "Item_Manatrank", "Manatrank", "Ein kräftiger blauer Trank aus Mondblüten.",
                S, Use, EquipSlot.None, none, 0, 18, 10, 40);
            Item("saphirring", "Item_Saphirring", "Saphirring", "Ein goldener Ring mit tiefblauem Saphir.",
                E, Gear, EquipSlot.Ring, new StatBlock { intelligence = 2, luck = 1 }, 0, 80, 1);

            var database = LoadOrCreate<ItemDatabase>($"{DataItems}/ItemDatabase.asset");
            database.items = items.Values.ToList();
            EditorUtility.SetDirty(database);

            var table = LoadOrCreate<LootTable>($"{DataItems}/Slime_Loot.asset");
            table.entries = new[]
            {
                new LootTable.Entry { item = items["schleimgel"], dropChance = 1f, min = 1, max = 2 },
                new LootTable.Entry { item = items["heiltrank_klein"], dropChance = 0.25f, min = 1, max = 1 },
                new LootTable.Entry { item = items["lederkappe"], dropChance = 0.06f, min = 1, max = 1 },
                new LootTable.Entry { item = items["lederhose"], dropChance = 0.06f, min = 1, max = 1 },
                new LootTable.Entry { item = items["lederstiefel"], dropChance = 0.06f, min = 1, max = 1 },
                new LootTable.Entry { item = items["kupferring"], dropChance = 0.03f, min = 1, max = 1 },
                new LootTable.Entry { item = items["lederhandschuhe"], dropChance = 0.06f, min = 1, max = 1 },
                new LootTable.Entry { item = items["manatrank_klein"], dropChance = 0.2f, min = 1, max = 1 },
            };
            EditorUtility.SetDirty(table);
            return (items, database, table);
        }

        // ------------------------------------------------------------ Prefabs
        static GameObject SavePrefab(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Prefabs}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject Visual(Transform parent, SpriteAnimationSet set)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var anim = visual.AddComponent<DirectionalSpriteAnimator>();
            anim.animationSet = set;
            anim.startClip = "idle";
            if (set.clips.Length > 0 && set.clips[0].down.Length > 0) sr.sprite = set.clips[0].down[0];
            visual.AddComponent<DamageFlash>();
            return visual;
        }

        static Rigidbody2D TopDownBody(GameObject go, float mass)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            return rb;
        }

        static GameObject CreatePlayerPrefab(SpriteAnimationSet anim, ItemDatabase database, MagicAssets magic)
        {
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var go = new GameObject("Player") { tag = "Player", layer = LayerPlayer };
            TopDownBody(go, 1f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.offset = new Vector2(0f, 0.12f);
            var health = go.AddComponent<Health>();
            health.maxHealth = 50;
            health.hitInvulnerability = 0.6f;
            var controller = go.AddComponent<PlayerController>();
            controller.actions = input;
            var attack = go.AddComponent<PlayerAttack>();
            attack.actions = input;
            attack.enemyMask = 1 << LayerEnemy;
            attack.bodyOffset = new Vector2(0f, 0.3f);
            attack.range = 0.55f;
            attack.radius = 0.45f;
            go.AddComponent<PlayerProgression>();
            go.AddComponent<PlayerInventory>();
            go.AddComponent<PlayerEquipment>();
            go.AddComponent<PlayerStats>();
            var potions = go.AddComponent<PotionUser>();
            potions.actions = input;
            var save = go.AddComponent<SaveSystem>();
            save.database = database;
            AddMagicToPlayer(go, magic, input);
            var visual = Visual(go.transform, anim);
            // Sichtbare Ausruestung: eine Ebene pro Platz ueber dem Koerper, als Gruppe sortiert
            visual.AddComponent<UnityEngine.Rendering.SortingGroup>();
            var layers = visual.AddComponent<EquipmentVisuals>();
            layers.layers = new SpriteRenderer[EquipmentVisuals.LayerOrder.Length];
            for (int i = 0; i < layers.layers.Length; i++)
            {
                var layer = new GameObject("Layer_" + EquipmentVisuals.LayerOrder[i]);
                layer.transform.SetParent(visual.transform, false);
                var sr = layer.AddComponent<SpriteRenderer>();
                sr.sortingOrder = i + 1;
                layers.layers[i] = sr;
            }
            return SavePrefab(go, "Player");
        }

        /// <summary>Animationssets der sichtbaren Ausruestung (Sprites/Equipment/&lt;id&gt;, gleiche Clips wie der Spieler).</summary>
        static Dictionary<string, SpriteAnimationSet> CreateEquipmentSets()
        {
            var sets = new Dictionary<string, SpriteAnimationSet>();
            string equipRoot = Root + "/Sprites/Equipment";
            if (!AssetDatabase.IsValidFolder(equipRoot)) return sets;
            foreach (var folder in AssetDatabase.GetSubFolders(equipRoot))
            {
                string id = Path.GetFileName(folder);
                ConfigureSprites(folder, new Vector2(0.5f, 0.3f));
                sets[id] = CreateAnimationSet("Equip_" + id, folder,
                    ("idle", 3f, true, 2), ("walk", 8f, true, 4), ("attack", 12f, false, 3), ("dodge", 15f, false, 3), ("cast", 12f, false, 3));
            }
            return sets;
        }

        static GameObject CreateChestPrefab(GameObject pickup, string prefabName, string chestId, TreasureChest.Content[] contents)
        {
            var go = new GameObject(prefabName);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.55f, 0.28f);
            col.offset = new Vector2(0f, 0.12f);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var chest = go.AddComponent<TreasureChest>();
            chest.chestId = chestId;
            chest.closedSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesProps}/chest_closed.png");
            chest.openSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesProps}/chest_open.png");
            sr.sprite = chest.closedSprite;
            chest.pickupPrefab = pickup.GetComponent<Pickup>();
            chest.contents = contents;
            return SavePrefab(go, prefabName);
        }

        static GameObject CreateSlimePrefab(SpriteAnimationSet anim, LootTable table, GameObject pickup)
        {
            var go = new GameObject("Slime") { layer = LayerEnemy };
            TopDownBody(go, 0.6f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.22f;
            col.offset = new Vector2(0f, 0.12f);
            var health = go.AddComponent<Health>();
            health.maxHealth = 30;
            go.AddComponent<SlimeAI>();
            var dropper = go.AddComponent<LootDropper>();
            dropper.table = table;
            dropper.pickupPrefab = pickup.GetComponent<Pickup>();
            Visual(go.transform, anim);
            return SavePrefab(go, "Slime");
        }

        static GameObject CreatePickupPrefab(Sprite icon)
        {
            var go = new GameObject("Pickup") { layer = LayerPickup };
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
            go.AddComponent<Pickup>();
            var visual = new GameObject("Icon");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = icon;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            return SavePrefab(go, "Pickup");
        }

        static Canvas NewCanvas(string name, int order, bool expand = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            if (expand) scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static Image Bar(string name, Transform parent, Vector2 pos, Vector2 size, Color fill)
        {
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var bg = Rect(name, parent, new Vector2(0, 1), pos, size).gameObject.AddComponent<Image>();
            bg.sprite = uiSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.08f, 0.07f, 0.1f, 0.85f);
            var fillRt = Rect("Fill", bg.transform, new Vector2(0, 0.5f), new Vector2(4, 0), size - new Vector2(8, 8));
            var img = fillRt.gameObject.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            img.color = fill;
            return img;
        }

        static Text Label(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, TextAnchor align)
        {
            var text = Rect(name, parent, anchor, pos, size).gameObject.AddComponent<Text>();
            text.font = T != null && T.bodyFont != null ? T.bodyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = T != null ? T.text : Color.white;
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.75f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        static GameObject CreateMobileControlsPrefab()
        {
            var canvas = NewCanvas("MobileControls", 20);
            var toggle = canvas.gameObject.AddComponent<MobileControlsToggle>();
            var root = Rect("Controls", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;
            toggle.controlsRoot = root.gameObject;
            var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            var stickBg = Rect("StickBackground", root, new Vector2(0, 0), new Vector2(120, 120), new Vector2(260, 260)).gameObject.AddComponent<Image>();
            stickBg.sprite = knob;
            stickBg.color = new Color(1, 1, 1, 0.18f);
            var stick = Rect("Stick", stickBg.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));
            var stickImg = stick.gameObject.AddComponent<Image>();
            stickImg.sprite = knob;
            stickImg.color = new Color(1, 1, 1, 0.55f);
            var onScreenStick = stick.gameObject.AddComponent<OnScreenStick>();
            onScreenStick.movementRange = 90f;
            onScreenStick.controlPath = "<Gamepad>/leftStick";

            void Button(string name, string label, Vector2 pos, float size, Color color, string path, Vector2? anchor = null)
            {
                var rt = Rect(name, root, anchor ?? new Vector2(1, 0), pos, new Vector2(size, size));
                var img = rt.gameObject.AddComponent<Image>();
                img.sprite = knob;
                img.color = color;
                var btn = rt.gameObject.AddComponent<OnScreenButton>();
                btn.controlPath = path;
                Label("Label", rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size), 28, TextAnchor.MiddleCenter).text = label;
            }
            Button("AttackButton", "Angriff", new Vector2(-110, 110), 200, new Color(0.9f, 0.3f, 0.3f, 0.6f), "<Gamepad>/buttonWest");
            Button("DodgeButton", "Ausw.", new Vector2(-330, 70), 150, new Color(0.3f, 0.6f, 0.9f, 0.6f), "<Gamepad>/buttonSouth");
            Button("PotionButton", "Trank", new Vector2(-145, 340), 130, new Color(0.3f, 0.8f, 0.4f, 0.6f), "<Gamepad>/buttonNorth");
            Button("MenuButton", "Menü", new Vector2(-30, -30), 120, new Color(0.85f, 0.7f, 0.3f, 0.6f), "<Gamepad>/start", new Vector2(1, 1));
            var spellColor = new Color(0.45f, 0.5f, 1f, 0.6f);
            Button("Spell1Button", "Z1", new Vector2(-330, 240), 100, spellColor, "<Gamepad>/rightShoulder");
            Button("Spell2Button", "Z2", new Vector2(-440, 240), 100, spellColor, "<Gamepad>/rightTrigger");
            Button("Spell3Button", "Z3", new Vector2(-330, 350), 100, spellColor, "<Gamepad>/leftShoulder");
            Button("Spell4Button", "Z4", new Vector2(-440, 350), 100, spellColor, "<Gamepad>/leftTrigger");
            Button("MeditateButton", "Medit.", new Vector2(-145, 490), 100, new Color(0.3f, 0.75f, 0.9f, 0.6f), "<Gamepad>/buttonEast");
            return SavePrefab(canvas.gameObject, "MobileControls");
        }

        // ------------------------------------------------------------ Menue (Inventar + Charakter)
        /// <summary>Fenster, eingelassener Kasten oder Knopf im Theme-Stil. color faerbt nur Knoepfe (Holz wird getoent).</summary>
        static Image Panel(string name, Transform parent, Vector2 pos, Vector2 size, Color color, PanelStyle style = PanelStyle.Inset)
        {
            var img = Rect(name, parent, new Vector2(0.5f, 0.5f), pos, size).gameObject.AddComponent<Image>();
            img.type = Image.Type.Sliced;
            if (T == null)
            {
                img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                img.color = color;
                return img;
            }
            img.sprite = style == PanelStyle.Window ? T.window : style == PanelStyle.Button ? T.button : T.inset;
            img.color = style == PanelStyle.Button ? Color.Lerp(Color.white, new Color(color.r, color.g, color.b, 1f), 0.45f) : Color.white;
            return img;
        }

        static RectTransform Stretch(string name, Transform parent)
        {
            var rt = Rect(name, parent, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        static Text MenuText(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, string text = "")
        {
            var t = Label(name, parent, new Vector2(0.5f, 0.5f), pos, size, fontSize, align);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.text = text;
            return t;
        }

        static Button MenuButton(string name, Transform parent, Vector2 pos, Vector2 size, string label, int fontSize, Color color)
        {
            var img = Panel(name, parent, pos, size, color, PanelStyle.Button);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 0.85f);
            colors.selectedColor = new Color(1f, 0.92f, 0.55f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            button.colors = colors;
            var text = MenuText("Label", img.transform, Vector2.zero, size, fontSize, TextAnchor.MiddleCenter, label);
            if (T != null && T.headingFont != null) text.font = T.headingFont;
            return button;
        }

        static ItemSlotView MenuSlot(string name, Transform parent, Vector2 pos, float size, UiSprites ui, Sprite placeholder)
        {
            var frame = Rect(name, parent, new Vector2(0.5f, 0.5f), pos, new Vector2(size, size)).gameObject.AddComponent<Image>();
            frame.sprite = ui.emptyFrame;
            frame.type = Image.Type.Sliced;
            var view = frame.gameObject.AddComponent<ItemSlotView>();
            view.transition = Selectable.Transition.None;
            view.targetGraphic = frame;
            view.frame = frame;
            view.emptyFrame = ui.emptyFrame;
            view.rarityFrames = (Sprite[])ui.rarityFrames.Clone();

            if (placeholder != null)
            {
                view.placeholder = Rect("Placeholder", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.62f, size * 0.62f)).gameObject.AddComponent<Image>();
                view.placeholder.sprite = placeholder;
                view.placeholder.raycastTarget = false;
            }
            view.icon = Rect("Icon", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.72f, size * 0.72f)).gameObject.AddComponent<Image>();
            view.icon.preserveAspect = true;
            view.icon.raycastTarget = false;
            view.icon.enabled = false;
            view.countText = MenuText("Count", frame.transform, new Vector2(size * 0.1f, -size * 0.32f), new Vector2(size * 0.7f, size * 0.4f), 24, TextAnchor.LowerRight);
            view.countText.raycastTarget = false;
            view.selection = Rect("Selection", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size + 8, size + 8)).gameObject.AddComponent<Image>();
            view.selection.sprite = ui.selection;
            view.selection.raycastTarget = false;
            view.selection.enabled = false;
            return view;
        }

        static GameObject CreateMenuPrefab(UiSprites ui)
        {
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var canvas = NewCanvas("GameMenu", 30, true);
            var menu = canvas.gameObject.AddComponent<GameMenu>();
            menu.actions = input;

            var root = Stretch("Root", canvas.transform);
            var dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            menu.root = root.gameObject;

            var panel = Panel("Panel", root, Vector2.zero, new Vector2(1760, 960), new Color(0.11f, 0.1f, 0.14f, 0.97f), PanelStyle.Window).transform;
            var tabColor = new Color(0.25f, 0.23f, 0.3f, 1f);
            menu.inventoryTabButton = MenuButton("TabInventar", panel, new Vector2(-760, 425), new Vector2(200, 62), "Inventar", 30, tabColor);
            menu.characterTabButton = MenuButton("TabCharakter", panel, new Vector2(-545, 425), new Vector2(200, 62), "Charakter", 30, tabColor);
            menu.closeButton = MenuButton("Close", panel, new Vector2(830, 425), new Vector2(76, 62), "X", 34, new Color(0.55f, 0.2f, 0.22f, 1f));
            AddMagicMenu(menu, panel, tabColor);

            // ---------------- Reiter Inventar
            var invPage = Stretch("InventoryPage", panel);
            menu.inventoryPage = invPage.gameObject;
            var inv = invPage.gameObject.AddComponent<InventoryView>();
            menu.inventoryView = inv;

            MenuText("EquipTitle", invPage, new Vector2(-600, 345), new Vector2(420, 50), 30, TextAnchor.MiddleCenter, "Ausrüstung");
            var equipRoot = Rect("Equipment", invPage, new Vector2(0.5f, 0.5f), new Vector2(-600, 0), new Vector2(420, 600));
            var equipView = equipRoot.gameObject.AddComponent<EquipmentView>();
            // Positionen in der Reihenfolge Helm, Brust, Hose, Schuhe, Handschuhe, Hauptwaffe, Nebenhand, Halskette, Ring, Ring
            Vector2[] equipPos =
            {
                new(0, 250), new(0, 130), new(0, 10), new(0, -110), new(-130, 10),
                new(-130, 130), new(130, 130), new(130, 10), new(-130, -110), new(130, -110)
            };
            equipView.slots = new ItemSlotView[PlayerEquipment.SlotCount];
            for (int i = 0; i < PlayerEquipment.SlotCount; i++)
                equipView.slots[i] = MenuSlot($"Equip_{i}_{EquipSlots.Positions[i]}", equipRoot, equipPos[i], 100, ui, ui.slotIcons[i]);
            inv.equipmentView = equipView;

            string[] filterNames = { "Alle", "Ausrüstung", "Verbrauch", "Material" };
            inv.filterButtons = new Button[4];
            for (int i = 0; i < 4; i++)
                inv.filterButtons[i] = MenuButton($"Filter_{i}", invPage, new Vector2(-310 + i * 140, 345), new Vector2(130, 54), filterNames[i], 22, tabColor);
            inv.sortButton = MenuButton("Sort", invPage, new Vector2(250, 345), new Vector2(130, 54), "Sortieren", 22, new Color(0.3f, 0.38f, 0.5f, 1f));

            var gridRoot = Rect("Grid", invPage, new Vector2(0.5f, 0.5f), new Vector2(-20, 75), new Vector2(650, 430));
            inv.gridSlots = new ItemSlotView[PlayerInventory.Size];
            for (int i = 0; i < PlayerInventory.Size; i++)
            {
                int col = i % 6, row = i / 6;
                inv.gridSlots[i] = MenuSlot($"Slot_{i:00}", gridRoot, new Vector2(-275 + col * 110, 165 - row * 110), 100, ui, null);
            }
            inv.messageText = MenuText("Message", invPage, new Vector2(-20, -200), new Vector2(650, 50), 26, TextAnchor.MiddleCenter);
            inv.messageText.color = new Color(1f, 0.85f, 0.4f);
            MenuText("Hint", invPage, new Vector2(-20, -420), new Vector2(1400, 40), 22, TextAnchor.MiddleCenter,
                "<color=#BBBBBB>Enter / Süd-Taste: Anlegen · Benutzen · Ablegen      Tab / Esc / Start: Menü schließen</color>");

            var detailPanel = Panel("Detail", invPage, new Vector2(590, 0), new Vector2(500, 760), new Color(0.16f, 0.15f, 0.2f, 1f)).transform;
            var detail = detailPanel.gameObject.AddComponent<ItemDetailView>();
            detail.emptyText = MenuText("Empty", detailPanel, Vector2.zero, new Vector2(460, 80), 26, TextAnchor.MiddleCenter, "<color=#999999>Wähle einen Gegenstand</color>");
            var content = Stretch("Content", detailPanel);
            detail.content = content.gameObject;
            detail.iconImage = Rect("Icon", content, new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(110, 110)).gameObject.AddComponent<Image>();
            detail.iconImage.preserveAspect = true;
            detail.nameText = MenuText("Name", content, new Vector2(0, 215), new Vector2(460, 50), 34, TextAnchor.MiddleCenter);
            detail.categoryText = MenuText("Category", content, new Vector2(0, 172), new Vector2(460, 36), 22, TextAnchor.MiddleCenter);
            detail.categoryText.color = new Color(0.75f, 0.75f, 0.8f);
            detail.descriptionText = MenuText("Description", content, new Vector2(0, 95), new Vector2(460, 110), 23, TextAnchor.UpperCenter);
            detail.statsText = MenuText("Stats", content, new Vector2(0, -55), new Vector2(460, 190), 25, TextAnchor.UpperLeft);
            detail.sellText = MenuText("Sell", content, new Vector2(0, -185), new Vector2(460, 36), 22, TextAnchor.MiddleLeft);
            detail.sellText.color = new Color(1f, 0.85f, 0.4f);
            detail.actionButton = MenuButton("Action", content, new Vector2(0, -300), new Vector2(320, 80), "Anlegen", 30, new Color(0.3f, 0.5f, 0.32f, 1f));
            detail.actionLabel = detail.actionButton.GetComponentInChildren<Text>();
            content.gameObject.SetActive(false);
            inv.detail = detail;

            // ---------------- Reiter Charakter
            var charPage = Stretch("CharacterPage", panel);
            menu.characterPage = charPage.gameObject;
            var cv = charPage.gameObject.AddComponent<CharacterView>();
            menu.characterView = cv;
            Panel("StatsBox", charPage, new Vector2(-450, 20), new Vector2(640, 720), new Color(0.16f, 0.15f, 0.2f, 1f));
            cv.statsText = MenuText("Stats", charPage, new Vector2(-450, 20), new Vector2(560, 640), 34, TextAnchor.UpperLeft);
            cv.pointsText = MenuText("Points", charPage, new Vector2(300, 345), new Vector2(640, 50), 32, TextAnchor.MiddleLeft);
            cv.pointsText.color = new Color(1f, 0.85f, 0.3f);
            cv.attributeTexts = new Text[PlayerStats.AttributeCount];
            cv.effectTexts = new Text[PlayerStats.AttributeCount];
            cv.plusButtons = new Button[PlayerStats.AttributeCount];
            for (int i = 0; i < PlayerStats.AttributeCount; i++)
            {
                float y = 245 - i * 120;
                Panel($"AttrBox_{i}", charPage, new Vector2(300, y), new Vector2(640, 104), new Color(0.16f, 0.15f, 0.2f, 1f));
                cv.attributeTexts[i] = MenuText($"Attr_{i}", charPage, new Vector2(240, y + 18), new Vector2(480, 50), 32, TextAnchor.MiddleLeft);
                cv.effectTexts[i] = MenuText($"Effect_{i}", charPage, new Vector2(240, y - 26), new Vector2(480, 34), 22, TextAnchor.MiddleLeft);
                cv.effectTexts[i].color = new Color(0.75f, 0.75f, 0.8f);
                cv.plusButtons[i] = MenuButton($"Plus_{i}", charPage, new Vector2(560, y), new Vector2(84, 84), "+", 44, new Color(0.3f, 0.5f, 0.32f, 1f));
            }
            cv.deleteButton = MenuButton("DeleteSave", charPage, new Vector2(520, -400), new Vector2(400, 70), "Spielstand löschen", 26, new Color(0.55f, 0.2f, 0.22f, 1f));
            var confirm = Panel("Confirm", charPage, Vector2.zero, new Vector2(760, 320), new Color(0.08f, 0.07f, 0.1f, 0.98f));
            MenuText("Question", confirm.transform, new Vector2(0, 60), new Vector2(700, 120), 32, TextAnchor.MiddleCenter, "Spielstand wirklich löschen?\nDas Spiel startet danach neu.");
            cv.confirmYes = MenuButton("Yes", confirm.transform, new Vector2(-160, -90), new Vector2(240, 76), "Ja, löschen", 28, new Color(0.55f, 0.2f, 0.22f, 1f));
            cv.confirmNo = MenuButton("No", confirm.transform, new Vector2(160, -90), new Vector2(240, 76), "Nein", 28, tabColor);
            cv.confirmPanel = confirm.gameObject;
            confirm.gameObject.SetActive(false);
            charPage.gameObject.SetActive(false);

            return SavePrefab(canvas.gameObject, "GameMenu");
        }

        // ------------------------------------------------------------ Szene
        static void BuildScene((Tile ground, Tile wall) tiles, GameObject player, GameObject slime, GameObject hud, GameObject mobile,
            GameObject chest, GameObject chest2, GameObject menu)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.08f);
            camGo.transform.position = new Vector3(0, 0, -10);
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.AddComponent<CameraFollow>();

            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            var grid = new GameObject("Grid").AddComponent<Grid>();
            Tilemap NewMap(string name, int order, bool collider)
            {
                var go = new GameObject(name);
                go.transform.SetParent(grid.transform, false);
                var map = go.AddComponent<Tilemap>();
                var r = go.AddComponent<TilemapRenderer>();
                r.sortingOrder = order;
                if (collider)
                {
                    go.layer = LayerWalls;
                    var tc = go.AddComponent<TilemapCollider2D>();
                    var rb = go.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Static;
                    var composite = go.AddComponent<CompositeCollider2D>();
                    composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                    tc.compositeOperation = Collider2D.CompositeOperation.Merge;
                }
                return map;
            }
            var groundMap = NewMap("Ground", -10, false);
            var wallMap = NewMap("Walls", -5, true);
            const int w = 20, h = 12;
            for (int x = -w / 2; x < w / 2; x++)
                for (int y = -h / 2; y < h / 2; y++)
                {
                    bool border = x == -w / 2 || x == w / 2 - 1 || y == -h / 2 || y == h / 2 - 1;
                    if (border) wallMap.SetTile(new Vector3Int(x, y, 0), tiles.wall);
                    else groundMap.SetTile(new Vector3Int(x, y, 0), tiles.ground);
                }

            // Mauer-Geometrie sofort erzeugen, sonst wird die Szene mit leerem Collider gespeichert
            wallMap.CompressBounds();
            wallMap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            var wallComposite = wallMap.GetComponent<CompositeCollider2D>();
            wallComposite.GenerateGeometry();
            if (wallComposite.shapeCount == 0)
                throw new System.InvalidOperationException("Mauer-Collider ist leer. Szene wird nicht gespeichert.");
            wallMap.gameObject.AddComponent<WallColliderGuard>();

            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(player);
            playerInstance.transform.position = Vector3.zero;
            camGo.GetComponent<CameraFollow>().target = playerInstance.transform;

            Vector2[] slimePositions = { new(-6, 3), new(6, 3), new(-6, -3), new(6, -3), new(4, -4) };
            var enemies = new GameObject("Enemies").transform;
            foreach (var p in slimePositions)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(slime, enemies);
                s.transform.position = p;
            }

            var chestInstance = (GameObject)PrefabUtility.InstantiatePrefab(chest);
            chestInstance.transform.position = new Vector3(-8f, 0f, 0f);
            var chest2Instance = (GameObject)PrefabUtility.InstantiatePrefab(chest2);
            chest2Instance.transform.position = new Vector3(8f, 3f, 0f);

            PrefabUtility.InstantiatePrefab(hud);
            PrefabUtility.InstantiatePrefab(mobile);
            PrefabUtility.InstantiatePrefab(menu);

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            module.deselectOnBackgroundClick = false;

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static void ConfigureBuild()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }
    }
}
