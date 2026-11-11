using System.IO;
using Reborn.Magic;
using Reborn.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 3: Zauber-Daten, Effekt-Prefabs (Partikel + Licht), HUD, Menue-Reiter "Magie".</summary>
    public static partial class RebornBuilder
    {
        const string SpritesSpells = Root + "/Sprites/Spells";
        const string SpritesFx = Root + "/Sprites/FX";
        const string DataSpells = Root + "/Data/Spells";
        const int FxOrder = 9;

        public class MagicAssets
        {
            public SpellData[] spells;
            public GameObject castFlash;
            public Material soft, spark, drop;
        }

        // ------------------------------------------------------------ Texturen und Material
        static Texture2D FxTexture(string name, int size, System.Func<float, float, float> alpha)
        {
            var path = WritePng(SpritesFx, name, size, (x, y) =>
            {
                float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(u, v)) * 255f);
                return new Color32(255, 255, 255, a);
            }, size);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material FxMaterial(string name, Texture2D texture)
        {
            Directory.CreateDirectory(DataSpells);
            var path = $"{DataSpells}/Mat_{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.shader = shader;
            mat.mainTexture = texture;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>Partikel-System mit weichem Ausblenden und Schrumpfen.</summary>
        static ParticleSystem Particles(string name, Transform parent, Material mat, Color a, Color b, float life, float speed,
            float size, float rate, int burst, bool world, float radius = 0.05f, bool loop = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = loop ? 1f : 0.5f;
            main.loop = loop;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
            main.simulationSpace = world ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;
            main.maxParticles = 300;
            main.playOnAwake = true;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            if (burst > 0) emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mat;
            renderer.sortingOrder = FxOrder + 1;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            return ps;
        }

        static Light2D PointLight(string name, Transform parent, Color color, float intensity, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius = 0f;
            light.pointLightOuterRadius = radius;
            return light;
        }

        static SpriteRenderer FxSprite(string name, Transform parent, Sprite sprite, int order = FxOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        static GameObject SaveFx(GameObject go, string name)
        {
            Directory.CreateDirectory($"{Prefabs}/Magic");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Prefabs}/Magic/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static Sprite SpellSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesSpells}/{file}.png");

        // ------------------------------------------------------------ Alles fuer die Magie
        static MagicAssets CreateMagic()
        {
            ConfigureSprites(SpritesSpells, new Vector2(0.5f, 0.5f));
            var m = new MagicAssets();
            var soft = FxTexture("FX_Soft", 32, (u, v) => 1f - Mathf.Sqrt(u * u + v * v));
            var spark = FxTexture("FX_Spark", 32, (u, v) => 1f - (Mathf.Abs(u) + Mathf.Abs(v)) * 1.1f);
            var drop = FxTexture("FX_Drop", 32, (u, v) => 1f - Mathf.Sqrt(u * u * 1.6f + (v + 0.2f) * (v + 0.2f)) * 1.3f);
            FxTexture("FX_Ring", 128, (u, v) => 1f - Mathf.Abs(Mathf.Sqrt(u * u + v * v) - 0.85f) * 9f);
            m.soft = FxMaterial("Soft", soft);
            m.spark = FxMaterial("Spark", spark);
            m.drop = FxMaterial("Drop", drop);
            var ringPath = $"{SpritesFx}/FX_Ring.png";
            ConfigureTexture(ringPath, 64, new Vector2(0.5f, 0.5f));
            var ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ringPath);

            Color water = new Color(0.4f, 0.8f, 1f), waterLight = new Color(0.85f, 0.97f, 1f);
            Color fire = new Color(1f, 0.55f, 0.1f), fireLight = new Color(1f, 0.9f, 0.4f), fireDark = new Color(0.9f, 0.2f, 0.05f);
            Color gold = new Color(1f, 0.85f, 0.25f);

            // ---- Treffer-Effekte
            var hitWaterGo = new GameObject("FX_Hit_Water");
            Particles("Splash", hitWaterGo.transform, m.drop, water, waterLight, 0.45f, 4f, 0.18f, 0f, 18, true, 0.1f, false);
            var hitWater = SaveFx(hitWaterGo, "FX_Hit_Water");

            var explosionGo = new GameObject("FX_Explosion");
            Particles("Sparks", explosionGo.transform, m.spark, fire, fireLight, 0.5f, 5f, 0.25f, 0f, 30, true, 0.15f, false);
            Particles("Smoke", explosionGo.transform, m.soft, fireDark, new Color(0.3f, 0.2f, 0.15f), 0.7f, 1.5f, 0.6f, 0f, 10, true, 0.3f, false);
            PointLight("Flash", explosionGo.transform, fire, 2.5f, 2.2f).gameObject.AddComponent<LightFlash>().duration = 0.35f;
            var explosion = SaveFx(explosionGo, "FX_Explosion");

            var hitFireGo = new GameObject("FX_Hit_Fire");
            Particles("Sparks", hitFireGo.transform, m.spark, fire, fireLight, 0.35f, 3f, 0.16f, 0f, 14, true, 0.1f, false);
            var hitFire = SaveFx(hitFireGo, "FX_Hit_Fire");

            var flashGo = new GameObject("FX_CastFlash");
            Particles("Glow", flashGo.transform, m.soft, Color.white, Color.white, 0.3f, 1.5f, 0.35f, 0f, 12, true, 0.1f, false);
            m.castFlash = SaveFx(flashGo, "FX_CastFlash");

            GameObject GoldAura(Transform parent)
            {
                var ps = Particles("GoldAura", parent, m.spark, gold, Color.white, 0.4f, 0.6f, 0.14f, 30f, 0, true, 0.25f);
                ps.gameObject.SetActive(false);
                return ps.gameObject;
            }

            // ---- Wasserkugel
            var wbGo = new GameObject("Spell_Wasserkugel");
            var wb = wbGo.AddComponent<SpellProjectile>();
            wb.visual = FxSprite("Visual", wbGo.transform, SpellSprite("spell_waterball")).transform;
            wb.trails = new[] { Particles("Trail", wbGo.transform, m.drop, water, waterLight, 0.35f, 0.4f, 0.16f, 45f, 0, true, 0.08f) };
            PointLight("Glow", wbGo.transform, water, 0.8f, 1.2f);
            wb.hitEffect = hitWater;
            wb.goldAura = GoldAura(wbGo.transform);
            var waterballPrefab = SaveFx(wbGo, "Spell_Wasserkugel").GetComponent<SpellEffect>();

            // ---- Wasserdrache
            var dGo = new GameObject("Spell_Wasserdrache");
            var dragon = dGo.AddComponent<WaterDragon>();
            var head = FxSprite("Head", dGo.transform, SpellSprite("spell_dragon_head"), FxOrder + 1).transform;
            dragon.head = head;
            dragon.segments = new Transform[8];
            for (int i = 0; i < 8; i++) dragon.segments[i] = FxSprite($"Body_{i}", dGo.transform, SpellSprite("spell_dragon_body"), FxOrder - i % 2).transform;
            var drops = Particles("Drops", head, m.drop, water, waterLight, 0.6f, 1.2f, 0.2f, 70f, 0, true, 0.3f);
            dragon.trails = new[] { drops };
            PointLight("Glow", head, water, 1.2f, 2f);
            dragon.hitEffect = hitWater;
            dragon.goldAura = GoldAura(head);
            var dragonPrefab = SaveFx(dGo, "Spell_Wasserdrache").GetComponent<SpellEffect>();

            // ---- Feuerball
            var fbGo = new GameObject("Spell_Feuerball");
            var fb = fbGo.AddComponent<SpellProjectile>();
            fb.explode = true;
            var fbVisual = FxSprite("Visual", fbGo.transform, SpellSprite("spell_fireball_0"));
            fbVisual.gameObject.AddComponent<SpriteLoop>().frames = new[] { SpellSprite("spell_fireball_0"), SpellSprite("spell_fireball_1") };
            fb.visual = fbVisual.transform;
            fb.trails = new[] { Particles("Embers", fbGo.transform, m.spark, fire, fireLight, 0.4f, 0.6f, 0.15f, 60f, 0, true, 0.1f) };
            PointLight("Glow", fbGo.transform, fire, 1.3f, 1.6f);
            fb.hitEffect = explosion;
            fb.goldAura = GoldAura(fbGo.transform);
            var fireballPrefab = SaveFx(fbGo, "Spell_Feuerball").GetComponent<SpellEffect>();

            // ---- Flammenring
            var rGo = new GameObject("Spell_Flammenring");
            var ring = rGo.AddComponent<FlameRing>();
            var ringVisual = FxSprite("Ring", rGo.transform, ringSprite);
            ringVisual.color = fire;
            ring.ringVisual = ringVisual.transform;
            var flames = Particles("Flames", rGo.transform, m.spark, fire, fireLight, 0.4f, 0.8f, 0.3f, 260f, 0, true, 0.3f);
            var flameShape = flames.shape;
            flameShape.radiusThickness = 0f;
            ring.flames = flames;
            ring.trails = new[] { flames };
            ring.glow = PointLight("Glow", rGo.transform, fire, 2f, 2.5f);
            ring.hitEffect = hitFire;
            ring.goldAura = GoldAura(rGo.transform);
            var ringPrefab = SaveFx(rGo, "Spell_Flammenring").GetComponent<SpellEffect>();

            // ---- Zauber-Daten
            Directory.CreateDirectory(DataSpells);
            SpellData Spell(string id, string name, SpellElement element, SpellKind kind, string icon, string description, SpellEffect prefab,
                int mana, int damage, float cooldown, float speed, float range, float radius, float knock, int xp, bool big,
                string unlockId = "", int unlockLevel = 0, int playerLevel = 1, int splash = 0, float splashRadius = 1f)
            {
                var s = LoadOrCreate<SpellData>($"{DataSpells}/Spell_{name}.asset");
                s.id = id; s.spellName = name; s.element = element; s.kind = kind; s.icon = SpellSprite(icon); s.description = description;
                s.effectPrefab = prefab; s.manaCost = mana; s.baseDamage = damage; s.cooldown = cooldown; s.speed = speed; s.range = range;
                s.radius = radius; s.knockback = knock; s.xpPerHit = xp; s.bigChant = big; s.unlockSpellId = unlockId;
                s.unlockSpellLevel = unlockLevel; s.unlockPlayerLevel = playerLevel; s.splashDamage = splash; s.splashRadius = splashRadius;
                EditorUtility.SetDirty(s);
                return s;
            }
            m.spells = new[]
            {
                Spell("wasserkugel", "Wasserkugel", SpellElement.Wasser, SpellKind.Projectile, "icon_wasserkugel",
                    "Eine Kugel aus Wasser, die beim Treffer zerspritzt.", waterballPrefab, 5, 8, 0.5f, 9f, 6f, 0.3f, 3f, 1, false),
                Spell("wasserdrache", "Wasserdrache", SpellElement.Wasser, SpellKind.Dragon, "icon_wasserdrache",
                    "Ein Drache aus Wasser schlängelt sich durch alle Gegner.", dragonPrefab, 25, 20, 4f, 9f, 8f, 0.5f, 4f, 2, true,
                    "wasserkugel", 3),
                Spell("feuerball", "Feuerball", SpellElement.Feuer, SpellKind.Fireball, "icon_feuerball",
                    "Ein glühender Ball, der beim Aufprall explodiert.", fireballPrefab, 8, 12, 0.8f, 8f, 6f, 0.3f, 3f, 1, false,
                    "", 0, 2, 6, 1f),
                Spell("flammenring", "Flammenring", SpellElement.Feuer, SpellKind.Ring, "icon_flammenring",
                    "Ein Ring aus Feuer breitet sich um dich aus und stößt Gegner weg.", ringPrefab, 20, 15, 3f, 0f, 0f, 1.8f, 6f, 1, true,
                    "feuerball", 3),
            };
            return m;
        }

        /// <summary>Mana, Zauberbuch und Meditations-Aura an den Spieler haengen.</summary>
        static void AddMagicToPlayer(GameObject go, MagicAssets m, InputActionAsset input)
        {
            var aura = Particles("MeditateAura", go.transform, m.soft, new Color(0.4f, 0.75f, 1f, 0.9f), new Color(0.8f, 0.9f, 1f, 0.9f),
                1.1f, 0.8f, 0.18f, 22f, 0, true, 0.35f);
            aura.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            var auraMain = aura.main;
            auraMain.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            var auraShape = aura.shape;
            auraShape.rotation = new Vector3(0f, 0f, 0f);
            var vel = aura.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.y = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var mana = go.AddComponent<PlayerMana>();
            mana.actions = input;
            mana.meditateAura = aura.gameObject;
            var book = go.AddComponent<SpellBook>();
            book.actions = input;
            book.spells = m.spells;
            book.enemyMask = 1 << LayerEnemy;
            book.wallMask = 1 << LayerWalls;
            book.castFlash = m.castFlash;
        }

        /// <summary>Mana-Leiste, 4 Zauber-Plaetze, Ausruf und grosses Banner im HUD.</summary>
        static void AddMagicHud(Canvas canvas, HUD hud, UiSprites ui)
        {
            hud.manaFill = Bar("ManaBar", canvas.transform, new Vector2(30, -98), new Vector2(420, 20), new Color(0.25f, 0.55f, 1f));
            hud.manaText = Label("ManaText", hud.manaFill.transform.parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 20), 16, TextAnchor.MiddleCenter);

            string[] keys = { "1", "2", "3", "4" };
            hud.spellIcons = new Image[SpellBook.SlotCount];
            hud.spellCooldowns = new Image[SpellBook.SlotCount];
            hud.spellLevels = new Text[SpellBook.SlotCount];
            for (int i = 0; i < SpellBook.SlotCount; i++)
            {
                var frame = Rect($"SpellSlot_{i}", canvas.transform, new Vector2(0.5f, 0f), new Vector2(-165 + i * 110, 20), new Vector2(96, 96)).gameObject.AddComponent<Image>();
                frame.sprite = ui.emptyFrame;
                var icon = Rect("Icon", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80)).gameObject.AddComponent<Image>();
                icon.preserveAspect = true;
                hud.spellIcons[i] = icon;
                var cd = Rect("Cooldown", frame.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88, 88)).gameObject.AddComponent<Image>();
                cd.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                cd.color = new Color(0f, 0f, 0f, 0.65f);
                cd.type = Image.Type.Filled;
                cd.fillMethod = Image.FillMethod.Radial360;
                cd.fillOrigin = (int)Image.Origin360.Top;
                cd.fillClockwise = false;
                cd.fillAmount = 0f;
                hud.spellCooldowns[i] = cd;
                Label("Key", frame.transform, new Vector2(0, 1), new Vector2(6, -2), new Vector2(40, 30), 22, TextAnchor.UpperLeft).text = keys[i];
                hud.spellLevels[i] = Label("Level", frame.transform, new Vector2(1, 0), new Vector2(-6, 2), new Vector2(70, 28), 20, TextAnchor.LowerRight);
            }

            var chantText = Label("ChantText", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 50), 30, TextAnchor.MiddleCenter);
            chantText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var chant = canvas.gameObject.AddComponent<ChantText>();
            chant.text = chantText;

            var banner = canvas.gameObject.AddComponent<CastBanner>();
            banner.title = Label("BannerTitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 260), new Vector2(1600, 120), 84, TextAnchor.MiddleCenter);
            banner.title.horizontalOverflow = HorizontalWrapMode.Overflow;
            banner.subtitle = Label("BannerSubtitle", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1600, 50), 32, TextAnchor.MiddleCenter);
            banner.subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        static Image MenuBar(string name, Transform parent, Vector2 pos, Vector2 size, Color fill)
        {
            var bg = Panel(name, parent, pos, size, new Color(0.08f, 0.07f, 0.1f, 1f));
            var img = Rect("Fill", bg.transform, new Vector2(0f, 0.5f), new Vector2(2, 0), size - new Vector2(4, 4)).gameObject.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.color = fill;
            img.fillAmount = 0f;
            return img;
        }

        /// <summary>Dritter Menue-Reiter "Magie".</summary>
        static void AddMagicMenu(GameMenu menu, Transform panel, Color tabColor)
        {
            menu.magicTabButton = MenuButton("TabMagie", panel, new Vector2(-330, 425), new Vector2(200, 62), "Magie", 30, tabColor);
            var page = Stretch("MagicPage", panel);
            menu.magicPage = page.gameObject;
            var view = page.gameObject.AddComponent<MagicView>();
            menu.magicView = view;
            view.icons = new Image[SpellBook.SlotCount];
            view.titles = new Text[SpellBook.SlotCount];
            view.xpFills = new Image[SpellBook.SlotCount];
            view.infos = new Text[SpellBook.SlotCount];
            for (int i = 0; i < SpellBook.SlotCount; i++)
            {
                float y = 290 - i * 150;
                Panel($"Row_{i}", page, new Vector2(0, y), new Vector2(1640, 132), new Color(0.16f, 0.15f, 0.2f, 1f));
                view.icons[i] = Rect($"Icon_{i}", page, new Vector2(0.5f, 0.5f), new Vector2(-740, y), new Vector2(104, 104)).gameObject.AddComponent<Image>();
                view.icons[i].preserveAspect = true;
                view.titles[i] = MenuText($"Title_{i}", page, new Vector2(-60, y + 36), new Vector2(1240, 46), 30, TextAnchor.MiddleLeft);
                view.xpFills[i] = MenuBar($"Xp_{i}", page, new Vector2(-60, y - 2), new Vector2(1240, 16), new Color(0.95f, 0.8f, 0.25f));
                view.infos[i] = MenuText($"Info_{i}", page, new Vector2(-60, y - 38), new Vector2(1240, 34), 22, TextAnchor.MiddleLeft);
            }
            view.manaText = MenuText("Mana", page, new Vector2(0, -330), new Vector2(1640, 76), 25, TextAnchor.MiddleLeft);
            view.manaXpFill = MenuBar("ManaXp", page, new Vector2(0, -385), new Vector2(1640, 18), new Color(0.25f, 0.55f, 1f));
            page.gameObject.SetActive(false);
        }
    }
}
