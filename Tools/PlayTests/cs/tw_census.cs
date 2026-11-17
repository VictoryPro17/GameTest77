// Stadt (Meilenstein 7a): was steht in der Szene
//#HEADER
var town = UnityEngine.GameObject.Find("Town");
int houses = 0, shops = 0, guilds = 0, wall = 0;
if (town != null)
{
    var h = town.transform.Find("Houses"); if (h != null) foreach (UnityEngine.Transform c in h) if (c.name.StartsWith("House_")) houses++;
    var s = town.transform.Find("Shops"); if (s != null) foreach (UnityEngine.Transform c in s) if (c.name.StartsWith("Shop_")) shops++;
    var g = town.transform.Find("Guilds"); if (g != null) foreach (UnityEngine.Transform c in g) if (c.name.StartsWith("Guild_")) guilds++;
    var w = town.transform.Find("TownWall"); if (w != null) wall = w.childCount;
}
int soldiers = 0, people = 0;
foreach (var v in UnityEngine.Object.FindObjectsByType<Reborn.World.Wanderer>())
{
    people++;
    if (v.name.StartsWith("Villager_soldat")) soldiers++;
}
int missing = 0;
foreach (var sr in UnityEngine.Object.FindObjectsByType<UnityEngine.SpriteRenderer>()) if (sr.sprite == null && sr.GetComponent<Reborn.Animation.DirectionalSpriteAnimator>() == null && !sr.name.StartsWith("Layer_")) missing++;   // leere Ausruestungs-Ebenen sind gewollt
var cave = UnityEngine.GameObject.Find("Portal_Kampf");
return "Haeuser=" + houses + " Laeden=" + shops + " Gilden=" + guilds + " Mauerteile=" + wall + " Leute=" + people + " Soldaten=" + soldiers
    + " Hoehle=" + (cave != null && cave.transform.parent != null ? cave.transform.parent.name : "-") + " ohneBild=" + missing;
