using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reborn.World
{
    /// <summary>
    /// Dauerhafte Welt-Schalter (Meilenstein 9a), z. B. "frost_siegel" = Siegel-Portal geoeffnet.
    /// Das SaveSystem schreibt und liest sie (Spielstand Version 7).
    /// </summary>
    public static class WorldFlags
    {
        static readonly HashSet<string> Flags = new HashSet<string>();

        public static event Action OnChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Flags.Clear();

        public static bool Has(string id) => !string.IsNullOrEmpty(id) && Flags.Contains(id);

        public static void Set(string id)
        {
            if (string.IsNullOrEmpty(id) || !Flags.Add(id)) return;
            OnChanged?.Invoke();
        }

        public static IEnumerable<string> All => Flags;

        /// <summary>Alle Schalter ersetzen (Spielstand laden bzw. Neustart)</summary>
        public static void SetAll(IEnumerable<string> ids)
        {
            Flags.Clear();
            if (ids != null)
                foreach (var id in ids)
                    if (!string.IsNullOrEmpty(id)) Flags.Add(id);
            OnChanged?.Invoke();
        }
    }
}
