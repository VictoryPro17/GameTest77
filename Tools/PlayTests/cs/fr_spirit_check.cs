// Fernkampf des Frostgeists: Schuesse, Eissplitter, Treffer
//#HEADER
int shots = -1;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>())
    if (a.creatureId == "frostgeist") shots = a.Shots;
return "Schuesse=" + shots + " Splitter=" + Reborn.Enemies.IceShard.ShotCount + " SplitterTreffer=" + Reborn.Enemies.IceShard.HitCount + " Spieler=" + hp.Current;
