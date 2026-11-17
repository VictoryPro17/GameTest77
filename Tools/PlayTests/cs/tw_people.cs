// Stadt: wie viele Bewohner und Soldaten sich bewegt haben (Strecke > 0,5)
int moved = 0, soldiers = 0, walkers = 0;
foreach (var v in UnityEngine.Object.FindObjectsByType<Reborn.World.Wanderer>())
{
    if (v.waypoints.Length < 2) continue;
    walkers++;
    if (v.Travelled > 0.5f) { moved++; if (v.name.StartsWith("Villager_soldat")) soldiers++; }
}
return "laufend=" + walkers + " bewegt=" + moved + " Soldaten=" + soldiers;
