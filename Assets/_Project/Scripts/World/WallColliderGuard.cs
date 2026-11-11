using UnityEngine;
using UnityEngine.Tilemaps;

namespace Reborn.World
{
    /// <summary>
    /// Sicherheitsnetz fuer die Mauern: Ist der zusammengefasste Collider beim Start leer,
    /// wird er aus der Tilemap neu erzeugt. Bleibt er leer, kommt eine Fehlermeldung.
    /// </summary>
    [RequireComponent(typeof(CompositeCollider2D))]
    public class WallColliderGuard : MonoBehaviour
    {
        void Awake()
        {
            var composite = GetComponent<CompositeCollider2D>();
            if (composite.shapeCount > 0) return;

            var tilemap = GetComponent<Tilemap>();
            var tilemapCollider = GetComponent<TilemapCollider2D>();
            if (tilemap != null) tilemap.CompressBounds();
            if (tilemapCollider != null) tilemapCollider.ProcessTilemapChanges();
            composite.GenerateGeometry();

            if (composite.shapeCount == 0)
                Debug.LogError("Mauern ohne Collider: Der Spieler kann durch die Waende laufen. Bitte 'Reborn > Spiel neu bauen' ausfuehren.", this);
        }
    }
}
