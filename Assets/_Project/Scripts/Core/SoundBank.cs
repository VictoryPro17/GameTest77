using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Core
{
    /// <summary>
    /// Erzeugt alle Soundeffekte beim ersten Gebrauch im Code (Meilenstein 10b): kurze Toene, Rauschen und Huellkurven,
    /// keine Audiodateien. 22050 Hz, mono. Laeuft auch im Browser (WebGL).
    /// </summary>
    public static class SoundBank
    {
        public const int Rate = 22050;

        static readonly Dictionary<SfxId, AudioClip> Clips = new Dictionary<SfxId, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Clips.Clear();

        public static int Count => Clips.Count;

        public static AudioClip Get(SfxId id)
        {
            if (Clips.TryGetValue(id, out var clip) && clip != null) return clip;
            float[] data = Make(id);
            clip = AudioClip.Create("sfx_" + id, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            Clips[id] = clip;
            return clip;
        }

        static float[] Make(SfxId id)
        {
            var rng = new System.Random((int)id * 7919 + 13);
            switch (id)
            {
                case SfxId.Swing:   // Luftzug: gefiltertes Rauschen, schnell an und ab
                    return Noise(0.16f, rng, 0.55f, t => Env(t, 0.02f, 0.14f), cutoff: t => 0.35f - 0.25f * t);
                case SfxId.Hit:     // dumpfer Schlag
                    return Mix(Tone(0.14f, t => 180f - 110f * t, Wave.Sine, t => Env(t, 0.003f, 0.13f), 0.8f),
                               Noise(0.08f, rng, 0.45f, t => Env(t, 0.002f, 0.07f), cutoff: t => 0.5f));
                case SfxId.Hurt:    // Spieler getroffen: tiefer, rauer
                    return Mix(Tone(0.22f, t => 220f - 120f * t, Wave.Square, t => Env(t, 0.004f, 0.2f), 0.32f),
                               Noise(0.12f, rng, 0.35f, t => Env(t, 0.002f, 0.1f), cutoff: t => 0.3f));
                case SfxId.Dodge:   // Wusch nach oben
                    return Noise(0.2f, rng, 0.45f, t => Env(t, 0.05f, 0.15f), cutoff: t => 0.1f + 0.4f * t);
                case SfxId.Cast:    // Funkeln: steigender Ton mit Vibrato
                    return Mix(Tone(0.35f, t => 520f + 480f * t + 25f * Mathf.Sin(t * 60f), Wave.Triangle, t => Env(t, 0.02f, 0.32f), 0.45f),
                               Tone(0.35f, t => 780f + 720f * t, Wave.Sine, t => Env(t, 0.05f, 0.28f), 0.2f));
                case SfxId.Pickup:  // kurzes Ploppen
                    return Tone(0.1f, t => 600f + 500f * t, Wave.Sine, t => Env(t, 0.004f, 0.09f), 0.6f);
                case SfxId.Coin:    // zwei helle Toene
                    return Concat(Tone(0.06f, t => 1320f, Wave.Square, t => Env(t, 0.002f, 0.055f), 0.22f),
                                  Tone(0.16f, t => 1760f, Wave.Square, t => Env(t, 0.002f, 0.15f), 0.22f));
                case SfxId.Click:   // Knopf
                    return Tone(0.04f, t => 900f, Wave.Triangle, t => Env(t, 0.001f, 0.035f), 0.4f);
                case SfxId.Step:    // Einfuehrung: Schritt geschafft
                    return Concat(Tone(0.09f, t => 660f, Wave.Triangle, t => Env(t, 0.004f, 0.08f), 0.4f),
                                  Tone(0.18f, t => 990f, Wave.Triangle, t => Env(t, 0.004f, 0.17f), 0.4f));
                case SfxId.LevelUp: // kleine Fanfare
                    return Concat(Tone(0.1f, t => 523f, Wave.Square, t => Env(t, 0.004f, 0.09f), 0.22f),
                                  Tone(0.1f, t => 659f, Wave.Square, t => Env(t, 0.004f, 0.09f), 0.22f),
                                  Tone(0.1f, t => 784f, Wave.Square, t => Env(t, 0.004f, 0.09f), 0.22f),
                                  Tone(0.3f, t => 1047f, Wave.Square, t => Env(t, 0.004f, 0.28f), 0.22f));
                case SfxId.Roar:    // Boss bruellt: tiefes, raues Grollen
                    return Mix(Tone(1.1f, t => 95f - 40f * t + 6f * Mathf.Sin(t * 40f), Wave.Saw, t => Env(t, 0.12f, 0.9f), 0.35f),
                               Noise(1.1f, rng, 0.4f, t => Env(t, 0.15f, 0.9f), cutoff: t => 0.08f));
                case SfxId.Portal:  // Schwirren nach oben
                    return Tone(0.55f, t => 200f + 700f * t * t + 30f * Mathf.Sin(t * 90f), Wave.Sine, t => Env(t, 0.08f, 0.45f), 0.5f);
                case SfxId.Death:   // fallender Ton
                    return Tone(0.7f, t => 330f - 230f * t, Wave.Triangle, t => Env(t, 0.01f, 0.68f), 0.5f);
                case SfxId.Potion:  // Gluckern
                    return Tone(0.3f, t => 300f + 160f * Mathf.Abs(Mathf.Sin(t * 30f)), Wave.Sine, t => Env(t, 0.01f, 0.28f), 0.5f);
                case SfxId.Defeat:  // Gegner besiegt
                    return Mix(Tone(0.25f, t => 300f - 180f * t, Wave.Square, t => Env(t, 0.003f, 0.24f), 0.2f),
                               Noise(0.2f, rng, 0.3f, t => Env(t, 0.003f, 0.18f), cutoff: t => 0.25f));
                default:
                    return new float[Rate / 20];
            }
        }

        // ------------------------------------------------------------ Bausteine

        enum Wave { Sine, Square, Triangle, Saw }

        /// <summary>Huellkurve: Anstieg in attack s, dann Abfall bis release s (Zeit t in Sekunden)</summary>
        static float Env(float t, float attack, float release)
        {
            if (t < attack) return t / attack;
            float r = (t - attack) / Mathf.Max(0.001f, release - attack);
            return Mathf.Clamp01(1f - r) * Mathf.Clamp01(1f - r);
        }

        static float[] Tone(float seconds, Func<float, float> freq, Wave wave, Func<float, float> env, float gain)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var d = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                phase += freq(t / seconds) / Rate;
                float p = (float)(phase - Math.Floor(phase));
                float s;
                switch (wave)
                {
                    case Wave.Square: s = p < 0.5f ? 1f : -1f; break;
                    case Wave.Triangle: s = 4f * Mathf.Abs(p - 0.5f) - 1f; break;
                    case Wave.Saw: s = 2f * p - 1f; break;
                    default: s = Mathf.Sin(p * Mathf.PI * 2f); break;
                }
                d[i] = s * env(t) * gain;
            }
            return d;
        }

        /// <summary>Rauschen durch einen einfachen Tiefpass (cutoff 0..1 je Zeitanteil)</summary>
        static float[] Noise(float seconds, System.Random rng, float gain, Func<float, float> env, Func<float, float> cutoff)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var d = new float[n];
            float y = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float x = (float)(rng.NextDouble() * 2.0 - 1.0);
                y += Mathf.Clamp01(cutoff(t / seconds)) * (x - y);
                d[i] = y * env(t) * gain * 2f;
            }
            return d;
        }

        static float[] Mix(params float[][] parts)
        {
            int n = 0;
            foreach (var p in parts) n = Mathf.Max(n, p.Length);
            var d = new float[n];
            foreach (var p in parts) for (int i = 0; i < p.Length; i++) d[i] += p[i];
            for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(d[i], -1f, 1f);
            return d;
        }

        static float[] Concat(params float[][] parts)
        {
            int n = 0;
            foreach (var p in parts) n += p.Length;
            var d = new float[n];
            int o = 0;
            foreach (var p in parts) { Array.Copy(p, 0, d, o, p.Length); o += p.Length; }
            return d;
        }
    }
}
