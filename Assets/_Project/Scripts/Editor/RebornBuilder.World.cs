using Reborn.Animation;
using Reborn.Magic;
using UnityEditor;
using UnityEngine;

namespace Reborn.EditorTools
{
    /// <summary>Meilenstein 5: Deko im Testraum (Blender-Gegenstaende) und Schaufenster mit Wolf, Waldwaechter und Spinne.</summary>
    public static partial class RebornBuilder
    {
        static Sprite PropSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesProps}/{file}.png");

        /// <summary>Gegenstand mit Fusspunkt-Sortierung; collider &gt; 0 = kleiner fester Kreis am Fuss (Stamm, Stein).</summary>
        static GameObject Prop(Transform parent, string name, string file, Vector2 position, float collider = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PropSprite(file);
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            if (collider > 0f)
            {
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = collider;
                col.offset = new Vector2(0f, collider * 0.5f);
            }
            return go;
        }

        /// <summary>
        /// Deko liegt abseits der Laufwege der Tests (Mitte, Spalte x = 0 nach unten, Zeile y = 2 an den Seitenmauern,
        /// Spalte x = 2 nach oben/unten, rund um die Truhen). Nur Baeume, Fels und Buesche haben einen kleinen Collider.
        /// </summary>
        static void BuildDecoration((SpriteAnimationSet set, float shadow)[] showcase)
        {
            var deco = new GameObject("Decoration").transform;
            Prop(deco, "Tree_Oak", "tree_oak", new Vector2(-7.5f, -3.8f), 0.3f);
            Prop(deco, "Tree_Pine", "tree_pine", new Vector2(7.6f, -3.6f), 0.25f);
            Prop(deco, "Rock", "rock", new Vector2(-4f, -4.2f), 0.3f);
            Prop(deco, "Bush_1", "bush", new Vector2(-8.2f, 4.1f), 0.25f);
            Prop(deco, "Bush_2", "bush", new Vector2(8.2f, -0.6f), 0.25f);
            Vector2[] grass = { new(-2.5f, -3.2f), new(3f, 1.5f), new(-5f, 1f), new(6f, 0.5f), new(1.5f, -4.4f), new(-6.5f, -0.6f), new(5f, 2.6f) };
            for (int i = 0; i < grass.Length; i++) Prop(deco, "Grass_" + i, "grass_tuft", grass[i]);

            var fire = new Color(1f, 0.6f, 0.25f);
            foreach (var (name, pos) in new[] { ("Torch_L", new Vector2(-4.4f, 4.3f)), ("Torch_R", new Vector2(4.6f, 4.3f)) })
            {
                var torch = Prop(deco, name, "torch_0", pos);
                var loop = torch.AddComponent<SpriteLoop>();
                loop.frames = new[] { PropSprite("torch_0"), PropSprite("torch_1") };
                loop.fps = 6f;
                PointLight("Glow", torch.transform, fire, 0.9f, 2.2f).transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            // Schaufenster: neue Gegner-Grafik (Kampf-Logik folgt spaeter), spielt nur die Warte-Animation
            var show = new GameObject("Showcase").transform;
            Vector2[] places = { new(-2.6f, 3.7f), new(0f, 3.9f), new(3.4f, 3.7f) };
            for (int i = 0; i < showcase.Length && i < places.Length; i++)
            {
                var (set, shadow) = showcase[i];
                var go = new GameObject("Showcase_" + set.name.Replace("_Anim", ""));
                go.transform.SetParent(show, false);
                go.transform.position = places[i];
                var visual = new GameObject("Visual");
                visual.transform.SetParent(go.transform, false);
                var sr = visual.AddComponent<SpriteRenderer>();
                sr.spriteSortPoint = SpriteSortPoint.Pivot;
                var anim = visual.AddComponent<DirectionalSpriteAnimator>();
                anim.animationSet = set;
                anim.startClip = "idle";
                if (set.clips.Length > 0 && set.clips[0].down.Length > 0) sr.sprite = set.clips[0].down[0];
                AddShadow(go.transform, shadow);
            }
        }
    }
}
