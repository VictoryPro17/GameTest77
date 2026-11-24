// Frostgipfel (Meilenstein 9a): Gegner nach Art, Saeulen, Drache, Szene
int wolf = 0, ghost = 0, ranged = 0;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
{
    if (a.creatureId == "eiswolf") wolf++; else if (a.creatureId == "frostgeist") ghost++;
    if (a.IsRanged) ranged++;
}
int pillars = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.IcePillar>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Length;
var dragon = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.DragonAI>(UnityEngine.FindObjectsInactive.Include);
var g = UnityEngine.GameObject.Find("Grid/Ground").GetComponent<UnityEngine.Tilemaps.Tilemap>();
return "Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + " Eiswoelfe=" + wolf + " Frostgeister=" + ghost + " Fernkampf=" + ranged
    + " Saeulen=" + pillars + " Drache=" + (dragon != null) + " friedlich=" + Reborn.World.SceneRules.Peaceful + " Boden=" + g.GetUsedTilesCount();
