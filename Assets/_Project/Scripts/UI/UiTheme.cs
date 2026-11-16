using UnityEngine;

namespace Reborn.UI
{
    /// <summary>Aussehen der Oberflaeche: Rahmen, Farben, Schriften (dunkles Fantasy). Wird vom Builder erzeugt.</summary>
    [CreateAssetMenu(menuName = "Reborn/UI Theme", fileName = "UiTheme")]
    public class UiTheme : ScriptableObject
    {
        [Header("Schriften")]
        public Font headingFont;
        public Font bodyFont;
        public Font bodyBoldFont;

        [Header("Rahmen (9-teilig)")]
        public Sprite window;
        public Sprite inset;
        public Sprite button;
        public Sprite tab;
        public Sprite slotEmpty;
        /// <summary>Gewoehnlich, Selten, Episch, Legendaer</summary>
        public Sprite[] slotRarity = new Sprite[4];
        public Sprite selection;
        public Sprite barFrame;
        public Sprite barFill;
        public Sprite portrait;
        public Sprite badge;

        [Header("Farben")]
        public Color text = new Color(0.93f, 0.9f, 0.82f);
        public Color textDim = new Color(0.68f, 0.65f, 0.6f);
        public Color gold = new Color(0.96f, 0.78f, 0.35f);
        public Color health = new Color(0.86f, 0.2f, 0.22f);
        public Color mana = new Color(0.25f, 0.55f, 1f);
        public Color xp = new Color(0.95f, 0.78f, 0.25f);
        public Color buttonTint = Color.white;
        public Color tabInactive = new Color(0.62f, 0.6f, 0.62f);
        public Color danger = new Color(0.75f, 0.28f, 0.25f);
        public Color confirm = new Color(0.45f, 0.75f, 0.45f);
    }
}
