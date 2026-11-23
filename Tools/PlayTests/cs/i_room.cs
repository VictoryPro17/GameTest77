// Raum (Meilenstein 7b): Licht, friedlich, Gildenmeister, Kacheln, Lichtquellen, Tueren
//#HEADER
float global = -1f; int points = 0;
foreach (var l in UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>())
{
    if (l.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global) global = l.intensity;
    else points++;
}
string master = "-";
foreach (var n in UnityEngine.Object.FindObjectsByType<Reborn.World.Npc>()) if (n.kind == Reborn.World.NpcKind.Gildenmeister) master = n.displayName;
var g = UnityEngine.GameObject.Find("Grid/Ground").GetComponent<UnityEngine.Tilemaps.Tilemap>();
var w = UnityEngine.GameObject.Find("Grid/Walls").GetComponent<UnityEngine.Tilemaps.Tilemap>();
int missing = 0;
foreach (var sr in UnityEngine.Object.FindObjectsByType<UnityEngine.SpriteRenderer>()) if (sr.sprite == null && sr.GetComponent<Reborn.Animation.DirectionalSpriteAnimator>() == null && !sr.name.StartsWith("Layer_")) missing++;
return "Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " Licht=" + global.ToString("F2", ic)
    + " friedlich=" + Reborn.World.SceneRules.Peaceful + " Meister=" + master + " Boden=" + g.GetUsedTilesCount() + " Mauerkacheln=" + w.GetUsedTilesCount()
    + " Lichter=" + points + " ohneBild=" + missing;