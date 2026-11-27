using Reborn.Core;
using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reborn.World
{
    /// <summary>
    /// Siegel-Portal (Meilenstein 9a): anfangs grau und versiegelt. Mit dem Schluessel im Inventar und "Interact" (E)
    /// in der Naehe wird es geoeffnet (Schluessel verbraucht, Welt-Schalter gespeichert) und fuehrt dann wie ein Portal weiter.
    /// Ohne Schluessel zeigt eine Sprechblase "Versiegelt - ein Schluessel fehlt".
    /// </summary>
    public class SealedPortal : Portal
    {
        public string flagId = "frost_siegel";
        public ItemData keyItem;
        public InputActionAsset actions;
        public float range = 1.6f;
        public GameObject sealedVisual;
        public GameObject openVisual;
        public SpeechBubble bubble;

        public const string PromptText = "E: Siegel öffnen";
        public const string LockedText = "Versiegelt – ein Schlüssel fehlt";
        public const string OpenedText = "Das Siegel bricht!";

        public bool IsOpen => WorldFlags.Has(flagId);
        public override bool CanEnter => IsOpen && !Core.Demo.Active;   // Demo (10c): Frostgipfel gesperrt
        /// <summary>Letzte Meldung (fuer Tests)</summary>
        public string LastMessage { get; private set; } = "";

        InputAction _interact;
        Transform _player;
        PlayerInventory _inventory;
        bool _shownOpen;
        bool _wasNear;

        void Awake()
        {
            if (actions != null) _interact = actions.FindActionMap("Gameplay", true).FindAction("Interact", true);
        }

        void OnEnable()
        {
            if (_interact != null) _interact.actionMap.Enable();
        }

        new void Start()
        {
            base.Start();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                _player = p.transform;
                _inventory = p.GetComponent<PlayerInventory>();
            }
            ApplyVisual();
        }

        void ApplyVisual()
        {
            _shownOpen = IsOpen;
            if (sealedVisual != null) sealedVisual.SetActive(!_shownOpen);
            if (openVisual != null) openVisual.SetActive(_shownOpen);
        }

        void Update()
        {
            if (_shownOpen != IsOpen) ApplyVisual();   // Spielstand wurde nach Start geladen
            if ((IsOpen && !Core.Demo.Active) || _player == null) return;
            bool near = Vector2.Distance(_player.position, transform.position) <= range;
            if (near && !_wasNear) Say(Core.Demo.Active ? Core.Demo.LockedText : PromptText);
            _wasNear = near;
            if (!near || UI.TradeWindow.IsOpen || UI.GameMenu.IsOpen) return;
            if (_interact != null && _interact.WasPressedThisFrame()) TryUnseal();
        }

        void Say(string text)
        {
            LastMessage = text;
            if (bubble != null) bubble.Show(Loc.T(text));
        }

        /// <summary>Siegel mit dem Schluessel oeffnen (auch fuer Tests). true = jetzt offen</summary>
        public bool TryUnseal()
        {
            if (Core.Demo.Active) { Say(Core.Demo.LockedText); return false; }
            if (IsOpen) return true;
            if (keyItem == null || _inventory == null || !_inventory.Remove(keyItem, 1))
            {
                Say(LockedText);
                return false;
            }
            WorldFlags.Set(flagId);
            ApplyVisual();
            Say(OpenedText);
            Core.Sfx.Play(Core.SfxId.Portal);
            var save = _player != null ? _player.GetComponent<Save.SaveSystem>() : null;
            if (save != null) save.Save();
            return true;
        }
    }
}
