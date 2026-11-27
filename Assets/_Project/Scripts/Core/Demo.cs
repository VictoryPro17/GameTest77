using UnityEngine;

namespace Reborn.Core
{
    /// <summary>
    /// Demo-Fassung (Meilenstein 10c) fuer die Web-Demo auf okhomina.dev: gebaut mit dem Kennzeichen REBORN_DEMO.
    /// Stadt, Hoehle, Wald und Golem-Boss sind spielbar; das Siegel zum Frostgipfel bleibt zu ("in der Vollversion").
    /// </summary>
    public static class Demo
    {
#if REBORN_DEMO
        const bool BuiltAsDemo = true;
#else
        const bool BuiltAsDemo = false;
#endif

        /// <summary>Demo aktiv (Tests koennen das im Editor umschalten)</summary>
        public static bool Active { get; set; } = BuiltAsDemo;

        public const string Label = "Demo";
        public const string LockedText = "Frostgipfel – nur in der Vollversion";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = BuiltAsDemo;
    }
}
