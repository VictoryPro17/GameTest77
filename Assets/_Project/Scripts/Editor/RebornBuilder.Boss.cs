using System.Collections.Generic;
using Reborn.Combat;
using Reborn.Enemies;
using Reborn.Loot;
using Reborn.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 8: Golem-Boss "Waldwaechter" im Kampfraum (statt Schaufenster), Wurzelangriff, Lebensleiste.</summary>
    public static partial class RebornBuilder
    {
        /// <summary>Boss-Prefab fuer den Kampfraum (in BuildAll vor BuildScene gesetzt)</summary>
        static GameObject BossPrefab;

        static Sprite BossSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesBoss}/{file}.png");

        static GameObject CreateRootSpikePrefab()
        {
            var go = new GameObject("RootSpike");
            var spike = go.AddComponent<RootSpike>();
            var warn = new GameObject("Warning");
            warn.transform.SetParent(go.transform, false);
            spike.warning = warn.AddComponent<SpriteRenderer>();
            spike.warning.sprite = BossSprite("warning_circle");
            spike.warning.sortingOrder = -4;   // flach auf dem Boden, unter allen Figuren
            spike.warning.color = new Color(1f, 1f, 1f, 0.85f);
            var sp = new GameObject("Spike");
            sp.transform.SetParent(go.transform, false);
            spike.spike = sp.AddComponent<SpriteRenderer>();
            spike.spike.spriteSortPoint = SpriteSortPoint.Pivot;
            spike.spikeFrames = new[] { BossSprite("root_spike_0"), BossSprite("root_spike_1"), BossSprite("root_spike_2") };
            spike.spike.sprite = spike.spikeFrames[0];
            return SavePrefab(go, "RootSpike");
        }

        static GameObject CreateBossPrefab(Animation.SpriteAnimationSet anim, LootTable loot, GameObject pickup, GameObject goldPickup,
            GameObject wolf, GameObject rootSpike)
        {
            var go = new GameObject("Golem") { layer = LayerEnemy };
            TopDownBody(go, 40f);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.55f;
            col.offset = new Vector2(0f, 0.25f);
            go.AddComponent<Health>().maxHealth = 1200;
            var ai = go.AddComponent<BossAI>();
            ai.wakeRadius = 3.2f;
            ai.wolfPrefab = wolf;
            ai.rootSpikePrefab = rootSpike.GetComponent<RootSpike>();
            var dropper = go.AddComponent<LootDropper>();
            dropper.table = loot;
            dropper.pickupPrefab = pickup.GetComponent<Pickup>();
            dropper.goldPickupPrefab = goldPickup.GetComponent<Pickup>();
            dropper.scatterRadius = 0.8f;
            Visual(go.transform, anim);
            AddShadow(go.transform, 1.6f);
            return SavePrefab(go, "Golem");
        }

        // ------------------------------------------------------------ Meilenstein 8b: Stufe 2
        const string SpritesBoss2 = Root + "/Sprites/Boss2";
        static Sprite Boss2Sprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesBoss2}/{file}.png");

        static SpriteRenderer FxChild(Transform parent, string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>Stufe-2-Grafik und Effekte des Golems: setzt sie am Boss-Prefab (nach CreateBossPrefab aufrufen)</summary>
        static void AddBossStage2(GameObject bossPrefab, Dictionary<string, ItemData> items)
        {
            ConfigureSprites(SpritesBoss2, FootPivot, SpritePpu);
            // Effekte: Ursprung im Bild laut build_boss2_fx.py (Ring/Kreis mittig, Linie beginnt links)
            foreach (var (f, pivot) in new[] { ("thorn_ring_0", new Vector2(0.5f, 0.4747f)), ("thorn_ring_1", new Vector2(0.5f, 0.4747f)),
                                               ("thorn_ring_2", new Vector2(0.5f, 0.4747f)), ("ultimate_circle", new Vector2(0.5f, 0.5f)),
                                               ("charge_line", new Vector2(0.0476f, 0.5f)) })
                ConfigureTexture($"{SpritesBoss2}/{f}.png", SpritePpu, pivot);
            var set = CreateAnimationSet("Boss2_Anim", SpritesBoss2, BossClips);

            var thornGo = new GameObject("ThornWave");
            var thorn = thornGo.AddComponent<ThornWave>();
            thorn.ring = FxChild(thornGo.transform, "Ring", Boss2Sprite("thorn_ring_0"), -4);
            thorn.frames = new[] { Boss2Sprite("thorn_ring_0"), Boss2Sprite("thorn_ring_1"), Boss2Sprite("thorn_ring_2") };
            var thornPrefab = SavePrefab(thornGo, "ThornWave");

            var chargeGo = new GameObject("ChargeTelegraph");
            var charge = chargeGo.AddComponent<ChargeTelegraph>();
            charge.line = FxChild(chargeGo.transform, "Line", Boss2Sprite("charge_line"), -4);
            var chargePrefab = SavePrefab(chargeGo, "ChargeTelegraph");

            var ultGo = new GameObject("UltimateZone");
            var ult = ultGo.AddComponent<UltimateZone>();
            ult.circle = FxChild(ultGo.transform, "Circle", Boss2Sprite("ultimate_circle"), -5);
            var ultPrefab = SavePrefab(ultGo, "UltimateZone");

            var repeat = CreateLoot("Boss_Loot_Repeat", items, new[] { ("heiltrank", 1f, 2, 2), ("manatrank", 1f, 1, 1) }, 300, 300);

            var root = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(bossPrefab));
            var ai = root.GetComponent<BossAI>();
            ai.stage2Set = set;
            ai.thornWavePrefab = thornPrefab.GetComponent<ThornWave>();
            ai.chargeTelegraphPrefab = chargePrefab.GetComponent<ChargeTelegraph>();
            ai.ultimatePrefab = ultPrefab.GetComponent<UltimateZone>();
            ai.repeatLoot = repeat;
            PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GetAssetPath(bossPrefab));
            PrefabUtility.UnloadPrefabContents(root);
        }

        /// <summary>Golem oben in der Mitte des Kampfraums (wacht in 3,2 Einheiten Naehe auf)</summary>
        static void PlaceBoss()
        {
            if (BossPrefab == null) return;
            var boss = (GameObject)PrefabUtility.InstantiatePrefab(BossPrefab);
            boss.name = "Boss_Waldwaechter";
            boss.transform.position = new Vector3(0f, 3.9f, 0f);
        }

        /// <summary>Lebensleiste oben in der Mitte (im HUD-Stil), anfangs aus</summary>
        static void AddBossBar(Canvas canvas)
        {
            var bar = canvas.gameObject.AddComponent<BossBar>();
            var root = Rect("BossBar", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 110f));
            bar.root = root.gameObject;
            // Meilenstein 8b: hoeheres Textfeld und Overflow - vorher war es fuer die Schrift zu niedrig und Unity blendete den Namen aus
            bar.nameText = Label("BossName", root, new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(900f, 48f), 30, TextAnchor.MiddleCenter);
            bar.nameText.verticalOverflow = VerticalWrapMode.Overflow;
            bar.nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            bar.nameText.font = T.headingFont;
            bar.nameText.color = T.gold;
            bar.nameText.text = "Waldwächter";
            bar.fill = ThemedBar("BossHealth", root, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(860f, 36f), new Color(0.75f, 0.15f, 0.12f), out var label, 1);
            label.text = "";
            // Lade-Leiste der Ultimate (nur Stufe 2)
            var ultRoot = Rect("UltimateBar", root, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(500f, 20f));
            bar.ultimateRoot = ultRoot.gameObject;
            bar.ultimateFill = ThemedBar("UltimateCharge", ultRoot, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 20f), new Color(1f, 0.55f, 0.15f), out var ultLabel, 1);
            ultLabel.text = "";
            ultRoot.gameObject.SetActive(false);
            root.gameObject.SetActive(false);
            // Warnhinweis waehrend der Ultimate-Aufladung (Bildmitte oben)
            bar.warningText = Label("UltimateWarning", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1400f, 70f), 48, TextAnchor.MiddleCenter);
            bar.warningText.verticalOverflow = VerticalWrapMode.Overflow;
            bar.warningText.horizontalOverflow = HorizontalWrapMode.Overflow;
            bar.warningText.font = T.headingFont;
            bar.warningText.color = new Color(1f, 0.35f, 0.2f);
            bar.warningText.text = "";
            bar.warningText.gameObject.SetActive(false);
        }
    }
}
