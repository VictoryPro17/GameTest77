using Reborn.Combat;
using UnityEngine;

namespace Reborn.Enemies
{
    /// <summary>Was die grosse Boss-Lebensleiste anzeigt (Meilenstein 9a: Golem und Eisdrache)</summary>
    public interface IBossBarSource
    {
        Health Health { get; }
        /// <summary>Name mit Zusatz (Stufe, Phase)</summary>
        string BarName { get; }
        bool ShowUltimateBar { get; }
        float UltimateProgress { get; }
        bool UltimateCharging { get; }
        string WarningText { get; }
    }

    /// <summary>Hilfe fuer die Lebensleiste: der gerade wache Boss (Golem oder Drache), sonst null</summary>
    public static class ActiveBoss
    {
        public static IBossBarSource Current
        {
            get
            {
                if (BossAI.Active != null) return BossAI.Active;
                if (DragonAI.Active != null) return DragonAI.Active;
                return null;
            }
        }
    }
}
