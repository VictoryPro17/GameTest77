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
            var root = Rect("BossBar", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 80f));
            bar.root = root.gameObject;
            bar.nameText = Label("BossName", root, new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(900f, 36f), 30, TextAnchor.MiddleCenter);
            bar.nameText.font = T.headingFont;
            bar.nameText.color = T.gold;
            bar.nameText.text = "Waldwächter";
            bar.fill = ThemedBar("BossHealth", root, new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(860f, 36f), new Color(0.75f, 0.15f, 0.12f), out var label, 1);
            label.text = "";
            root.gameObject.SetActive(false);
        }
    }
}
