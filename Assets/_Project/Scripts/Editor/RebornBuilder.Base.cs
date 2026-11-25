using System.Collections.Generic;
using Reborn.Animation;
using Reborn.Economy;
using Reborn.Inventory;
using Reborn.Loot;
using Reborn.Magic;
using Reborn.UI;
using Reborn.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 6: Basis-Szene mit Haus, Portal, Haendler und Schmied; Gold, Laden, Handelsfenster.</summary>
    public static partial class RebornBuilder
    {
        const string BaseScenePath = Root + "/Scenes/Base.unity";
        const string SpritesBase = Root + "/Sprites/Base";
        const string DataEconomy = Root + "/Data/Economy";

        static Sprite BaseSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesBase}/{file}.png");

        /// <summary>Sprites der Basis: Fusspunkt wie alle Gegenstaende, Gold-Icon mittig bei 96 PPU (0,5 Einheiten am Boden)</summary>
        static Sprite ConfigureBaseSprites()
        {
            ConfigureSprites(SpritesBase, FootPivot, SpritePpu);
            var goldPath = $"{SpritesBase}/gold.png";
            ConfigureTexture(goldPath, 96f, new Vector2(0.5f, 0.5f));
            return AssetDatabase.LoadAssetAtPath<Sprite>(goldPath);
        }

        static GameObject CreateGoldPickupPrefab(Sprite gold)
        {
            var go = new GameObject("GoldPickup") { layer = LayerPickup };
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
            go.AddComponent<Pickup>();
            var visual = new GameObject("Icon");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = gold;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            return SavePrefab(go, "GoldPickup");
        }

        static ShopData CreateShop(Dictionary<string, ItemData> items)
        {
            System.IO.Directory.CreateDirectory(DataEconomy);
            var shop = LoadOrCreate<ShopData>($"{DataEconomy}/Shop_Haendler.asset");
            shop.items = new List<ItemData>();
            foreach (var id in new[] { "heiltrank_klein", "manatrank_klein", "heiltrank", "manatrank",
                                       "eisenschwert", "eisenbrust", "eisenhose", "eisenstiefel", "eisenhandschuhe" })
                shop.items.Add(items[id]);
            EditorUtility.SetDirty(shop);
            return shop;
        }

        /// <summary>Portal: Steinbogen mit Wirbel (4 Bilder), Licht, Trigger am Fuss</summary>
        static GameObject CreatePortalPrefab()
        {
            var go = new GameObject("Portal");
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            col.offset = new Vector2(0f, 0.25f);
            go.AddComponent<Portal>();
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = BaseSprite("portal_0");
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var loop = visual.AddComponent<SpriteLoop>();
            loop.frames = new[] { BaseSprite("portal_0"), BaseSprite("portal_1"), BaseSprite("portal_2"), BaseSprite("portal_3") };
            loop.fps = 8f;
            PointLight("Glow", go.transform, new Color(0.65f, 0.45f, 1f), 1.2f, 2.5f).transform.localPosition = new Vector3(0f, 1f, 0f);
            return SavePrefab(go, "Portal");
        }

        // ------------------------------------------------------------ Handelsfenster (Haendler + Schmied)
        static ItemDetailView BuildDetail(Transform parent, Vector2 pos)
        {
            var detailPanel = Panel("Detail", parent, pos, new Vector2(500, 700), new Color(0.16f, 0.15f, 0.2f, 1f)).transform;
            var detail = detailPanel.gameObject.AddComponent<ItemDetailView>();
            detail.emptyText = MenuText("Empty", detailPanel, Vector2.zero, new Vector2(460, 80), 26, TextAnchor.MiddleCenter, "<color=#999999>Wähle einen Gegenstand</color>");
            var content = Stretch("Content", detailPanel);
            detail.content = content.gameObject;
            detail.iconImage = Rect("Icon", content, new Vector2(0.5f, 0.5f), new Vector2(0, 270), new Vector2(110, 110)).gameObject.AddComponent<Image>();
            detail.iconImage.preserveAspect = true;
            detail.nameText = MenuText("Name", content, new Vector2(0, 190), new Vector2(460, 50), 32, TextAnchor.MiddleCenter);
            detail.categoryText = MenuText("Category", content, new Vector2(0, 150), new Vector2(460, 36), 22, TextAnchor.MiddleCenter);
            detail.categoryText.color = new Color(0.75f, 0.75f, 0.8f);
            detail.descriptionText = MenuText("Description", content, new Vector2(0, 80), new Vector2(460, 100), 22, TextAnchor.UpperCenter);
            detail.statsText = MenuText("Stats", content, new Vector2(0, -60), new Vector2(460, 170), 24, TextAnchor.UpperLeft);
            detail.sellText = MenuText("Sell", content, new Vector2(0, -170), new Vector2(460, 36), 24, TextAnchor.MiddleLeft);
            detail.sellText.color = new Color(1f, 0.85f, 0.4f);
            detail.actionButton = MenuButton("Action", content, new Vector2(0, -280), new Vector2(340, 80), "Kaufen", 28, new Color(0.3f, 0.5f, 0.32f, 1f));
            detail.actionLabel = detail.actionButton.GetComponentInChildren<Text>();
            content.gameObject.SetActive(false);
            return detail;
        }

        static GameObject CreateTradeWindowPrefab(UiSprites ui, ItemData material)
        {
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var canvas = NewCanvas("TradeWindow", 31, true);
            var window = canvas.gameObject.AddComponent<TradeWindow>();
            window.actions = input;

            window.promptText = Label("Prompt", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0, 190), new Vector2(900, 50), 30, TextAnchor.MiddleCenter);
            window.promptText.font = T.headingFont;
            window.promptText.color = T.gold;
            window.promptText.text = "";

            var root = Stretch("Root", canvas.transform);
            root.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            window.root = root.gameObject;
            var panel = Panel("Panel", root, Vector2.zero, new Vector2(1500, 900), new Color(0.11f, 0.1f, 0.14f, 0.97f), PanelStyle.Window).transform;
            window.titleText = MenuText("Title", panel, new Vector2(-520, 395), new Vector2(400, 60), 40, TextAnchor.MiddleLeft, "Händler");
            window.titleText.font = T.headingFont;
            window.titleText.color = T.gold;
            window.goldText = MenuText("Gold", panel, new Vector2(330, 395), new Vector2(360, 60), 32, TextAnchor.MiddleRight, "Gold: 0");
            window.goldText.font = T.headingFont;
            window.goldText.color = new Color(1f, 0.82f, 0.3f);
            window.closeButton = MenuButton("Close", panel, new Vector2(690, 395), new Vector2(76, 62), "X", 34, new Color(0.55f, 0.2f, 0.22f, 1f));
            window.messageText = MenuText("Message", panel, new Vector2(-260, -330), new Vector2(760, 50), 28, TextAnchor.MiddleCenter);
            window.messageText.color = new Color(1f, 0.85f, 0.4f);
            MenuText("Hint", panel, new Vector2(0, -405), new Vector2(1400, 40), 22, TextAnchor.MiddleCenter,
                "<color=#BBBBBB>Enter / Süd-Taste: Kaufen · Verkaufen · Verbessern      Tab / Esc / Start: Schließen</color>");
            var tabColor = new Color(0.25f, 0.23f, 0.3f, 1f);

            // ---- Haendler
            var shopPage = Stretch("ShopPage", panel);
            var shop = shopPage.gameObject.AddComponent<ShopView>();
            window.shopView = shop;
            shop.buyTab = MenuButton("TabKaufen", shopPage, new Vector2(-560, 300), new Vector2(200, 60), "Kaufen", 28, tabColor, true);
            shop.sellTab = MenuButton("TabVerkaufen", shopPage, new Vector2(-345, 300), new Vector2(220, 60), "Verkaufen", 28, tabColor, true);
            var shopGrid = Rect("Grid", shopPage, new Vector2(0.5f, 0.5f), new Vector2(-260, 20), new Vector2(660, 440));
            shop.slots = new ItemSlotView[PlayerInventory.Size];
            for (int i = 0; i < PlayerInventory.Size; i++)
                shop.slots[i] = MenuSlot($"Slot_{i:00}", shopGrid, new Vector2(-275 + (i % 6) * 110, 165 - (i / 6) * 110), 100, ui, null);
            shop.detail = BuildDetail(shopPage, new Vector2(450, -10));

            // ---- Schmied
            var smithPage = Stretch("SmithPage", panel);
            var smith = smithPage.gameObject.AddComponent<SmithView>();
            window.smithView = smith;
            smith.material = material;
            MenuText("SmithTitle", smithPage, new Vector2(-260, 300), new Vector2(660, 50), 28, TextAnchor.MiddleCenter, "Ausrüstung verbessern (bis +5)", true);
            // 18 Plaetze: alle 14 verbesserbaren Arten (Waffen, Schild, Leder- und Eisen-Set) passen hinein
            var smithGrid = Rect("Grid", smithPage, new Vector2(0.5f, 0.5f), new Vector2(-260, 85), new Vector2(660, 330));
            smith.slots = new ItemSlotView[18];
            for (int i = 0; i < smith.slots.Length; i++)
                smith.slots[i] = MenuSlot($"Slot_{i:00}", smithGrid, new Vector2(-275 + (i % 6) * 110, 110 - (i / 6) * 110), 100, ui, null);
            smith.levelText = MenuText("Level", smithPage, new Vector2(-260, -200), new Vector2(660, 100), 30, TextAnchor.MiddleCenter);
            smith.levelText.color = new Color(1f, 0.85f, 0.4f);
            smith.detail = BuildDetail(smithPage, new Vector2(450, -10));

            // ---- Gilde (Meilenstein 7c): Begruessung, Rang, Quest, Knoepfe
            var guildPage = Stretch("GuildPage", panel);
            var guild = guildPage.gameObject.AddComponent<GuildView>();
            window.guildView = guild;
            guild.greetingText = MenuText("Greeting", guildPage, new Vector2(0, 270), new Vector2(1300, 110), 30, TextAnchor.MiddleCenter);
            guild.greetingText.fontStyle = FontStyle.Italic;
            guild.statusText = MenuText("Status", guildPage, new Vector2(0, 150), new Vector2(1300, 80), 30, TextAnchor.MiddleCenter);
            guild.statusText.color = new Color(1f, 0.85f, 0.4f);
            Panel("QuestBox", guildPage, new Vector2(0, -30), new Vector2(1100, 220), new Color(0.16f, 0.15f, 0.2f, 1f));
            guild.questText = MenuText("Quest", guildPage, new Vector2(0, -30), new Vector2(1040, 200), 32, TextAnchor.MiddleCenter);
            guild.joinButton = MenuButton("Join", guildPage, new Vector2(-260, -230), new Vector2(380, 80), "Beitreten", 30, new Color(0.3f, 0.5f, 0.32f, 1f));
            guild.joinLabel = guild.joinButton.GetComponentInChildren<Text>();
            guild.acceptButton = MenuButton("Accept", guildPage, new Vector2(0, -230), new Vector2(340, 80), "Annehmen", 30, new Color(0.3f, 0.38f, 0.5f, 1f));
            guild.turnInButton = MenuButton("TurnIn", guildPage, new Vector2(260, -230), new Vector2(340, 80), "Abgeben", 30, new Color(0.55f, 0.45f, 0.2f, 1f));

            root.gameObject.SetActive(false);
            return SavePrefab(canvas.gameObject, "TradeWindow");
        }

        // ------------------------------------------------------------ Basis-Szene
        static GameObject BaseProp(Transform parent, string name, string file, Vector2 position, Vector2 boxSize, Vector2 boxOffset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BaseSprite(file);
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            if (boxSize != Vector2.zero)
            {
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                var col = go.AddComponent<BoxCollider2D>();
                col.size = boxSize;
                col.offset = boxOffset;
            }
            return go;
        }

        static GameObject AddNpc(Transform parent, string name, NpcKind kind, string displayName, ShopData shop, Vector2 position)
        {
            var go = new GameObject("Npc_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.offset = new Vector2(0f, 0.12f);
            var npc = go.AddComponent<Npc>();
            npc.kind = kind;
            npc.displayName = displayName;
            npc.shop = shop;
            npc.range = 1.5f;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sprite = BaseSprite($"npc_{name}_0");
            var loop = visual.AddComponent<SpriteLoop>();
            loop.frames = new[] { BaseSprite($"npc_{name}_0"), BaseSprite($"npc_{name}_1"), BaseSprite($"npc_{name}_2"), BaseSprite($"npc_{name}_3") };
            loop.fps = 4f;
            AddShadow(go.transform, 0.55f);
            return go;
        }

        /// <summary>
        /// Basis (16 x 12 Kacheln, friedlich): Haus oben links, Portal oben rechts, Haendler am Stand und Schmied am Amboss
        /// am Weg (Kachelzeile -1), Spieler startet in der Mitte. Spawn "Start" (Spielstart, nach dem Tod), "Portal" (Rueckkehr).
        /// </summary>
        static void BuildBaseScene(TileSet tiles, GameObject player, GameObject hud, GameObject mobile, GameObject menu,
            GameObject trade, GameObject portal, ShopData shop)
        {
            // Zweite Szene im Durchlauf: Mauer notfalls als feste Boxen (siehe NewWorldScene)
            var (scene, camGo) = NewWorldScene(tiles, 16, 12, -1, requireTilemapCollider: false);
            new GameObject("SceneRules").AddComponent<SceneRules>().peaceful = true;

            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(player);
            playerInstance.transform.position = new Vector3(0f, -2f, 0f);
            camGo.GetComponent<Core.CameraFollow>().target = playerInstance.transform;
            AddSpawn("Start", new Vector2(0f, -2f));
            AddSpawn("Portal", new Vector2(4.6f, 0.9f));

            var world = new GameObject("BaseWorld").transform;
            BaseProp(world, "House", "house", new Vector2(-4.2f, 1.6f), new Vector2(1.3f, 0.6f), new Vector2(0f, 0.3f));
            BaseProp(world, "Stall", "stall", new Vector2(-1.4f, -3.3f), new Vector2(0.7f, 0.25f), new Vector2(0f, 0.1f));
            BaseProp(world, "Anvil", "anvil", new Vector2(2.6f, -3.4f), new Vector2(0.32f, 0.16f), new Vector2(0f, 0.08f));
            BaseProp(world, "Barrel", "barrel", new Vector2(-6.3f, -1.9f), new Vector2(0.24f, 0.16f), new Vector2(0f, 0.08f));
            BaseProp(world, "Crate", "crate", new Vector2(-5.7f, -2.4f), new Vector2(0.26f, 0.16f), new Vector2(0f, 0.08f));
            BaseProp(world, "Crate_2", "crate", new Vector2(3.9f, -3.6f), new Vector2(0.26f, 0.16f), new Vector2(0f, 0.08f));
            Prop(world, "Tree_Pine", "tree_pine", new Vector2(6.4f, -3.4f), 0.25f);
            Prop(world, "Tree_Oak", "tree_oak", new Vector2(-6.4f, 3.6f), 0.3f);
            Prop(world, "Bush", "bush", new Vector2(-1.8f, 3.9f), 0.25f);
            Vector2[] grass = { new(1.2f, 2.6f), new(-2.6f, -0.3f), new(5.6f, 3.4f), new(-6.2f, 0.2f) };
            for (int i = 0; i < grass.Length; i++) Prop(world, "Grass_" + i, "grass_tuft", grass[i]);
            foreach (var (name, pos) in new[] { ("Torch_L", new Vector2(2.9f, 2.6f)), ("Torch_R", new Vector2(6.1f, 2.6f)) })
            {
                var torch = Prop(world, name, "torch_0", pos);
                var loop = torch.AddComponent<SpriteLoop>();
                loop.frames = new[] { PropSprite("torch_0"), PropSprite("torch_1") };
                loop.fps = 6f;
                PointLight("Glow", torch.transform, new Color(1f, 0.6f, 0.25f), 0.9f, 2.2f).transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            var npcs = new GameObject("Npcs").transform;
            AddNpc(npcs, "haendler", NpcKind.Haendler, "Händler", shop, new Vector2(-1.4f, -2.75f));
            AddNpc(npcs, "schmied", NpcKind.Schmied, "Schmied", null, new Vector2(3.3f, -3.0f));

            var gate = (GameObject)PrefabUtility.InstantiatePrefab(portal);
            gate.name = "Portal_Kampf";
            gate.transform.position = new Vector3(4.5f, 2.2f, 0f);
            var gatePortal = gate.GetComponent<Portal>();
            gatePortal.targetScene = SceneTravel.FightScene;
            gatePortal.targetSpawn = "Ankunft";
            PrefabUtility.RecordPrefabInstancePropertyModifications(gatePortal);

            AddCommonUi(hud, mobile, menu, trade);
            EditorSceneManager.SaveScene(scene, BaseScenePath);
        }
    }
}
