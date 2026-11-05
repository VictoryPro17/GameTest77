using System;
using UnityEngine;

namespace Reborn.Animation
{
    /// <summary>Alle Animationen einer Figur, jeweils mit Bildern fuer 4 Richtungen.</summary>
    [CreateAssetMenu(menuName = "Reborn/Sprite Animation Set", fileName = "NewAnimationSet")]
    public class SpriteAnimationSet : ScriptableObject
    {
        [Serializable]
        public class Clip
        {
            public string name;
            public float fps = 8f;
            public bool loop = true;
            public Sprite[] down;
            public Sprite[] up;
            public Sprite[] left;
            public Sprite[] right;

            public Sprite[] Get(Facing facing)
            {
                switch (facing)
                {
                    case Facing.Up: return up;
                    case Facing.Left: return left;
                    case Facing.Right: return right;
                    default: return down;
                }
            }
        }

        public Clip[] clips = Array.Empty<Clip>();

        public Clip Find(string clipName)
        {
            foreach (var clip in clips)
            {
                if (clip != null && clip.name == clipName) return clip;
            }
            return null;
        }
    }
}
