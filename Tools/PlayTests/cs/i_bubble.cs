// Sprechblase des Gildenmeisters (Meilenstein 7b)
foreach (var n in UnityEngine.Object.FindObjectsByType<Reborn.World.Npc>())
{
    if (n.kind != Reborn.World.NpcKind.Gildenmeister) continue;
    var b = n.GetComponent<Reborn.World.SpeechBubble>();
    return "Blase=" + (b != null && b.IsVisible) + " Fenster=" + Reborn.UI.TradeWindow.IsOpen + " Text=" + (b != null ? b.CurrentText : "");
}
return "kein Gildenmeister";