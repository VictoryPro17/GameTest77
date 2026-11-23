using Reborn.Magic;
using Reborn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace Reborn.EditorTools
{
    /// <summary>
    /// Meilenstein 7b: begehbare Raeume. Je eine Szene fuer Kriegerhalle, Magierturm, Assassinenhaus und Hoehle
    /// (16 x 12 Kacheln, friedlich, dunkleres Licht). Ausgang: Fussmatte unten in der Mitte (Tuer zurueck vor das Haus),
    /// Ankunft "Eingang" knapp darueber. In der Hoehle steht das Portal zum Kampfraum (Ankunft von dort: "Portal").
    /// </summary>
    public static partial class RebornBuilder
    {
        const string SpritesInteriors = Root + "/Sprites/Interiors";
        public const string SceneWarrior = "Guild_Warrior", SceneMage = "Guild_Mage", SceneAssassin = "Guild_Assassin", SceneCave = "Cave";
        static string InteriorScenePath(string name) => $"{Root}/Scenes/{name}.unity";
        static readonly string[] InteriorScenes = { SceneWarrior, SceneMage, SceneAssassin, SceneCave };

        static Sprite InteriorSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesInteriors}/{file}.png");

        /// <summary>Kacheln 64 px mittig, alles andere 128 px pro Einheit mit Fusspunkt; Kachel-Assets fuer Boden und Wand.</summary>
        static void ConfigureInteriorSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesInteriors }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var file = System.IO.Path.GetFileName(path);
                if (file.StartsWith("Floor_") || file.StartsWith("Wall_")) ConfigureTexture(path, 64f, new Vector2(0.5f, 0.5f));
                else ConfigureTexture(path, SpritePpu, FootPivot);
            }
            foreach (var n in new[] { "Floor_Wood", "Floor_Stone", "Floor_Dark", "Floor_Cave", "Wall_Wood", "Wall_Mage", "Wall_Dark", "Wall_Cave" })
                CreateTile("Tile_Int_" + n, $"{SpritesInteriors}/{n}.png");
        }

        /// <summary>Laedt Boden- und Wandkachel eines Raums frisch von der Platte (nach dem Szenenwechsel).</summary>
        static System.Func<TileSet> InteriorTiles(string floor, string wall) => () =>
        {
            Tile Load(string n)
            {
                var tile = AssetDatabase.LoadAssetAtPath<Tile>($"{SpritesTiles}/Tile_Int_{n}.asset");
                if (tile == null || tile.sprite == null) throw new System.InvalidOperationException("Kachel fehlt: " + n);
                return tile;
            }
            var f = Load(floor);
            return new TileSet { grass = new[] { f }, path = f, wall = Load(wall) };
        };

        static GameObject InteriorProp(Transform parent, string name, string file, Vector2 pos, Vector2 box = default, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = InteriorSprite(file);
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sortingOrder = order;
            if (box != Vector2.zero)
            {
                go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                var col = go.AddComponent<BoxCollider2D>();
                col.size = box;
                col.offset = new Vector2(0f, box.y * 0.5f);
            }
            return go;
        }

        static void Glow(Transform parent, Color color, float intensity, float radius, float height)
        {
            PointLight("Glow", parent, color, intensity, radius).transform.localPosition = new Vector3(0f, height, 0f);
        }

        static void InteriorTorch(Transform parent, Vector2 pos)
        {
            var torch = Prop(parent, "Torch", "torch_0", pos);
            var loop = torch.AddComponent<SpriteLoop>();
            loop.frames = new[] { PropSprite("torch_0"), PropSprite("torch_1") };
            loop.fps = 6f;
            Glow(torch.transform, new Color(1f, 0.6f, 0.25f), 1.1f, 3f, 0.75f);
        }

        static void GuildMaster(Transform parent, string name, string displayName, string greeting, Vector2 pos)
        {
            var go = new GameObject("Npc_Meister_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.offset = new Vector2(0f, 0.12f);
            var npc = go.AddComponent<Npc>();
            npc.kind = NpcKind.Gildenmeister;
            npc.displayName = displayName;
            npc.greeting = greeting;
            npc.range = 1.5f;
            go.AddComponent<SpeechBubble>();
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sprite = InteriorSprite($"master_{name}_0");
            var loop = visual.AddComponent<SpriteLoop>();
            loop.frames = new[] { InteriorSprite($"master_{name}_0"), InteriorSprite($"master_{name}_1"), InteriorSprite($"master_{name}_2"), InteriorSprite($"master_{name}_3") };
            loop.fps = 4f;
            AddShadow(go.transform, 0.55f);
        }

        /// <summary>Unsichtbarer Tuer-Trigger (Kasten) mit Ziel; in der Stadt vor den Gildenhaeusern, drinnen auf der Fussmatte.</summary>
        static Door AddDoor(Transform parent, string name, Vector2 pos, Vector2 size, string targetScene, string targetSpawn)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
            var door = go.AddComponent<Door>();
            door.targetScene = targetScene;
            door.targetSpawn = targetSpawn;
            return door;
        }

        /// <summary>Raum-Grundgeruest: Szene, Licht, Spieler am Eingang, Ausgang mit Fussmatte, Oberflaeche.</summary>
        static (UnityEngine.SceneManagement.Scene scene, Transform room) NewInterior(string sceneName, string floor, string wall, float light,
            GameObject player, string exitSpawn)
        {
            var (scene, camGo) = NewWorldScene(null, 16, 12, 99, requireTilemapCollider: false, tileLoader: InteriorTiles(floor, wall));
            Shadow = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesFx}/FX_Shadow.png");   // nach dem Szenenwechsel frisch laden
            new GameObject("SceneRules").AddComponent<SceneRules>().peaceful = true;
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null) globalLight.intensity = light;
            camGo.GetComponent<Camera>().backgroundColor = new Color(0.03f, 0.03f, 0.04f);

            var p = (GameObject)PrefabUtility.InstantiatePrefab(player);
            p.transform.position = new Vector3(0f, -3.6f, 0f);
            camGo.GetComponent<Core.CameraFollow>().target = p.transform;
            AddSpawn("Eingang", new Vector2(0f, -3.6f));

            var room = new GameObject("Room").transform;
            InteriorProp(room, "Doormat", "doormat", new Vector2(0f, -4.9f), default, -8);
            AddDoor(room, "Tuer_Ausgang", new Vector2(0f, -4.8f), new Vector2(1.2f, 0.4f), SceneTravel.BaseScene, exitSpawn);
            return (scene, room);
        }

        static void SaveInterior(UnityEngine.SceneManagement.Scene scene, string name, GameObject hud, GameObject mobile, GameObject menu, GameObject trade)
        {
            AddCommonUi(hud, mobile, menu, trade);
            EditorSceneManager.SaveScene(scene, InteriorScenePath(name));
        }

        static void BuildInteriors(GameObject player, GameObject hud, GameObject mobile, GameObject menu, GameObject trade, GameObject portal)
        {
            // ---- Kriegerhalle: Holz, Waffenstaender, Uebungspuppen, Banner, langer Tisch
            var (s1, r1) = NewInterior(SceneWarrior, "Floor_Wood", "Wall_Wood", 0.6f, player, "Tuer_Krieger");
            GuildMaster(r1, "krieger", "Kriegerin Brunhild", "Willkommen, Kämpfer! Wer zu uns will, muss sich erst beweisen. Komm wieder, wenn du bereit bist.", new Vector2(0f, 2.6f));
            InteriorProp(r1, "WeaponRack_L", "weapon_rack", new Vector2(-5f, 3.9f), new Vector2(0.5f, 0.2f));
            InteriorProp(r1, "WeaponRack_R", "weapon_rack", new Vector2(5f, 3.9f), new Vector2(0.5f, 0.2f));
            InteriorProp(r1, "Banner_L", "banner", new Vector2(-2.4f, 4.5f));
            InteriorProp(r1, "Banner_R", "banner", new Vector2(2.4f, 4.5f));
            InteriorProp(r1, "Dummy_1", "dummy", new Vector2(-4.6f, -0.6f), new Vector2(0.25f, 0.15f));
            InteriorProp(r1, "Dummy_2", "dummy", new Vector2(-5.8f, 1.2f), new Vector2(0.25f, 0.15f));
            InteriorProp(r1, "Table", "table_long", new Vector2(4.2f, 0f), new Vector2(1.1f, 0.3f));
            InteriorTorch(r1, new Vector2(-6.4f, 4.3f));
            InteriorTorch(r1, new Vector2(6.4f, 4.3f));
            SaveInterior(s1, SceneWarrior, hud, mobile, menu, trade);

            // ---- Magierturm: Steinboden, Runenkreis, Regale, Alchemie, Kristall
            var (s2, r2) = NewInterior(SceneMage, "Floor_Stone", "Wall_Mage", 0.6f, player, "Tuer_Magier");
            InteriorProp(r2, "RuneCircle", "rune_circle", new Vector2(0f, 0.2f), default, -8);
            var crystal = InteriorProp(r2, "Crystal", "crystal_float", new Vector2(0f, 0.8f), new Vector2(0.3f, 0.15f));
            Glow(crystal.transform, new Color(0.45f, 0.85f, 1f), 1.4f, 3.5f, 0.6f);
            GuildMaster(r2, "magier", "Erzmagier Aldric", "Die Magie wählt ihre Schüler sorgfältig. Kehre zurück, wenn deine Zeit gekommen ist.", new Vector2(0f, 3f));
            foreach (var x in new[] { -5.2f, -3.8f, 3.8f, 5.2f })
                InteriorProp(r2, "Bookshelf", "bookshelf", new Vector2(x, 4.1f), new Vector2(0.55f, 0.2f));
            InteriorProp(r2, "Alchemy_L", "alchemy_table", new Vector2(-5f, -1.5f), new Vector2(0.5f, 0.2f));
            InteriorProp(r2, "Alchemy_R", "alchemy_table", new Vector2(5f, -1.5f), new Vector2(0.5f, 0.2f));
            InteriorProp(r2, "Candles", "candles", new Vector2(-2.2f, 3.6f));
            InteriorProp(r2, "Candles_2", "candles", new Vector2(2.2f, 3.6f));
            InteriorTorch(r2, new Vector2(-6.4f, 1.5f));
            InteriorTorch(r2, new Vector2(6.4f, 1.5f));
            SaveInterior(s2, SceneMage, hud, mobile, menu, trade);

            // ---- Assassinenhaus: dunkles Holz, Vorhaenge, Zielscheiben, Kerzen, Kisten
            var (s3, r3) = NewInterior(SceneAssassin, "Floor_Dark", "Wall_Dark", 0.45f, player, "Tuer_Assassine");
            GuildMaster(r3, "assassine", "Schattenmeisterin Nyx", "Niemand hat dich hereingebeten ... und doch bist du hier. Interessant. Wir sprechen uns noch.", new Vector2(0f, 2.6f));
            InteriorProp(r3, "Curtain_L", "curtain", new Vector2(-3.8f, 4.5f));
            InteriorProp(r3, "Curtain_R", "curtain", new Vector2(3.8f, 4.5f));
            InteriorProp(r3, "Target_1", "target", new Vector2(-5.4f, 0.2f), new Vector2(0.3f, 0.15f));
            InteriorProp(r3, "Target_2", "target", new Vector2(-5.4f, 2.2f), new Vector2(0.3f, 0.15f));
            foreach (var pos in new[] { new Vector2(-1.8f, 3.6f), new Vector2(1.8f, 3.6f), new Vector2(5.2f, -2f) })
            {
                var c = InteriorProp(r3, "Candles", "candles", pos);
                Glow(c.transform, new Color(1f, 0.75f, 0.4f), 0.9f, 2.2f, 0.95f);
            }
            var crate = new GameObject("Crate");
            crate.transform.SetParent(r3, false);
            crate.transform.position = new Vector2(5.2f, 1.2f);
            var csr = crate.AddComponent<SpriteRenderer>();
            csr.sprite = BaseSprite("crate");
            csr.spriteSortPoint = SpriteSortPoint.Pivot;
            SaveInterior(s3, SceneAssassin, hud, mobile, menu, trade);

            // ---- Hoehle: Fels, Stalagmiten, leuchtende Kristalle, Portal zum Kampfraum
            var (s4, r4) = NewInterior(SceneCave, "Floor_Cave", "Wall_Cave", 0.4f, player, "Portal");
            var gate = (GameObject)PrefabUtility.InstantiatePrefab(portal);
            PrefabUtility.UnpackPrefabInstance(gate, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            gate.name = "Portal_Kampf";
            gate.transform.SetParent(r4, true);
            gate.transform.position = new Vector3(0f, 2.6f, 0f);
            var gatePortal = gate.GetComponent<Portal>();
            gatePortal.targetScene = SceneTravel.FightScene;
            gatePortal.targetSpawn = "Ankunft";
            AddSpawn("Portal", new Vector2(0f, 1.1f));   // Rueckkehr aus dem Kampfraum
            foreach (var pos in new[] { new Vector2(-5.5f, 3.6f), new Vector2(-4.2f, -2f), new Vector2(5.3f, 3.2f), new Vector2(4.4f, -2.6f), new Vector2(-6f, 0.6f) })
                InteriorProp(r4, "Stalagmite", "stalagmite", pos, new Vector2(0.3f, 0.15f));
            foreach (var pos in new[] { new Vector2(-3f, 3.9f), new Vector2(3.2f, 4f), new Vector2(6f, 0.4f), new Vector2(-2.6f, -3.4f) })
            {
                var c = InteriorProp(r4, "Crystals", "crystal_cluster", pos, new Vector2(0.3f, 0.15f));
                Glow(c.transform, new Color(0.4f, 0.8f, 1f), 1.1f, 2.6f, 0.3f);
            }
            SaveInterior(s4, SceneCave, hud, mobile, menu, trade);
        }
    }
}
