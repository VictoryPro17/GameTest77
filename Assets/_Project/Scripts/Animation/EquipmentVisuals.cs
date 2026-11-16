using Reborn.Inventory;
using Reborn.Loot;
using UnityEngine;

namespace Reborn.Animation
{
    /// <summary>
    /// Zeigt die angelegte Ausruestung an der Figur: eine Bildebene pro Platz ueber dem Koerper.
    /// Jede Ebene zeigt dieselbe Animation, Richtung und Bildnummer wie der Koerper.
    /// Reihenfolge von unten nach oben: Hose, Schuhe, Brust, Handschuhe, Helm, Nebenhand, Hauptwaffe.
    /// </summary>
    [RequireComponent(typeof(DirectionalSpriteAnimator))]
    public class EquipmentVisuals : MonoBehaviour
    {
        public static readonly EquipSlot[] LayerOrder =
            { EquipSlot.Hose, EquipSlot.Schuhe, EquipSlot.Brust, EquipSlot.Handschuhe, EquipSlot.Helm, EquipSlot.Nebenhand, EquipSlot.Hauptwaffe };

        /// <summary>Eine Ebene pro Eintrag in LayerOrder (wird vom Builder angelegt)</summary>
        public SpriteRenderer[] layers = new SpriteRenderer[7];

        DirectionalSpriteAnimator _body;
        SpriteRenderer _bodyRenderer;
        PlayerEquipment _equipment;
        readonly SpriteAnimationSet[] _sets = new SpriteAnimationSet[7];

        void Awake()
        {
            _body = GetComponent<DirectionalSpriteAnimator>();
            _bodyRenderer = GetComponent<SpriteRenderer>();
            _equipment = GetComponentInParent<PlayerEquipment>();
        }

        void OnEnable()
        {
            _body.OnFrameChanged += HandleFrame;
            if (_equipment != null) _equipment.OnChanged += Refresh;
        }

        void OnDisable()
        {
            _body.OnFrameChanged -= HandleFrame;
            if (_equipment != null) _equipment.OnChanged -= Refresh;
        }

        void Start() => Refresh();

        /// <summary>Welche Ausruestung ist angelegt? Ebenen neu belegen.</summary>
        public void Refresh()
        {
            for (int l = 0; l < LayerOrder.Length; l++)
            {
                ItemData item = null;
                if (_equipment != null)
                    for (int p = 0; p < PlayerEquipment.SlotCount; p++)
                        if (EquipSlots.Positions[p] == LayerOrder[l] && _equipment.Get(p) != null) { item = _equipment.Get(p); break; }
                _sets[l] = item != null ? item.visual : null;
            }
            HandleFrame(_body.CurrentClip, _body.Facing, _body.FrameIndex);
        }

        void HandleFrame(string clip, Facing facing, int index)
        {
            for (int l = 0; l < layers.Length && l < _sets.Length; l++)
            {
                var layer = layers[l];
                if (layer == null) continue;
                Sprite sprite = null;
                var c = _sets[l] != null && clip != null ? _sets[l].Find(clip) : null;
                var frames = c != null ? c.Get(facing) : null;
                if (frames != null && frames.Length > 0) sprite = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
                layer.sprite = sprite;
                layer.color = _bodyRenderer.color;
            }
        }

        /// <summary>Farbe der Ebenen an den Koerper anpassen (Treffer- und Heil-Aufblitzen).</summary>
        public void SyncColor()
        {
            foreach (var layer in layers)
                if (layer != null) layer.color = _bodyRenderer.color;
        }

        /// <summary>Fuer Tests: Name des Bildes der Ebene fuer einen Platz (leer = keins)</summary>
        public string SpriteNameFor(EquipSlot slot)
        {
            for (int l = 0; l < LayerOrder.Length; l++)
                if (LayerOrder[l] == slot) return layers[l] != null && layers[l].sprite != null ? layers[l].sprite.name : "";
            return "";
        }
    }
}
