using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.UI
{
    /// <summary>Zeigt die Touch-Steuerung nur auf Geraeten mit Touchscreen.</summary>
    public class MobileControlsToggle : MonoBehaviour
    {
        public GameObject controlsRoot;
        public bool forceShow;

        void Start()
        {
            bool show = forceShow || Application.isMobilePlatform || Touchscreen.current != null;
            if (controlsRoot != null) controlsRoot.SetActive(show);
        }
    }
}
