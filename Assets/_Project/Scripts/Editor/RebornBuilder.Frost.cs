using System.Collections.Generic;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Enemies;
using Reborn.Loot;
using Reborn.Magic;
using Reborn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace Reborn.EditorTools
{
    /// <summary>
    /// Meilenstein 9a: Frostgipfel (Szene "Frost") mit Eiswoelfen, Frostgeistern und dem Eisdrachen; Siegel-Portal in der Hoehle.
    /// </summary>
    public static partial class RebornBuilder
    {
        const string SpritesFrost = Root + "/Sprites/Frost";
        const string SpritesIceWolf = Root + "/Sprites/IceWolf";
        const string SpritesFrostSpirit = Root + "/Sprites/FrostSpirit";
        const string SpritesDragon = Root + "/Sprites/Dragon";
        public const string SceneFrost = "Frost";

        static readonly (string, float, bool, int)[] DragonClips =
        {
            ("idle", 5f, true, 4), ("walk", 8f, true, 6), ("breath", 10f, false, 6), ("wing", 10f, false, 6),
            ("fly", 6f, true, 4), ("roar", 6f, false, 4), ("hurt", 8f, false, 2), ("death", 8f, false, 6)
        };

        /// <summary>Sprite aus dem Frost-Ordner; fehlt er, bricht der Bau mit klarer Meldung ab</summary>
        static Sprite FrostSprite(string file)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFrost}/{file}.png");
            if (s == null) throw new System.InvalidOperationException("Frost-Grafik fehlt: " + file);
            return s;
        }

        static System.Func<TileSet> FrostTiles() => () =>
        {
            Tile Load(string n)
            {
                var t = AssetDatabase.LoadAssetAtPath<Tile>($"{SpritesTiles}/Tile_{n}.asset");
                if (t == null || t.sprite == null) throw new System.InvalidOperationException("Kachel fehlt: " + n);
                return t;
            }
            return new TileSet { grass = new[] { Load("Frost_Snow_0"), Load("Frost_Snow_1") }, path = Load("Frost_Path"), wall = Load("Frost_Wall") };
        };

        static void ConfigureFrostSprites()
        {
            var middle = new Vector2(0.5f, 0.5f);
            var lineStart = new Vector2(0f, 0.5f);
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesFrost }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var file = System.IO.Path.GetFileNameWithoutExtension(path);
                if (file.StartsWith("Frost_")) ConfigureTexture(path, 64f, middle);
                else if (file == "breath_cone" || file == "breath_warn") ConfigureTexture(path, SpritePpu, lineStart);
                else if (file == "ice_warn_circle" || file == "blizzard" || file == "ice_shard" || file.StartsWith("wing_wave_"))
                    ConfigureTexture(path, SpritePpu, middle);
                else ConfigureTexture(path, SpritePpu, FootPivot);
            }
            foreach (var n in new[] { "Frost_Snow_0", "Frost_Snow_1", "Frost_Path", "Frost_Wall" })
            {
                if (AssetDatabase.LoadAssetAtPath<Texture2D>($"{SpritesFrost}/{n}.png") == null)
                    throw new System.InvalidOperationException("Frost-Kachel fehlt: " + n);
                CreateTile("Tile_" + n, $"{SpritesFrost}/{n}.png");
            }
            foreach (var folder in new[] { SpritesIceWolf, SpritesFrostSpirit, SpritesDragon })
                ConfigureSprites(folder, FootPivot, SpritePpu);
        }

        static void BuildFrost(Dictionary<string, ItemData> items, GameObject pickup, GameObject goldPickup, GameObject player, GameObject hud,
            GameObject mobile, GameObject menu, GameObject trade, GameObject portal)
        {
            ConfigureFrostSprites();

            var wolfAnim = CreateAnimationSet("IceWolf_Anim", SpritesIceWolf, CreatureClips);
            var spiritAnim = CreateAnimationSet("FrostSpirit_Anim", SpritesFrostSpirit, CreatureClips);
            var dragonAnim = CreateAnimationSet("Dragon_Anim", SpritesDragon, DragonClips);

            var wolfLoot = CreateLoot("IceWolf_Loot", items, new[] { ("wolfsfell", 0.5f, 1, 1), ("heiltrank_klein", 0.15f, 1, 1) }, 6, 10);
            var spiritLoot = CreateLoot("FrostSpirit_Loot", items, new[] { ("eiskristall", 0.35f, 1, 1), ("manatrank_klein", 0.2f, 1, 1) }, 8, 12);
            var dragonLoot = CreateLoot("Dragon_Loot", items, new[] { ("drachenschuppe", 1f, 3, 3), ("eiskristall", 1f, 2, 2) }, 600, 600);
            var dragonRepeat = CreateLoot("Dragon_Loot_Repeat", items, new[] { ("eiskristall", 1f, 1, 2), ("heiltrank", 1f, 1, 1) }, 250, 300);

            // Eissplitter
            var shardGo = new GameObject("IceShard");
            var shard = shardGo.AddComponent<IceShard>();
            shard.damage = 10;
            shard.speed = 6f;
            FxChild(shardGo.transform, "Sprite", FrostSprite("ice_shard"), 5);
            var shardPrefab = SavePrefab(shardGo, "IceShard");

            // Kreaturen
            var wolf = CreateCreaturePrefab("IceWolf", "eiswolf", wolfAnim, wolfLoot, pickup, goldPickup, 60, 3.6f, 12, 80, 6.5f, 0.26f, 0.8f);
            var spirit = CreateCreaturePrefab("FrostSpirit", "frostgeist", spiritAnim, spiritLoot, pickup, goldPickup, 70, 1.4f, 6, 90, 7f, 0.25f, 0.6f);
            var spiritRoot = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(spirit));
            var spiritAi = spiritRoot.GetComponent<SlimeAI>();
            spiritAi.projectilePrefab = shardPrefab.GetComponent<IceShard>();
            spiritAi.shootRange = 5.5f;
            spiritAi.keepDistance = 3.2f;
            spiritAi.shootCooldown = 2.2f;
            spiritAi.projectileDamage = 10;
            PrefabUtility.SaveAsPrefabAsset(spiritRoot, AssetDatabase.GetAssetPath(spirit));
            PrefabUtility.UnloadPrefabContents(spiritRoot);

            // Effekte des Drachen
            var warnGo = new GameObject("Dragon_BreathWarn");
            var warn = warnGo.AddComponent<ChargeTelegraph>();
            warn.line = FxChild(warnGo.transform, "Line", FrostSprite("breath_warn"), -4);
            warn.spriteLength = 4f;
            var warnPrefab = SavePrefab(warnGo, "Dragon_BreathWarn");

            var coneGo = new GameObject("Dragon_BreathCone");
            var cone = coneGo.AddComponent<ChargeTelegraph>();
            cone.line = FxChild(coneGo.transform, "Line", FrostSprite("breath_cone"), 6);
            cone.spriteLength = 4f;
            var conePrefab = SavePrefab(coneGo, "Dragon_BreathCone");

            var icicleGo = new GameObject("Icicle");
            var icicle = icicleGo.AddComponent<RootSpike>();
            icicle.warning = FxChild(icicleGo.transform, "Warning", FrostSprite("ice_warn_circle"), -4);
            icicle.warning.transform.localScale = Vector3.one * 0.8f;   // 2 Einheiten Durchmesser -> Radius 0.8
            icicle.warning.color = new Color(1f, 1f, 1f, 0.85f);
            icicle.spike = FxChild(icicleGo.transform, "Spike", null, 0);
            icicle.spike.spriteSortPoint = SpriteSortPoint.Pivot;
            icicle.spikeFrames = new[] { FrostSprite("icicle_0"), FrostSprite("icicle_1"), FrostSprite("icicle_2") };
            icicle.spike.sprite = icicle.spikeFrames[0];
            icicle.damage = 25;
            icicle.delay = 1f;
            icicle.radius = 0.8f;
            icicle.lifetime = 1.6f;
            var iciclePrefab = SavePrefab(icicleGo, "Icicle");

            var waveGo = new GameObject("WingWave");
            var wave = waveGo.AddComponent<ThornWave>();
            wave.frames = new[] { FrostSprite("wing_wave_0"), FrostSprite("wing_wave_1"), FrostSprite("wing_wave_2") };
            wave.ring = FxChild(waveGo.transform, "Ring", wave.frames[0], -3);
            wave.spriteRadius = 2f;
            wave.startRadius = 0.8f;
            wave.maxRadius = 3.5f;
            wave.duration = 0.6f;
            wave.thickness = 0.6f;
            wave.damage = 15;
            var wavePrefab = SavePrefab(waveGo, "WingWave");

            var blizzGo = new GameObject("Blizzard");
            var blizz = blizzGo.AddComponent<UltimateZone>();
            blizz.circle = FxChild(blizzGo.transform, "Circle", FrostSprite("blizzard"), 20);
            blizz.spriteRadius = 2f;
            blizz.radius = 12f;
            var blizzPrefab = SavePrefab(blizzGo, "Blizzard");

            // Eisdrache
            var dragonGo = new GameObject("Eisdrache") { layer = LayerEnemy };
            TopDownBody(dragonGo, 60f);
            var col = dragonGo.AddComponent<CircleCollider2D>();
            col.radius = 0.95f;   // passend zum vergroesserten Bild
            col.offset = new Vector2(0f, 0.4f);
            dragonGo.AddComponent<Health>().maxHealth = 3000;
            var ai = dragonGo.AddComponent<DragonAI>();
            ai.wakeRadius = 4.5f;
            ai.breathWarningPrefab = warnPrefab.GetComponent<ChargeTelegraph>();
            ai.breathConePrefab = conePrefab.GetComponent<ChargeTelegraph>();
            ai.iciclePrefab = iciclePrefab.GetComponent<RootSpike>();
            ai.wingWavePrefab = wavePrefab.GetComponent<ThornWave>();
            ai.blizzardPrefab = blizzPrefab.GetComponent<UltimateZone>();
            ai.spiritPrefab = spirit;
            ai.repeatLoot = dragonRepeat;
            var dropper = dragonGo.AddComponent<LootDropper>();
            dropper.table = dragonLoot;
            dropper.pickupPrefab = pickup.GetComponent<Pickup>();
            dropper.goldPickupPrefab = goldPickup.GetComponent<Pickup>();
            dropper.scatterRadius = 0.9f;
            // Bild 1,6-fach (wirkt sonst kaum groesser als der Spieler); Trefferflaeche bleibt, Atem kommt hoeher aus dem Kopf
            Visual(dragonGo.transform, dragonAnim).transform.localScale = new Vector3(1.6f, 1.6f, 1f);
            AddShadow(dragonGo.transform, 2.6f);
            ai.headOffset = new Vector2(0f, 0.8f);
            var dragon = SavePrefab(dragonGo, "Eisdrache");

            BuildFrostScene(player, wolf, spirit, dragon, hud, mobile, menu, trade, portal);
            AddFrostPortalToCave(portal, items["frostschluessel"]);
        }

        /// <summary>
        /// Frostgipfel (28 x 18 Kacheln, Weg in Kachelzeile -1): Ankunft und Rueckkehr-Portal links, Arena mit Drache und Eissaeulen rechts.
        /// </summary>
        static void BuildFrostScene(GameObject player, GameObject wolf, GameObject spirit, GameObject dragon, GameObject hud, GameObject mobile,
            GameObject menu, GameObject trade, GameObject portal)
        {
            var (scene, camGo) = NewWorldScene(null, 28, 18, -1, requireTilemapCollider: false, tileLoader: FrostTiles());
            Shadow = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFx}/FX_Shadow.png");
            new GameObject("SceneRules").AddComponent<SceneRules>().peaceful = false;
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null)
            {
                globalLight.intensity = 0.85f;
                globalLight.color = new Color(0.82f, 0.9f, 1f);
            }

            var p = (GameObject)PrefabUtility.InstantiatePrefab(player);
            p.transform.position = new Vector3(-11f, -0.6f, 0f);
            camGo.GetComponent<Core.CameraFollow>().target = p.transform;
            AddSpawn("Ankunft", new Vector2(-11f, -0.6f));

            // Rueckkehr-Portal in die Hoehle (eisig gefaerbt)
            var back = (GameObject)PrefabUtility.InstantiatePrefab(portal);
            back.name = "Portal_Hoehle";
            back.transform.position = new Vector3(-12.4f, 1.4f, 0f);
            var backPortal = back.GetComponent<Portal>();
            backPortal.targetScene = SceneCave;
            backPortal.targetSpawn = "Frost";
            PrefabUtility.RecordPrefabInstancePropertyModifications(backPortal);
            foreach (var sr in back.GetComponentsInChildren<SpriteRenderer>()) sr.color = new Color(0.75f, 0.9f, 1f);
            foreach (var l in back.GetComponentsInChildren<Light2D>()) l.color = new Color(0.5f, 0.8f, 1f);

            // Deko
            var nature = new GameObject("Nature").transform;
            GameObject Deco(string name, string file, Vector2 pos, float collider = 0f)
            {
                var go = new GameObject(name);
                go.transform.SetParent(nature, false);
                go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = FrostSprite(file);
                sr.spriteSortPoint = SpriteSortPoint.Pivot;
                if (collider > 0f)
                {
                    go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    var c = go.AddComponent<CircleCollider2D>();
                    c.radius = collider;
                    c.offset = new Vector2(0f, collider * 0.5f);
                }
                return go;
            }
            Vector2[] trees = { new(-8f, 5f), new(-2f, 6f), new(3f, 5.4f), new(-6f, -5.2f), new(-0.5f, -6f), new(3.5f, -5.6f), new(-9f, -3f), new(0f, 4f) };
            foreach (var t in trees) Deco("FrozenTree", "frozen_tree", t, 0.3f);
            foreach (var r in new[] { new Vector2(-4.5f, 2.4f), new Vector2(2.5f, -2.6f), new Vector2(-7.5f, 6.4f), new Vector2(4.5f, 3.6f) }) Deco("IceRock", "ice_rock", r, 0.3f);
            foreach (var s in new[] { new Vector2(-8f, 2.4f), new Vector2(-3f, -3f), new Vector2(1.5f, 2.2f), new Vector2(5f, -4.4f), new Vector2(-5.5f, 4.6f) }) Deco("SnowPile", "snow_pile", s);
            foreach (var c in new[] { new Vector2(-4f, -4.6f), new Vector2(5f, 5.2f), new Vector2(-9.5f, 4f), new Vector2(1f, -3.6f) })
            {
                var crystal = Deco("IceCrystal", "ice_crystal", c);
                Glow(crystal.transform, new Color(0.45f, 0.9f, 1f), 1.2f, 3f, 0.6f);
            }

            // Gegner
            var enemies = new GameObject("Enemies").transform;
            void Spawn(GameObject prefab, Vector2 pos)
            {
                var e = (GameObject)PrefabUtility.InstantiatePrefab(prefab, enemies);
                e.transform.position = pos;
            }
            foreach (var w in new[] { new Vector2(-5f, 1.2f), new Vector2(-2f, -2f), new Vector2(0.5f, 0.8f), new Vector2(3f, -0.4f) }) Spawn(wolf, w);
            foreach (var s in new[] { new Vector2(0f, 3f), new Vector2(2f, -3.5f), new Vector2(4.5f, 1.5f) }) Spawn(spirit, s);

            // Arena
            var boss = (GameObject)PrefabUtility.InstantiatePrefab(dragon);
            boss.name = "Boss_Eisdrache";
            boss.transform.position = new Vector3(10f, 0.5f, 0f);
            var pillars = new[] { new Vector2(7f, 2.8f), new Vector2(6.6f, -2.6f), new Vector2(11.8f, -3.4f) };
            for (int i = 0; i < pillars.Length; i++)
            {
                var pg = new GameObject("IcePillar_" + i);
                pg.transform.position = pillars[i];
                var psr = pg.AddComponent<SpriteRenderer>();
                psr.sprite = FrostSprite("ice_pillar");
                psr.spriteSortPoint = SpriteSortPoint.Pivot;
                pg.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                var box = pg.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.6f, 0.5f);
                box.offset = new Vector2(0f, 0.25f);
                pg.AddComponent<IcePillar>();
            }

            AddCommonUi(hud, mobile, menu, trade);
            EditorSceneManager.SaveScene(scene, InteriorScenePath(SceneFrost));
        }

        /// <summary>Siegel-Portal in der Hoehle: grau und versiegelt, bis der Frostschluessel es oeffnet; Rueckkehrpunkt "Frost" daneben.</summary>
        static void AddFrostPortalToCave(GameObject portal, ItemData key)
        {
            // Pfade vor dem Szenenwechsel merken, Referenzen danach frisch laden
            var portalPath = AssetDatabase.GetAssetPath(portal);
            var keyPath = AssetDatabase.GetAssetPath(key);
            EditorSceneManager.OpenScene(InteriorScenePath(SceneCave), OpenSceneMode.Single);
            var portalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(portalPath);
            var keyItem = AssetDatabase.LoadAssetAtPath<ItemData>(keyPath);
            if (portalPrefab == null || keyItem == null) throw new System.InvalidOperationException("Portal-Prefab oder Frostschluessel fehlt");

            // Raum-Elternobjekt wie bei den anderen Hoehlen-Portalen
            var kampf = GameObject.Find("Portal_Kampf");
            var room = kampf != null ? kampf.transform.parent : null;

            var gate = (GameObject)PrefabUtility.InstantiatePrefab(portalPrefab);
            PrefabUtility.UnpackPrefabInstance(gate, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            gate.name = "Portal_Frost";
            if (room != null) gate.transform.SetParent(room, true);
            gate.transform.position = new Vector3(-4.5f, 1.2f, 0f);

            // Portal-Komponente durch SealedPortal ersetzen (Trigger-Collider bleibt)
            var old = gate.GetComponent<Portal>();
            Object.DestroyImmediate(old);
            var sealedPortal = gate.AddComponent<SealedPortal>();
            sealedPortal.targetScene = SceneFrost;
            sealedPortal.targetSpawn = "Ankunft";
            sealedPortal.flagId = "frost_siegel";
            sealedPortal.keyItem = keyItem;
            sealedPortal.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            sealedPortal.range = 1.6f;

            // Offene Grafik: vorhandenes "Visual" mit Frostportal-Bildern, Licht darunter (aus, solange versiegelt)
            var visual = gate.transform.Find("Visual");
            var vsr = visual.GetComponent<SpriteRenderer>();
            var frames = new[] { FrostSprite("frost_portal_0"), FrostSprite("frost_portal_1"), FrostSprite("frost_portal_2"), FrostSprite("frost_portal_3") };
            vsr.sprite = frames[0];
            var loop = visual.GetComponent<SpriteLoop>();
            loop.frames = frames;
            var glow = gate.transform.Find("Glow");
            if (glow != null)
            {
                glow.SetParent(visual, false);
                glow.GetComponent<Light2D>().color = new Color(0.5f, 0.85f, 1f);
            }
            sealedPortal.openVisual = visual.gameObject;

            // Versiegelte Grafik
            var sealedGo = new GameObject("Sealed");
            sealedGo.transform.SetParent(gate.transform, false);
            var ssr = sealedGo.AddComponent<SpriteRenderer>();
            ssr.sprite = FrostSprite("sealed_portal");
            ssr.spriteSortPoint = SpriteSortPoint.Pivot;
            sealedPortal.sealedVisual = sealedGo;

            var bubble = gate.AddComponent<SpeechBubble>();
            bubble.height = 1.6f;
            sealedPortal.bubble = bubble;

            AddSpawn("Frost", new Vector2(-3.4f, 0.2f));
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
