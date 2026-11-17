// B1: Start in der Basis: Szene, Ankunftspunkt, friedlich, Gold, NPCs, Portal, Mauer
//#HEADER
var wallet = p.GetComponent<Reborn.Economy.PlayerWallet>();
var walls = UnityEngine.GameObject.Find("Grid/Walls");
int wallParts = 0;
if (walls != null)
{
    var comp = walls.GetComponent<UnityEngine.CompositeCollider2D>();
    if (comp != null) wallParts += comp.shapeCount;
    wallParts += walls.GetComponentsInChildren<UnityEngine.BoxCollider2D>().Length;
}
// Kacheln zaehlen (Meilenstein 6: die Basis wurde einmal ohne Boden gespeichert, die Mauerboxen verdeckten das)
var groundGo = UnityEngine.GameObject.Find("Grid/Ground");
int ground = groundGo != null ? groundGo.GetComponent<UnityEngine.Tilemaps.Tilemap>().GetUsedTilesCount() : 0;
int wallTiles = walls != null ? walls.GetComponent<UnityEngine.Tilemaps.Tilemap>().GetUsedTilesCount() : 0;
var portal = UnityEngine.Object.FindAnyObjectByType<Reborn.World.Portal>();
return "Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
    + " Pos=(" + p.transform.position.x.ToString("F1", ic) + "," + p.transform.position.y.ToString("F1", ic) + ")"
    + " friedlich=" + Reborn.World.SceneRules.Peaceful + " Gold=" + wallet.Gold
    + " NPCs=" + UnityEngine.Object.FindObjectsByType<Reborn.World.Npc>().Length
    + " Portal=" + (portal != null ? portal.name + ">" + portal.targetScene : "-") + " Mauer=" + wallParts
    + " Boden=" + ground + " Mauerkacheln=" + wallTiles;