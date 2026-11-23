// Wald (Meilenstein 7c): Gegner nach Art, Szene, Licht
int wolf = 0, spider = 0, slime = 0;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
{
    if (a.creatureId == "wolf") wolf++; else if (a.creatureId == "spinne") spider++; else slime++;
}
var g = UnityEngine.GameObject.Find("Grid/Ground").GetComponent<UnityEngine.Tilemaps.Tilemap>();
return "Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " Woelfe=" + wolf + " Spinnen=" + spider + " Schleime=" + slime
    + " friedlich=" + Reborn.World.SceneRules.Peaceful + " Boden=" + g.GetUsedTilesCount();