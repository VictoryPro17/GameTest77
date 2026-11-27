using System.Collections.Generic;
using System.IO;
using Reborn.Animation;
using Reborn.Economy;
using Reborn.Loot;
using Reborn.Magic;
using Reborn.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Reborn.EditorTools
{
    /// <summary>
    /// Meilenstein 7a: Die Basis wird zur Stadt (Szene "Base", 48 x 36 Kacheln). Stadtmauer rundum mit Ecktuermen,
    /// Soldaten auf der Mauer und Wachen am Tor; Marktplatz mit Brunnen, Haendler und Schmied; Laeden fuer Waffen,
    /// Ruestung und Traenke; Rathaus, Gasthaus, Wohnviertel; im Norden die drei Gildenhaeuser (Magier, Krieger,
    /// Assassinen); Felsen mit Hoehle, in der das Portal zum Kampfraum liegt; Teich mit Fischern; Bewohner laufen umher.
    /// Grafik: build_town.py und build_villagers.py (Sprites/Town). Gilden-Innenraeume und Quests folgen in 7b/7c.
    /// </summary>
    public static partial class RebornBuilder
    {
        const string SpritesTown = Root + "/Sprites/Town";
        /// <summary>Meter in Blender pro Einheit in Unity (128 px = 2,1 m) und Tiefe (45 Grad: 0,707 / 2,1)</summary>
        const float MetersPerUnit = 2.1f, DepthPerMeter = 0.707f / 2.1f;

        static readonly (string, float, bool, int)[] VillagerClips = { ("idle", 4f, true, 4), ("walk", 10f, true, 8) };

        static Sprite TownSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesTown}/{file}.png");

        /// <summary>Bodenkacheln 64 px mittig, alles andere 128 px pro Einheit mit Fusspunkt.</summary>
        static void ConfigureTownSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesTown }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path).StartsWith("Ground_")) ConfigureTexture(path, 64f, new Vector2(0.5f, 0.5f));
                else ConfigureTexture(path, SpritePpu, FootPivot);
            }
        }

        static Tile TownTile(string name, string file) => CreateTile(name, $"{SpritesTown}/{file}.png");

        static ShopData CreateTownShop(Dictionary<string, ItemData> items, string asset, string[] ids)
        {
            Directory.CreateDirectory(DataEconomy);
            var shop = LoadOrCreate<ShopData>($"{DataEconomy}/{asset}.asset");
            shop.items = new List<ItemData>();
            foreach (var id in ids)
                if (items.TryGetValue(id, out var item)) shop.items.Add(item);
            EditorUtility.SetDirty(shop);
            return shop;
        }

        /// <summary>
        /// Gebaeude oder Gegenstand aus Sprites/Town mit Fusspunkt-Sortierung. Grundflaeche in Metern (Blender):
        /// halbe Breite, halbe Tiefe und Mitte; daraus wird ein fester Kasten am Fuss (45-Grad-Ansicht: Tiefe x 0,707).
        /// </summary>
        static GameObject TownProp(Transform parent, string name, string file, Vector2 pos, float halfW = 0f, float halfD = 0f,
            float centerX = 0f, float centerY = 0f, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TownSprite(file);
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sortingOrder = order;
            if (halfW > 0f) AddFootprint(go, halfW, halfD, centerX, centerY);
            return go;
        }

        static void AddFootprint(GameObject go, float halfW, float halfD, float centerX = 0f, float centerY = 0f)
        {
            if (go.GetComponent<Rigidbody2D>() == null) go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2f * halfW / MetersPerUnit, Mathf.Max(0.1f, 2f * halfD * DepthPerMeter));
            box.offset = new Vector2(centerX / MetersPerUnit, centerY * DepthPerMeter);
        }

        /// <summary>Figur aus Sprites/Town/Villager_name: laeuft zwischen Wegpunkten (oder steht, wenn keine).</summary>
        static GameObject Villager(Transform parent, string name, SpriteAnimationSet set, Vector2[] waypoints, float speed = 1.1f, int order = 0)
        {
            var go = new GameObject("Villager_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = waypoints[0];
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sortingOrder = order;
            var anim = visual.AddComponent<DirectionalSpriteAnimator>();
            anim.animationSet = set;
            anim.startClip = "idle";
            if (set.clips.Length > 0 && set.clips[0].down.Length > 0) sr.sprite = set.clips[0].down[0];
            AddShadow(go.transform, 0.5f);
            if (order != 0)
                foreach (var shadow in go.GetComponentsInChildren<SpriteRenderer>())
                    if (shadow != sr) shadow.sortingOrder = order - 1;
            var w = go.AddComponent<Wanderer>();
            w.waypoints = waypoints;
            w.speed = speed;
            w.animator = anim;
            return go;
        }

        /// <summary>Haendler-NPC mit der Warte-Animation einer Stadtfigur (nach unten, 4 Bilder).</summary>
        static GameObject TownTrader(Transform parent, string villager, string displayName, ShopData shop, Vector2 position)
        {
            var go = new GameObject("Npc_" + villager);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.offset = new Vector2(0f, 0.12f);
            var npc = go.AddComponent<Npc>();
            npc.kind = NpcKind.Haendler;
            npc.displayName = displayName;
            npc.shop = shop;
            npc.range = 1.5f;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var frames = new Sprite[4];
            for (int i = 0; i < 4; i++) frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesTown}/Villager_{villager}/idle_down_{i}.png");
            sr.sprite = frames[0];
            var loop = visual.AddComponent<SpriteLoop>();
            loop.frames = frames;
            loop.fps = 4f;
            AddShadow(go.transform, 0.55f);
            return go;
        }

        static void Lantern(Transform parent, Vector2 pos)
        {
            var go = TownProp(parent, "Lantern", "lantern", pos, 0.06f, 0.06f);
            PointLight("Glow", go.transform, new Color(1f, 0.78f, 0.45f), 0.7f, 2.2f).transform.localPosition = new Vector3(0f, 0.65f, 0f);
        }

        /// <summary>
        /// Fuellt freie Grasflaechen mit Baeumen, Bueschen und Grasbuescheln (immer gleiches Muster). Frei bleiben: Pflaster,
        /// Erdwege, Wasser, die Naehe aller schon gesetzten Bilder und die Laufstrecken der Tests (Spalte x = -1,3 nach Norden,
        /// Zeile y = 0 nach Westen). Baeume nur mit Abstand zur Mauer, damit sie die Soldaten nicht verdecken.
        /// </summary>
        static void ScatterGreenery(Transform parent, Tilemap ground, TileSet tiles)
        {
            var taken = new List<(Vector2 pos, float radius)>();
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>())
                if (sr.sprite != null) taken.Add(((Vector2)sr.transform.position, sr.sprite.bounds.extents.x + 0.5f));
            var grass = new HashSet<TileBase>(tiles.grass);
            bool Free(Vector2 p, float r)
            {
                foreach (var (pos, radius) in taken)
                    if ((p - pos).sqrMagnitude < (radius + r) * (radius + r)) return false;
                return true;
            }
            int n = 0;
            for (int x = -21; x <= 20; x++)
                for (int y = -15; y <= 14; y++)
                {
                    if (!grass.Contains(ground.GetTile(new Vector3Int(x, y, 0)))) continue;
                    bool nearRoad = false;
                    for (int dx = -1; dx <= 1 && !nearRoad; dx++)
                        for (int dy = -1; dy <= 1 && !nearRoad; dy++)
                            nearRoad = !grass.Contains(ground.GetTile(new Vector3Int(x + dx, y + dy, 0)));
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if ((Mathf.Abs(p.x + 1.3f) < 1.2f && p.y > -2f && p.y < 11.5f) || Mathf.Abs(p.y) < 1.2f) continue;   // Teststrecken
                    uint hash = (uint)((x + 100) * 2654435761u) ^ (uint)((y + 100) * 40503u);
                    float roll = (hash % 1000u) / 1000f;
                    bool inner = x > -19 && x < 18 && y > -14 && y < 13;
                    if (!nearRoad && inner && roll < 0.05f && Free(p, 1.0f))
                    {
                        Prop(parent, "Tree_" + n++, roll < 0.025f ? "tree_oak" : "tree_pine", p, 0.25f);
                        taken.Add((p, 1.0f));
                    }
                    else if (!nearRoad && roll < 0.11f && Free(p, 0.4f))
                    {
                        Prop(parent, "Bush_" + n++, "bush", p, 0.2f);
                        taken.Add((p, 0.4f));
                    }
                    else if (roll > 0.72f && Free(p, 0.1f))
                    {
                        Prop(parent, "Grass_" + n++, "grass_tuft", p + new Vector2(((hash >> 10) % 40u) / 100f - 0.2f, 0f));
                    }
                }
        }

        // ------------------------------------------------------------ Stadt-Szene
        static void BuildTownScene(TileSet tiles, GameObject player, GameObject hud, GameObject mobile, GameObject menu,
            GameObject trade, GameObject portal, ShopData shop, Dictionary<string, ItemData> items)
        {
            ConfigureTownSprites();
            const int W = 48, H = 36;   // x -24..23, y -18..17
            var (scene, camGo) = NewWorldScene(tiles, W, H, 99, requireTilemapCollider: false);
            // Kacheln erst nach dem Szenenwechsel anlegen bzw. frisch laden: vorher erzeugte Referenzen sind danach ungueltig
            tiles = ReloadTiles();
            var cobble = new[] { TownTile("Tile_Cobble_0", "Ground_Cobble_0"), TownTile("Tile_Cobble_1", "Ground_Cobble_1"), TownTile("Tile_Cobble_2", "Ground_Cobble_2") };
            var cobbleEdge = TownTile("Tile_CobbleEdge", "Ground_CobbleEdge");
            var water = TownTile("Tile_Water", "Ground_Water_0");
            var bridge = TownTile("Tile_Bridge", "Ground_Bridge");
            new GameObject("SceneRules").AddComponent<SceneRules>().peaceful = true;
            var ground = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
            var wallMap = GameObject.Find("Grid/Walls").GetComponent<Tilemap>();
            // Hintergrund hinter der Mauer wie dunkler Wald
            camGo.GetComponent<Camera>().backgroundColor = new Color(0.1f, 0.16f, 0.1f);

            // ---- Boden: Pflaster fuer Platz und Strassen, Erdwege, Teich
            var isCobble = new HashSet<Vector2Int>();
            void Cobble(int x0, int x1, int y0, int y1)
            {
                for (int x = x0; x <= x1; x++)
                    for (int y = y0; y <= y1; y++) isCobble.Add(new Vector2Int(x, y));
            }
            Cobble(-6, 5, -5, 2);       // Marktplatz
            Cobble(-1, 0, -16, 12);     // Hauptstrasse vom Tor (Sueden) bis zum Magierturm (Norden)
            Cobble(-15, 14, 6, 7);      // Gildenstrasse
            Cobble(-11, 10, -11, -10);  // Ladenstrasse
            Cobble(-21, -16, 6, 7);     // Weg zur Hoehle
            foreach (var c in isCobble)
            {
                var cell = new Vector3Int(c.x, c.y, 0);
                bool up = !isCobble.Contains(c + Vector2Int.up), down = !isCobble.Contains(c + Vector2Int.down);
                bool left = !isCobble.Contains(c + Vector2Int.left), right = !isCobble.Contains(c + Vector2Int.right);
                if (up || down || left || right)
                {
                    ground.SetTile(cell, cobbleEdge);
                    float angle = up ? 0f : down ? 180f : left ? 90f : 270f;
                    ground.SetTransformMatrix(cell, Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle)));
                }
                else
                {
                    uint hash = (uint)((c.x + 50) * 73856093) ^ (uint)((c.y + 50) * 19349663);
                    ground.SetTile(cell, cobble[hash % (uint)cobble.Length]);
                }
            }
            for (int x = -21; x <= 21; x++)             // Erdwege in die Wohnviertel
                foreach (int y in new[] { 1, 2 })
                    if (!isCobble.Contains(new Vector2Int(x, y)) && (x < -6 || x > 5)) ground.SetTile(new Vector3Int(x, y, 0), tiles.path);
            for (int y = -9; y <= 5; y++)
                foreach (int x in new[] { -16, 15 })
                    ground.SetTile(new Vector3Int(x, y, 0), tiles.path);
            for (int x = 15; x <= 20; x++)              // Teich im Suedosten mit Steg
                for (int y = -15; y <= -12; y++)
                    ground.SetTile(new Vector3Int(x, y, 0), x == 15 && y == -12 ? bridge : water);

            // ---- Mauer: Seiten 2 Kacheln breit (Wehrgang von oben), Norden und Sueden als Mauerbild, Ecktuerme
            for (int y = -H / 2; y < H / 2; y++)
            {
                wallMap.SetTile(new Vector3Int(-W / 2 + 1, y, 0), tiles.wall);
                wallMap.SetTile(new Vector3Int(W / 2 - 2, y, 0), tiles.wall);
            }
            var wallRoot = GameObject.Find("Grid/Walls").transform;
            var town = new GameObject("Town").transform;
            var walls = new GameObject("TownWall").transform;
            walls.SetParent(town, false);
            for (int x = -W / 2 + 2; x < W / 2 - 2; x++)
            {
                TownProp(walls, $"WallN_{x}", "wall_segment", new Vector2(x + 0.5f, 16.6f));
                if (x < -1 || x > 0) TownProp(walls, $"WallS_{x}", "wall_segment", new Vector2(x + 0.5f, -17.2f));
            }
            TownProp(walls, "Gate", "gate", new Vector2(0f, -17.2f), 2.3f, 0.6f);
            foreach (var p in new[] { new Vector2(-22.5f, 16.4f), new Vector2(22.5f, 16.4f), new Vector2(-22.5f, -17.3f), new Vector2(22.5f, -17.3f) })
                TownProp(walls, "CornerTower", "corner_tower", p, 0.75f, 0.75f);
            foreach (var p in new[] { new Vector2(-22.5f, 0f), new Vector2(22.5f, 0f), new Vector2(-11f, 16.5f), new Vector2(11f, 16.5f) })
                TownProp(walls, "WallTower", "wall_tower", p, 0.6f, 0.6f);
            // Feste Kaesten der Stadtmauer (unter Grid/Walls, damit die Mauer-Pruefungen sie finden)
            var wallBodyGo = new GameObject("TownWallBounds") { layer = LayerWalls };
            wallBodyGo.transform.SetParent(wallRoot, false);
            wallBodyGo.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            void WallBox(float x0, float x1, float y0, float y1)
            {
                var box = wallBodyGo.AddComponent<BoxCollider2D>();
                box.offset = new Vector2((x0 + x1) / 2f, (y0 + y1) / 2f);
                box.size = new Vector2(x1 - x0, y1 - y0);
            }
            WallBox(-24f, 24f, 16.45f, 18f);    // Norden
            WallBox(-24f, 24f, -18f, -16.95f);  // Sueden (das Tor ist zu)
            WallBox(-24f, -22f, -18f, 18f);     // Westen
            WallBox(22f, 24f, -18f, 18f);       // Osten

            // ---- Spieler, Ankunftspunkte
            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(player);
            playerInstance.transform.position = new Vector3(0f, -2f, 0f);
            camGo.GetComponent<Core.CameraFollow>().target = playerInstance.transform;
            AddSpawn("Start", new Vector2(0f, -2f));
            AddSpawn("Portal", new Vector2(-18.5f, 9.3f));

            // ---- Marktplatz: Brunnen, Haendlerstand, Schmiede, Rathaus, Gasthaus
            var plaza = new GameObject("Plaza").transform;
            plaza.SetParent(town, false);
            var fountain = TownProp(plaza, "Fountain", "fountain_0", new Vector2(0f, 1.2f), 0.95f, 0.75f);
            var fl = fountain.AddComponent<SpriteLoop>();
            fl.frames = new[] { TownSprite("fountain_0"), TownSprite("fountain_1") };
            fl.fps = 3f;
            BaseProp(plaza, "Stall", "stall", new Vector2(-1.4f, -3.3f), new Vector2(0.7f, 0.25f), new Vector2(0f, 0.1f));
            BaseProp(plaza, "Anvil", "anvil", new Vector2(2.6f, -3.4f), new Vector2(0.32f, 0.16f), new Vector2(0f, 0.08f));
            var smithy = TownProp(plaza, "Smithy", "smithy", new Vector2(5.6f, -4.2f), 1.5f, 0.7f, 0.35f, 0.35f);
            AddFootprint(smithy, 0.42f, 0.36f, -1.45f, -0.35f);
            var smoke = TownProp(plaza, "Smoke", "smoke_0", new Vector2(4.9f, -3.4f), order: 1);
            var sl = smoke.AddComponent<SpriteLoop>();
            sl.frames = new[] { TownSprite("smoke_0"), TownSprite("smoke_1"), TownSprite("smoke_2") };
            sl.fps = 3f;
            PointLight("ForgeGlow", smithy.transform, new Color(1f, 0.55f, 0.2f), 1.1f, 2.4f).transform.localPosition = new Vector3(-0.7f, 0.3f, 0f);
            TownProp(plaza, "TownHall", "town_hall", new Vector2(-4.1f, 4.1f), 2.3f, 1.1f);
            TownProp(plaza, "Inn", "inn", new Vector2(4.1f, 4.1f), 2.08f, 1.1f);
            foreach (var p in new[] { new Vector2(-2.6f, 0.4f), new Vector2(2.6f, 0.4f), new Vector2(-2.6f, -1.2f), new Vector2(2.6f, -1.2f) })
                TownProp(plaza, "Bench", "bench", p);
            foreach (var p in new[] { new Vector2(-2.8f, -3.6f), new Vector2(-0.2f, -3.7f), new Vector2(-3.4f, -3.9f) })
                TownProp(plaza, "MarketCrate", "market_crate", p, 0.3f, 0.22f);
            TownProp(plaza, "Cart", "cart", new Vector2(-5f, -3.4f), 0.6f, 0.35f);
            TownProp(plaza, "Hay_Plaza", "hay", new Vector2(-5.6f, -2.2f), 0.35f, 0.3f);
            TownProp(plaza, "Well", "well", new Vector2(-5f, 1.2f), 0.45f, 0.45f);
            foreach (var p in new[] { new Vector2(-1.4f, 2.4f), new Vector2(1.4f, 2.4f), new Vector2(-4.4f, -0.4f), new Vector2(4.4f, -0.4f) })
                TownProp(plaza, "Planter", "planter", p);
            foreach (var p in new[] { new Vector2(-2f, 3.2f), new Vector2(2f, 3.2f), new Vector2(-6.3f, -1f), new Vector2(5.7f, -1f),
                                      new Vector2(-1.8f, -9.2f), new Vector2(1.8f, -9.2f), new Vector2(-1.8f, 5.6f), new Vector2(1.8f, 5.6f),
                                      new Vector2(-1.8f, -14f), new Vector2(1.8f, -14f) })
                Lantern(plaza, p);
            TownProp(plaza, "Signpost", "signpost", new Vector2(1.6f, 6.9f));

            var npcs = new GameObject("Npcs").transform;
            // Meilenstein 10b: Mentor Aldric neben dem Startpunkt (Einfuehrung)
            AddMentor(npcs, AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputPath), items["heiltrank_klein"], new Vector2(-1.3f, -1.5f));
            AddNpc(npcs, "haendler", NpcKind.Haendler, "Händler", shop, new Vector2(-1.4f, -2.75f));
            AddNpc(npcs, "schmied", NpcKind.Schmied, "Schmied", null, new Vector2(3.3f, -3.0f));

            // ---- Ladenstrasse im Sueden: Waffen, Ruestung, Traenke, Gemischtwaren
            var shops = new GameObject("Shops").transform;
            shops.SetParent(town, false);
            var weaponShop = CreateTownShop(items, "Shop_Waffen", new[] { "holzschwert", "eisenschwert", "eichenstab", "holzschild" });
            var armorShop = CreateTownShop(items, "Shop_Ruestung", new[] { "lederkappe", "lederweste", "lederhose", "lederstiefel", "lederhandschuhe",
                                                                            "eisenhelm", "eisenbrust", "eisenhose", "eisenstiefel", "eisenhandschuhe" });
            var potionShop = CreateTownShop(items, "Shop_Traenke", new[] { "heiltrank_klein", "heiltrank", "manatrank_klein", "manatrank" });
            TownProp(shops, "Shop_Weapons", "shop_weapons", new Vector2(-8.5f, -8.3f), 1.2f, 0.8f);
            TownTrader(npcs, "waffenhaendler", "Waffenhändler", weaponShop, new Vector2(-8.5f, -9.4f));
            TownProp(shops, "Shop_Armor", "shop_armor", new Vector2(-4.5f, -8.3f), 1.2f, 0.8f);
            TownTrader(npcs, "ruestungshaendler", "Rüstungshändler", armorShop, new Vector2(-4.5f, -9.4f));
            TownProp(shops, "Shop_Potions", "shop_potions", new Vector2(4.5f, -8.3f), 1.2f, 0.8f);
            TownTrader(npcs, "alchemistin", "Alchemistin", potionShop, new Vector2(4.5f, -9.4f));
            TownProp(shops, "Shop_Goods", "shop", new Vector2(8.5f, -8.3f), 1.2f, 0.8f);
            TownProp(shops, "Barrel", "hay", new Vector2(10.2f, -8.9f), 0.3f, 0.3f);

            // ---- Wohnviertel West und Ost, Haeuser im Sueden
            var houses = new GameObject("Houses").transform;
            houses.SetParent(town, false);
            (string file, Vector2 pos, float halfW)[] homes =
            {
                ("house_tall_blue", new Vector2(-19f, 3.6f), 1.2f), ("house_red", new Vector2(-13f, 3.6f), 1.2f),
                ("house_wide_green", new Vector2(-19f, -2.4f), 1.8f), ("house_green", new Vector2(-12.5f, -2.4f), 1.2f),
                ("house_blue", new Vector2(12.5f, 3.6f), 1.2f), ("house_tall_red", new Vector2(19f, 3.6f), 1.2f),
                ("house_red", new Vector2(12.5f, -2.4f), 1.2f), ("house_wide_green", new Vector2(19f, -2.4f), 1.8f),
                ("house_green", new Vector2(-19f, -9f), 1.2f), ("house_tall_red", new Vector2(-13f, -9f), 1.2f),
                ("house_blue", new Vector2(-8f, -14.2f), 1.2f), ("house_red", new Vector2(-4f, -14.2f), 1.2f),
                ("house_tall_blue", new Vector2(4.5f, -14.2f), 1.2f), ("house_green", new Vector2(9f, -14.2f), 1.2f),
                ("house_blue", new Vector2(12.5f, -8.5f), 1.2f),
            };
            for (int i = 0; i < homes.Length; i++)
            {
                var (file, pos, halfW) = homes[i];
                TownProp(houses, $"House_{i:00}", file, pos, halfW, 0.85f);
                TownProp(houses, $"Planter_{i:00}", "planter", pos + new Vector2(halfW / MetersPerUnit + 0.35f, -0.3f));
            }
            foreach (var p in new[] { new Vector2(-16.5f, -5f), new Vector2(-15.5f, -5f), new Vector2(15.5f, -5f), new Vector2(16.5f, -5f) })
                TownProp(houses, "Fence", "fence", p, 1.05f, 0.05f);
            TownProp(houses, "Hay", "hay", new Vector2(-20.5f, -5.6f), 0.35f, 0.3f);
            TownProp(houses, "Hay_2", "hay", new Vector2(-19.6f, -5.9f), 0.35f, 0.3f);
            TownProp(houses, "Cart_2", "cart", new Vector2(20f, -6f), 0.6f, 0.35f);

            // ---- Gildenviertel im Norden
            var guilds = new GameObject("Guilds").transform;
            guilds.SetParent(town, false);
            TownProp(guilds, "Guild_Warrior", "guild_warrior", new Vector2(-9.5f, 10f), 3.9f, 1.8f);
            // Magierturm: Sockel 1,5 m Radius plus Kristallsaeulen bei +/-1,8 m (Modell x 1,5)
            TownProp(guilds, "Guild_Mage", "guild_mage", new Vector2(-0.5f, 11.3f), 2.0f, 1.5f, 0f, 0.0f);
            TownProp(guilds, "Guild_Assassin", "guild_assassin", new Vector2(9.5f, 10f), 2.85f, 1.5f);
            foreach (var p in new[] { new Vector2(-14.5f, 8.3f), new Vector2(-4.6f, 8.3f), new Vector2(4.6f, 8.3f), new Vector2(14.5f, 8.3f) })
                Lantern(guilds, p);
            // Meilenstein 7b: Tueren an der Vorderkante der Grundflaeche, Ankunft davor
            AddDoor(guilds, "Tuer_Gilde_Krieger", new Vector2(-9.5f, 10f - 1.8f * DepthPerMeter - 0.12f), new Vector2(0.8f, 0.25f), SceneWarrior, "Eingang");
            AddDoor(guilds, "Tuer_Gilde_Magier", new Vector2(-0.5f, 11.3f - 1.5f * DepthPerMeter - 0.12f), new Vector2(0.8f, 0.25f), SceneMage, "Eingang");
            AddDoor(guilds, "Tuer_Gilde_Assassine", new Vector2(9.5f, 10f - 1.5f * DepthPerMeter - 0.12f), new Vector2(0.8f, 0.25f), SceneAssassin, "Eingang");
            AddSpawn("Tuer_Krieger", new Vector2(-9.5f, 8.6f));
            AddSpawn("Tuer_Magier", new Vector2(-0.5f, 9.9f));
            AddSpawn("Tuer_Assassine", new Vector2(9.5f, 8.7f));
            for (int i = 0; i < 6; i++)
                TownProp(guilds, $"Hedge_{i}", "hedge", new Vector2(-17.5f + i * 7f, 14.6f), 1.05f, 0.3f);

            // ---- Hoehle im Nordwesten: das Portal zum Kampfraum liegt im Felsen
            var cave = new GameObject("Cave").transform;
            cave.SetParent(town, false);
            var rock = TownProp(cave, "CaveRock", "cave_rock", new Vector2(-18.5f, 11f));
            AddFootprint(rock, 2.1f, 0.6f, 0f, 1.0f);
            AddFootprint(rock, 0.8f, 0.5f, -1.5f, 0.1f);
            AddFootprint(rock, 0.75f, 0.5f, 1.55f, 0.2f);
            var gate = (GameObject)PrefabUtility.InstantiatePrefab(portal);
            PrefabUtility.UnpackPrefabInstance(gate, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            gate.name = "Tuer_Hoehle";   // Meilenstein 7b: fuehrt in die Hoehle (dort steht das Portal zum Kampfraum)
            gate.transform.SetParent(cave, true);
            gate.transform.position = new Vector3(-18.5f, 10.15f, 0f);
            var visualChild = gate.transform.Find("Visual");
            if (visualChild != null) Object.DestroyImmediate(visualChild.gameObject);   // man sieht nur das Leuchten im Hoehlenloch
            var gatePortal = gate.GetComponent<Portal>();
            gatePortal.targetScene = SceneCave;
            gatePortal.targetSpawn = "Eingang";

            // ---- Teich im Suedosten, Baeume an der Mauer
            var nature = new GameObject("Nature").transform;
            nature.SetParent(town, false);
            var pond = new GameObject("PondBounds") { layer = LayerWalls };
            pond.transform.SetParent(nature, false);
            pond.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var pondBox = pond.AddComponent<BoxCollider2D>();
            pondBox.offset = new Vector2(18.5f, -13f);
            pondBox.size = new Vector2(5.6f, 3.8f);
            Prop(nature, "Tree_Oak", "tree_oak", new Vector2(-20.5f, 14f), 0.3f);
            Prop(nature, "Tree_Pine", "tree_pine", new Vector2(20.5f, 14f), 0.25f);
            Prop(nature, "Tree_Oak_2", "tree_oak", new Vector2(20.5f, 9f), 0.3f);
            Prop(nature, "Tree_Pine_2", "tree_pine", new Vector2(-20.8f, -14.5f), 0.25f);
            Prop(nature, "Tree_Oak_3", "tree_oak", new Vector2(13.5f, -15f), 0.3f);
            Prop(nature, "Rock", "rock", new Vector2(14.2f, -11f), 0.3f);
            foreach (var p in new[] { new Vector2(-6f, -12.8f), new Vector2(6.5f, -12.6f), new Vector2(-10f, 4.8f), new Vector2(11f, 0.2f), new Vector2(-17f, 12.8f) })
                Prop(nature, "Bush", "bush", p, 0.25f);
            foreach (var p in new[] { new Vector2(-5f, 13.6f), new Vector2(6f, 13.8f), new Vector2(-15f, -15.5f), new Vector2(16f, 7.6f), new Vector2(-9f, -5.8f) })
                Prop(nature, "Grass", "grass_tuft", p);

            // ---- Leute: Bewohner, Fischer, Wachen am Tor, Soldaten auf der Mauer
            var people = new GameObject("People").transform;
            SpriteAnimationSet Set(string v) => CreateAnimationSet($"Villager_{v}_Anim", $"{SpritesTown}/Villager_{v}", VillagerClips);
            var baeuerin = Set("baeuerin"); var wache = Set("wache"); var gelehrter = Set("gelehrter"); var soldat = Set("soldat");
            var bauer = Set("bauer"); var edeldame = Set("edeldame"); var barde = Set("barde"); var fischer = Set("fischer");
            Villager(people, "baeuerin", baeuerin, new[] { new Vector2(-5.5f, -1.5f), new Vector2(-5.5f, -4.4f), new Vector2(-3f, -4.4f) }, 0.9f);
            Villager(people, "bauer", bauer, new[] { new Vector2(-17f, 1.5f), new Vector2(-10f, 1.5f), new Vector2(-10f, -0.5f) }, 1f);
            Villager(people, "edeldame", edeldame, new[] { new Vector2(-4.2f, 2.7f), new Vector2(4.2f, 2.7f) }, 0.8f);
            Villager(people, "gelehrter", gelehrter, new[] { new Vector2(-0.5f, 4f), new Vector2(-0.5f, 7.2f), new Vector2(5f, 6.6f) }, 0.7f);
            Villager(people, "barde", barde, new[] { new Vector2(2.2f, 0.4f), new Vector2(-2.2f, 0.4f), new Vector2(-2.2f, -1f), new Vector2(2.2f, -1f) }, 0.8f);
            Villager(people, "baeuerin_2", baeuerin, new[] { new Vector2(10f, 1.5f), new Vector2(18f, 1.5f) }, 1f);
            Villager(people, "bauer_2", bauer, new[] { new Vector2(-6f, -10.6f), new Vector2(6f, -10.6f) }, 1.1f);
            Villager(people, "edeldame_2", edeldame, new[] { new Vector2(-13f, 6.6f), new Vector2(-4f, 6.6f) }, 0.8f);
            Villager(people, "fischer", fischer, new[] { new Vector2(15.3f, -10.9f) });
            Villager(people, "fischer_2", fischer, new[] { new Vector2(14.2f, -13.6f), new Vector2(14.2f, -12.4f) }, 0.4f);
            Villager(people, "wache_tor_L", wache, new[] { new Vector2(-1.6f, -16.2f) });
            Villager(people, "wache_tor_R", wache, new[] { new Vector2(1.6f, -16.2f) });
            Villager(people, "wache_platz", wache, new[] { new Vector2(-0.5f, -6.5f), new Vector2(-0.5f, -15.5f) }, 0.9f);
            // Soldaten oben auf der Mauer: Norden auf dem Wehrgang (vor der Mauer gezeichnet), Westen und Osten auf den Mauerkacheln
            Villager(people, "soldat_N1", soldat, new[] { new Vector2(-20f, 17.25f), new Vector2(-2f, 17.25f) }, 0.9f, order: 2);
            Villager(people, "soldat_N2", soldat, new[] { new Vector2(20f, 17.25f), new Vector2(2f, 17.25f) }, 0.9f, order: 2);
            Villager(people, "soldat_W", soldat, new[] { new Vector2(-22.5f, -15f), new Vector2(-22.5f, 14f) }, 0.9f, order: 2);
            Villager(people, "soldat_E", soldat, new[] { new Vector2(22.5f, 14f), new Vector2(22.5f, -15f) }, 0.9f, order: 2);
            Villager(people, "soldat_S", soldat, new[] { new Vector2(-20f, -16.55f), new Vector2(-4f, -16.55f) }, 0.9f, order: 2);

            ScatterGreenery(nature, ground, tiles);
            AddCommonUi(hud, mobile, menu, trade);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, BaseScenePath);
        }
    }
}
