using System.IO;
using Reborn.Animation;
using Reborn.Loot;
using Reborn.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.EditorTools
{
    /// <summary>
    /// Meilenstein 10b: Mentor Aldric (Einfuehrung) in der Stadt. Aussehen wie eine Stadtfigur (Sprites/Menu/Mentor),
    /// aber ohne Npc-Komponente (kein Handelsfenster) und ohne festen Collider - er begleitet den Spieler per TutorialGuide.
    /// </summary>
    public static partial class RebornBuilder
    {
        const string SpritesMentor = SpritesMenu + "/Mentor";

        /// <summary>Mentor-Sprites wie die Stadtfiguren: 128 px pro Einheit, Fusspunkt-Pivot, Point-Filter.</summary>
        static void ConfigureMentorSprites()
        {
            if (!Directory.Exists(SpritesMentor)) return;
            ConfigureSprites(SpritesMentor, FootPivot, SpritePpu);
        }

        static GameObject AddMentor(Transform parent, InputActionAsset actions, ItemData rewardPotion, Vector2 position)
        {
            ConfigureMentorSprites();
            var set = CreateAnimationSet("Mentor_Anim", SpritesMentor, VillagerClips);
            var arrow = LoadMenuSprite("target_arrow.png");

            var go = new GameObject("Mentor_Aldric");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            var anim = visual.AddComponent<DirectionalSpriteAnimator>();
            anim.animationSet = set;
            anim.startClip = "idle";
            if (set.clips.Length > 0 && set.clips[0].down.Length > 0) sr.sprite = set.clips[0].down[0];
            AddShadow(go.transform, 0.5f);

            var bubble = go.AddComponent<SpeechBubble>();
            bubble.height = 1.2f;
            bubble.duration = 4.5f;
            var guide = go.AddComponent<TutorialGuide>();
            guide.actions = actions;
            guide.rewardPotion = rewardPotion;
            guide.arrowSprite = arrow;
            guide.bubble = bubble;
            return go;
        }
    }
}
