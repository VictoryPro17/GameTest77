using System.Collections.Generic;
using UnityEngine;

namespace Reborn.World
{
    public enum NpcKind
    {
        Haendler,
        Schmied
    }

    /// <summary>NPC in der Basis (Meilenstein 6). In Reichweite zeigt das Handelsfenster "E: Sprechen".</summary>
    public class Npc : MonoBehaviour
    {
        public NpcKind kind;
        public string displayName = "Händler";
        public Economy.ShopData shop;
        public float range = 1.5f;

        static readonly List<Npc> All = new List<Npc>();

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        /// <summary>Naechster NPC in Reichweite (null = keiner)</summary>
        public static Npc Nearest(Vector2 position)
        {
            Npc best = null;
            float bestDist = float.MaxValue;
            foreach (var npc in All)
            {
                if (npc == null) continue;
                float d = Vector2.Distance(position, npc.transform.position);
                if (d <= npc.range && d < bestDist)
                {
                    best = npc;
                    bestDist = d;
                }
            }
            return best;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
