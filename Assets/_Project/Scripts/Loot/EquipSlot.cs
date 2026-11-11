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
                case EquipSlot.Helm: return "Helm";
                case EquipSlot.Brust: return "Brustplatte";
                case EquipSlot.Hose: return "Hose";
                case EquipSlot.Schuhe: return "Schuhe";
                case EquipSlot.Handschuhe: return "Handschuhe";
                case EquipSlot.Hauptwaffe: return "Hauptwaffe";
                case EquipSlot.Nebenhand: return "Nebenhand";
                case EquipSlot.Halskette: return "Halskette";
                case EquipSlot.Ring: return "Ring";
                default: return "";
            }
        }
    }
}
