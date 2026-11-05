using System.IO;
using System.Linq;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Core;
using Reborn.Enemies;
using Reborn.Loot;
using Reborn.Player;
using Reborn.Progression;
using Reborn.UI;
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
    /// <summary>Baut Meilenstein 1 automatisch: Einstellungen, Sprites, Daten, Prefabs und die Test-Szene.</summary>
    public static class RebornBuilder
    {
        const string Root = "Assets/_Project";
        const string SpritesPlayer = Root + "/Sprites/Player";
        const string SpritesSlime = Root + "/Sprites/Slime";
        const string SpritesTiles = Root + "/Sprites/Tiles";
        const string SpritesItems = Root + "/Sprites/Items";
        const string DataAnims = Root + "/Data/Animations";
        const string DataItems = Root + "/Data/Items";
        const string Prefabs = Root + "/Prefabs";
        const string ScenePath = Root + "/Scenes/Room_Test.unity";
        const string InputPath = Root + "/Input/RebornControls.inputactions";

        const int LayerPlayer = 6, LayerEnemy = 7, LayerPickup = 8, LayerWalls = 9;
        static readonly string[] Directions = { "down", "up", "left", "right" };

        [MenuItem("Reborn/Meilenstein 1 bauen")]
        public static string BuildAll()
        {
            AssetDatabase.Refresh();
            ConfigureProject();
            ConfigureSprites(SpritesPlayer, new Vector2(0.5f, 0.3f));
            ConfigureSprites(SpritesSlime, new Vector2(0.5f, 0.41f));
            var tiles = CreateTiles();
            var icons = CreateItemIcons();
            var playerAnim = CreateAnimationSet("Player_Anim", SpritesPlayer,
                ("idle", 3f, true, 2), ("walk", 8f, true, 4), ("attack", 12f, false, 3), ("dodge", 15f, false, 3));
            var slimeAnim = CreateAnimationSet("Slime_Anim", SpritesSlime,
                ("idle", 6f, true, 4), ("hurt", 8f, false, 1), ("death", 10f, false, 3));
            var (gel, potion, lootTable) = CreateItems(icons);
            var pickup = CreatePickupPrefab(icons.gel);
            var player = CreatePlayerPrefab(playerAnim);
            var slime = CreateSlimePrefab(slimeAnim, lootTable, pickup);
            var hud = CreateHudPrefab();
            var mobile = CreateMobileControlsPrefab();
            BuildScene(tiles, player, slime, hud, mobile);
            ConfigureBuild();
            AssetDatabase.SaveAssets();
            return "Meilenstein 1 gebaut: " + ScenePath;
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

        static void ConfigureSprites(string folder, Vector2 pivot)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ConfigureTexture(path, 64, pivot);
            }
        }

        static void ConfigureTexture(string path, float ppu, Vector2 pivot)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) return;
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

        static (Sprite gel, Sprite potion) CreateItemIcons()
        {
            Color32 clear = new Color32(0, 0, 0, 0);
            var gelPath = WritePng(SpritesItems, "Icon_Schleimgel", 16, (x, y) =>
            {
                float dx = x - 7.5f, dy = (y - 6f) * 1.3f;
                float d = dx * dx + dy * dy;
                if (d > 42f) return clear;
                if (d > 30f) return new Color32(25, 70, 20, 255);
                return (x == 5 && y == 8) || (x == 6 && y == 9) ? new Color32(220, 255, 210, 255) : new Color32(70, 170, 60, 255);
            }, 32);
            var potionPath = WritePng(SpritesItems, "Icon_Heiltrank", 16, (x, y) =>
            {
                if (y >= 11 && y <= 13 && x >= 6 && x <= 9) return new Color32(150, 110, 70, 255);
                float dx = x - 7.5f, dy = y - 5.5f;
                float d = dx * dx + dy * dy;
                if (d > 30f) return clear;
                if (d > 22f) return new Color32(70, 20, 30, 255);
                return (x == 5 && y == 7) ? new Color32(255, 220, 220, 255) : new Color32(210, 40, 60, 255);
            }, 32);
            return (AssetDatabase.LoadAssetAtPath<Sprite>(gelPath), AssetDatabase.LoadAssetAtPath<Sprite>(potionPath));
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

        static (ItemData gel, ItemData potion, LootTable table) CreateItems((Sprite gel, Sprite potion) icons)
        {
            var gel = LoadOrCreate<ItemData>($"{DataItems}/Item_Schleimgel.asset");
            gel.itemName = "Schleimgel";
            gel.description = "Klebriges Material eines Waldschleims. Wird zum Herstellen gebraucht.";
            gel.icon = icons.gel;
            gel.rarity = ItemRarity.Gewoehnlich;
            gel.maxStack = 99;
            EditorUtility.SetDirty(gel);

            var potion = LoadOrCreate<ItemData>($"{DataItems}/Item_KleinerHeiltrank.asset");
            potion.itemName = "Kleiner Heiltrank";
            potion.description = "Stellt ein wenig Leben wieder her.";
            potion.icon = icons.potion;
            potion.rarity = ItemRarity.Gewoehnlich;
            potion.maxStack = 20;
            EditorUtility.SetDirty(potion);

            var table = LoadOrCreate<LootTable>($"{DataItems}/Slime_Loot.asset");
            table.entries = new[]
            {
                new LootTable.Entry { item = gel, dropChance = 1f, min = 1, max = 2 },
                new LootTable.Entry { item = potion, dropChance = 0.25f, min = 1, max = 1 },
            };
            EditorUtility.SetDirty(table);
            return (gel, potion, table);
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

        static GameObject CreatePlayerPrefab(SpriteAnimationSet anim)
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
            Visual(go.transform, anim);
            return SavePrefab(go, "Player");
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

        static Canvas NewCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            return text;
        }

        static GameObject CreateHudPrefab()
        {
            var canvas = NewCanvas("HUD", 10);
            var hud = canvas.gameObject.AddComponent<HUD>();
            hud.healthFill = Bar("HealthBar", canvas.transform, new Vector2(30, -30), new Vector2(420, 36), new Color(0.85f, 0.2f, 0.25f));
            hud.xpFill = Bar("XPBar", canvas.transform, new Vector2(30, -74), new Vector2(420, 18), new Color(0.95f, 0.8f, 0.25f));
            hud.healthText = Label("HealthText", hud.healthFill.transform.parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 36), 22, TextAnchor.MiddleCenter);
            hud.levelText = Label("LevelText", canvas.transform, new Vector2(0, 1), new Vector2(465, -30), new Vector2(140, 40), 30, TextAnchor.MiddleLeft);
            return SavePrefab(canvas.gameObject, "HUD");
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

            void Button(string name, string label, Vector2 pos, float size, Color color, string path)
            {
                var rt = Rect(name, root, new Vector2(1, 0), pos, new Vector2(size, size));
                var img = rt.gameObject.AddComponent<Image>();
                img.sprite = knob;
                img.color = color;
                var btn = rt.gameObject.AddComponent<OnScreenButton>();
                btn.controlPath = path;
                Label("Label", rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size), 28, TextAnchor.MiddleCenter).text = label;
            }
            Button("AttackButton", "Angriff", new Vector2(-110, 110), 200, new Color(0.9f, 0.3f, 0.3f, 0.6f), "<Gamepad>/buttonWest");
            Button("DodgeButton", "Ausw.", new Vector2(-330, 70), 150, new Color(0.3f, 0.6f, 0.9f, 0.6f), "<Gamepad>/buttonSouth");
            return SavePrefab(canvas.gameObject, "MobileControls");
        }

        // ------------------------------------------------------------ Szene
        static void BuildScene((Tile ground, Tile wall) tiles, GameObject player, GameObject slime, GameObject hud, GameObject mobile)
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

            PrefabUtility.InstantiatePrefab(hud);
            PrefabUtility.InstantiatePrefab(mobile);

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

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
