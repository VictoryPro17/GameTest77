using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.UI
{
    /// <summary>
    /// Zeigt die Touch-Steuerung nur auf Geraeten mit Touchscreen.
    /// Meilenstein 6: In friedlichen Szenen (Basis) sind Kampf- und Zauberknoepfe aus;
    /// der Knopf "Sprechen" erscheint nur in der Naehe eines NPC.
    /// </summary>
    public class MobileControlsToggle : MonoBehaviour
    {
        public GameObject controlsRoot;
        public bool forceShow;
        /// <summary>Angriff, Zauber, Meditieren (aus in der Basis)</summary>
        public GameObject[] combatButtons = new GameObject[0];
        public GameObject talkButton;

        void OnEnable() => Core.GameSettings.OnChanged += ApplyVisibility;
        void OnDisable() => Core.GameSettings.OnChanged -= ApplyVisibility;

        /// <summary>Meilenstein 10a: Einstellung "Handy-Knoepfe" (automatisch / an / aus)</summary>
        void ApplyVisibility()
        {
            bool auto = forceShow || Application.isMobilePlatform || Touchscreen.current != null;
            if (controlsRoot != null) controlsRoot.SetActive(Core.GameSettings.ShowMobileControls(auto));
        }

        void Start()
        {
            ApplyVisibility();
            bool fight = !World.SceneRules.Peaceful;
            foreach (var b in combatButtons)
                if (b != null) b.SetActive(fight);
            if (talkButton != null) talkButton.SetActive(false);
        }

        void Update()
        {
            if (talkButton == null) return;
            bool near = TradeWindow.NearbyNpc != null && !TradeWindow.IsOpen;
            if (talkButton.activeSelf != near) talkButton.SetActive(near);
        }
    }
}
