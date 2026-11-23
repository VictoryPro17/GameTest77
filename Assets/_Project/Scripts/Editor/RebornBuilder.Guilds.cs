using System.Collections.Generic;
using Reborn.Animation;
using Reborn.Combat;
using Reborn.Enemies;
using Reborn.Guilds;
using Reborn.Loot;
using Reborn.Progression;
using Reborn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 7c: Gilden mit Quests, Schattenwolf und Riesenspinne mit Kampf-Logik, Wald hinter der Hoehle.</summary>
    public static partial class RebornBuilder
    {
        const string DataGuilds = Root + "/Data/Guilds";
        const string SpritesForest = Root + "/Sprites/Forest";
        public const string SceneForest = "Forest";

        /// <summary>Gilden-Daten fuer das Spieler-Prefab (wird in BuildAll vor CreatePlayerPrefab gesetzt)</summary>
        static GuildData[] GuildAssets = new GuildData[0];

        static QuestData Q(string title, QuestKind kind, string target, int amount, int gold, int xp, string item2 = null, int amount2 = 0) =>
            new QuestData { title = title, kind = kind, target = target, amount = amount, gold = gold, xp = xp, item2 = item2, amount2 = amount2 };

        static GuildData[] CreateGuilds(Dictionary<string, ItemData> items)
        {
            System.IO.Directory.CreateDirectory(DataGuilds);
            GuildData G(GuildId id, string name, PlayerAttribute attr, string weapon, QuestData[] quests)
            {
                var g = LoadOrCreate<GuildData>($"{DataGuilds}/Guild_{id}.asset");
                g.id = id;
                g.guildName = name;
                g.attribute = attr;
                g.weapon = items[weapon];
                g.quests = quests;
                EditorUtility.SetDirty(g);
                return g;
            }
            return new[]
            {
                G(GuildId.Krieger, "Kriegergilde", PlayerAttribute.Staerke, "gildenschwert", new[]
                {
                    Q("Schleime besiegen", QuestKind.Besiegen, "schleim", 5, 50, 60),
                    Q("Wölfe besiegen", QuestKind.Besiegen, "wolf", 3, 80, 100),
                    Q("Wolfsfelle bringen", QuestKind.Bringen, "wolfsfell", 4, 120, 150),
                }),
                G(GuildId.Magier, "Magiergilde", PlayerAttribute.Intelligenz, "gildenstab", new[]
                {
                    Q("Gegner mit Zaubern besiegen", QuestKind.MitZauberBesiegen, "*", 5, 50, 60),
                    Q("Spinnenseide bringen", QuestKind.Bringen, "spinnenseide", 3, 80, 100),
                    Q("Spinnen besiegen", QuestKind.Besiegen, "spinne", 3, 120, 150),
                }),
                G(GuildId.Assassine, "Assassinengilde", PlayerAttribute.Geschick, "schattendolche", new[]
                {
                    Q("Wölfe besiegen", QuestKind.Besiegen, "wolf", 3, 50, 60),
                    Q("Spinnen besiegen", QuestKind.Besiegen, "spinne", 3, 80, 100),
                    Q("Schleimgel und Seide bringen", QuestKind.Bringen, "schleimgel", 6, 120, 150, "spinnenseide", 2),
                }),
            };
        }

        static LootTable CreateLoot(string asset, Dictionary<string, ItemData> items, (string id, float chance, int min, int max)[] entries, int goldMin, int goldMax)
        {
            var table = LoadOrCreate<LootTable>($"{DataItems}/{asset}.asset");
            var list = new List<LootTable.Entry>();
            foreach (var e in entries) list.Add(new LootTable.Entry { item = items[e.id], dropChance = e.chance, min = e.min, max = e.max });
            table.entries = list.ToArray();
            table.goldMin = goldMin;
            table.goldMax = goldMax;
            EditorUtility.SetDirty(table);
            return table;
        }

        /// <summary>Kreatur mit SlimeAI (Werte pro Art), Beute und Schatten</summary>
        static GameObject CreateCreaturePrefab(string name, string id, SpriteAnimationSet anim, LootTable table, GameObject pickup, GameObject goldPickup,
            int hp, float speed, int damage, int xp, float detect, float radius, float shadow)
        {
            var go = new GameObject(name) { layer = LayerEnemy };
            TopDownBody(go, 0.8f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;
            col.offset = new Vector2(0f, 0.12f);
            go.AddComponent<Health>().maxHealth = hp;
            var ai = go.AddComponent<SlimeAI>();
            ai.creatureId = id;
            ai.speed = speed;
            ai.contactDamage = damage;
            ai.xpReward = xp;
            ai.detectRadius = detect;
            ai.playAttack = true;
            var dropper = go.AddComponent<LootDropper>();
            dropper.table = table;
            dropper.pickupPrefab = pickup.GetComponent<Pickup>();
            dropper.goldPickupPrefab = goldPickup.GetComponent<Pickup>();
            Visual(go.transform, anim);
            AddShadow(go.transform, shadow);
            return SavePrefab(go, name);
        }

        static System.Func<TileSet> ForestTiles() => () =>
        {
            Tile Load(string n)
            {
                var t = AssetDatabase.LoadAssetAtPath<Tile>($"{SpritesTiles}/Tile_{n}.asset");
                if (t == null || t.sprite == null) throw new System.InvalidOperationException("Kachel fehlt: " + n);
                return t;
            }
            return new TileSet { grass = new[] { Load("Forest_Ground_0"), Load("Forest_Ground_1") }, path = Load("Forest_Path"), wall = Load("Forest_Thorns") };
        };

        static void ConfigureForestSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesForest }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileName(path).StartsWith("Forest_")) ConfigureTexture(path, 64f, new Vector2(0.5f, 0.5f));
                else ConfigureTexture(path, SpritePpu, FootPivot);
            }
            foreach (var n in new[] { "Forest_Ground_0", "Forest_Ground_1", "Forest_Path", "Forest_Thorns" })
                CreateTile("Tile_" + n, $"{SpritesForest}/{n}.png");
        }

        /// <summary>
        /// Wald (24 x 16 Kacheln, Weg in Kachelzeile -1): 4 Schattenwoelfe, 3 Riesenspinnen, 2 Schleime; Baeume, Felsen, Deko.
        /// Ankunft "Ankunft" links am Weg, Rueckkehr-Portal links (in die Hoehle, Ankunft "Wald").
        /// </summary>
        static void BuildForestScene(GameObject player, GameObject wolf, GameObject spider, GameObject slime, GameObject hud, GameObject mobile,
            GameObject menu, GameObject trade, GameObject portal)
        {
            var (scene, camGo) = NewWorldScene(null, 24, 16, -1, requireTilemapCollider: false, tileLoader: ForestTiles());
            Shadow = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFx}/FX_Shadow.png");
            new GameObject("SceneRules").AddComponent<SceneRules>().peaceful = false;
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null) globalLight.intensity = 0.8f;

            var p = (GameObject)PrefabUtility.InstantiatePrefab(player);
            p.transform.position = new Vector3(-8f, -0.6f, 0f);
            camGo.GetComponent<Core.CameraFollow>().target = p.transform;
            AddSpawn("Ankunft", new Vector2(-8f, -0.6f));

            var back = (GameObject)PrefabUtility.InstantiatePrefab(portal);
            back.name = "Portal_Hoehle";
            back.transform.position = new Vector3(-10f, 1.2f, 0f);
            var backPortal = back.GetComponent<Portal>();
            backPortal.targetScene = SceneCave;
            backPortal.targetSpawn = "Wald";
            PrefabUtility.RecordPrefabInstancePropertyModifications(backPortal);

            var nature = new GameObject("Nature").transform;
            SpriteRenderer Deco(string name, string file, Vector2 pos, float collider = 0f)
            {
                var go = new GameObject(name);
                go.transform.SetParent(nature, false);
                go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesForest}/{file}.png");
                sr.spriteSortPoint = SpriteSortPoint.Pivot;
                if (collider > 0f)
                {
                    go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    var c = go.AddComponent<CircleCollider2D>();
                    c.radius = collider;
                    c.offset = new Vector2(0f, collider * 0.5f);
                }
                return sr;
            }
            Vector2[] oaks = { new(-6f, 4.5f), new(2f, 5f), new(8.5f, 4f), new(-3f, -5f), new(5f, -5.2f), new(10f, -3.5f) };
            Vector2[] pines = { new(-9.5f, 5f), new(-1f, 3.6f), new(6f, 2.4f), new(-7f, -4.5f), new(1f, -4f), new(9.5f, 1f) };
            foreach (var o in oaks) Prop(nature, "Tree_Oak", "tree_oak", o, 0.3f);
            foreach (var o in pines) Prop(nature, "Tree_Pine", "tree_pine", o, 0.25f);
            foreach (var r in new[] { new Vector2(-4f, 2f), new Vector2(4f, -2.6f), new Vector2(7.5f, -1.2f) }) Prop(nature, "Rock", "rock", r, 0.3f);
            Deco("FallenLog", "fallen_log", new Vector2(-2.5f, -2.8f), 0.3f);
            Deco("FallenLog_2", "fallen_log", new Vector2(3.5f, 2.2f), 0.3f);
            Deco("Stump", "stump", new Vector2(-5.5f, -2.2f), 0.25f);
            Deco("Stump_2", "stump", new Vector2(9f, -0.2f), 0.25f);
            foreach (var m in new[] { new Vector2(-1.5f, 1.5f), new Vector2(5.5f, -3.8f), new Vector2(-8.5f, -3f), new Vector2(0.5f, 2.6f) }) Deco("Mushrooms", "mushrooms", m);
            foreach (var f in new[] { new Vector2(-7.5f, 2.5f), new Vector2(1.5f, -2.5f), new Vector2(8f, 3f), new Vector2(-3.5f, 4f), new Vector2(4.5f, 0.8f) }) Deco("Fern", "fern", f);

            var enemies = new GameObject("Enemies").transform;
            void Spawn(GameObject prefab, Vector2 pos)
            {
                var e = (GameObject)PrefabUtility.InstantiatePrefab(prefab, enemies);
                e.transform.position = pos;
            }
            foreach (var w in new[] { new Vector2(0f, 1f), new Vector2(3f, -1.5f), new Vector2(6f, 1.2f), new Vector2(7.5f, -2.6f) }) Spawn(wolf, w);
            foreach (var s in new[] { new Vector2(-1f, -3f), new Vector2(4f, 3.4f), new Vector2(10f, -1.6f) }) Spawn(spider, s);
            foreach (var s in new[] { new Vector2(-1.5f, -2f), new Vector2(-2.5f, 3f) }) Spawn(slime, s);   // > 5 Einheiten von der Ankunft

            AddCommonUi(hud, mobile, menu, trade);
            EditorSceneManager.SaveScene(scene, InteriorScenePath(SceneForest));
        }
    }
}
