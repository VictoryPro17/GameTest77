using System;
using UnityEngine;

namespace Reborn.Animation
{
    /// <summary>Spielt Sprite-Animationen passend zur Blickrichtung ab.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalSpriteAnimator : MonoBehaviour
    {
        public SpriteAnimationSet animationSet;
        public string startClip = "idle";

        public event Action<string> OnClipFinished;
        /// <summary>Neues Bild: Animation, Richtung, Bildnummer (fuer Rüstungsebenen)</summary>
        public event Action<string, Facing, int> OnFrameChanged;

        public Facing Facing { get; private set; } = Facing.Down;
        public string CurrentClip => _clip != null ? _clip.name : null;
        /// <summary>Gerade gezeigte Bildnummer</summary>
        public int FrameIndex { get; private set; }

        SpriteRenderer _renderer;
        SpriteAnimationSet.Clip _clip;
        float _time;
        bool _finished;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            Play(startClip);
        }

        public void Play(string clipName, bool restart = false)
        {
            if (animationSet == null) return;
            if (!restart && _clip != null && _clip.name == clipName) return;
            var clip = animationSet.Find(clipName);
            if (clip == null) return;
            _clip = clip;
            _time = 0f;
            _finished = false;
            ShowFrame(0);
        }

        public void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            Facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? (direction.x > 0f ? Facing.Right : Facing.Left)
                : (direction.y > 0f ? Facing.Up : Facing.Down);
            ShowFrame(CurrentFrameIndex());
        }

        void Update()
        {
            if (_clip == null || _finished) return;
            _time += Time.deltaTime;
            var frames = _clip.Get(Facing);
            if (frames == null || frames.Length == 0) return;

            int index = Mathf.FloorToInt(_time * _clip.fps);
            if (!_clip.loop && index >= frames.Length)
            {
                _finished = true;
                ShowFrame(frames.Length - 1);
                OnClipFinished?.Invoke(_clip.name);
                return;
            }
            ShowFrame(index);
        }

        int CurrentFrameIndex()
        {
            if (_clip == null) return 0;
            return Mathf.FloorToInt(_time * _clip.fps);
        }

        void ShowFrame(int index)
        {
            if (_clip == null || _renderer == null) return;
            var frames = _clip.Get(Facing);
            if (frames == null || frames.Length == 0) return;
            index = _clip.loop ? index % frames.Length : Mathf.Clamp(index, 0, frames.Length - 1);
            bool changed = _renderer.sprite != frames[index] || index != FrameIndex;
            _renderer.sprite = frames[index];
            FrameIndex = index;
            if (changed) OnFrameChanged?.Invoke(_clip.name, Facing, index);
        }
    }
}
