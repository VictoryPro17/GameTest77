using Reborn.Core;
namespace Reborn.Loot
{
    /// <summary>Art des Ausruestungsplatzes. Ring gibt es zweimal (Platz 8 und 9).</summary>
    public enum EquipSlot
    {
        None,
        Helm,
        Brust,
        Hose,
        Schuhe,
        Handschuhe,
        Hauptwaffe,
        Nebenhand,
        Halskette,
        Ring
    }

    public static class EquipSlots
    {
        /// <summary>Reihenfolge der 10 Plaetze in PlayerEquipment.</summary>
        public static readonly EquipSlot[] Positions =
        {
            EquipSlot.Helm, EquipSlot.Brust, EquipSlot.Hose, EquipSlot.Schuhe, EquipSlot.Handschuhe,
            EquipSlot.Hauptwaffe, EquipSlot.Nebenhand, EquipSlot.Halskette, EquipSlot.Ring, EquipSlot.Ring
        };

        public static string Label(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Helm: return Loc.T("Helm");
                case EquipSlot.Brust: return Loc.T("Brustplatte");
                case EquipSlot.Hose: return Loc.T("Hose");
                case EquipSlot.Schuhe: return Loc.T("Schuhe");
                case EquipSlot.Handschuhe: return Loc.T("Handschuhe");
                case EquipSlot.Hauptwaffe: return Loc.T("Hauptwaffe");
                case EquipSlot.Nebenhand: return Loc.T("Nebenhand");
                case EquipSlot.Halskette: return Loc.T("Halskette");
                case EquipSlot.Ring: return Loc.T("Ring");
                default: return "";
            }
        }
    }
}
