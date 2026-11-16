// Seite (Platzhalter PAGE 0/1/2) oeffnen und auf Ueberlappungen pruefen; dazu 16:9 und 4:3 nachrechnen
var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
menu.ShowPage(%PAGE%);
UnityEngine.Canvas.ForceUpdateCanvases();
System.Func<string, UnityEngine.Rect> rectOf = path =>
{
    var t = menu.transform.Find(path);
    if (t == null) return new UnityEngine.Rect(0, 0, 0, 0);
    var c = new UnityEngine.Vector3[4];
    ((UnityEngine.RectTransform)t).GetWorldCorners(c);
    return UnityEngine.Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
};
System.Func<UnityEngine.Rect, UnityEngine.Rect, bool> hit = (a, b) => a.width > 0 && b.width > 0 && a.xMin < b.xMax - 0.5f && a.xMax > b.xMin + 0.5f && a.yMin < b.yMax - 0.5f && a.yMax > b.yMin + 0.5f;
var pairs = new System.Collections.Generic.List<string[]>();
string P = "Root/Panel/";
if (%PAGE% == 0)
{
    pairs.Add(new[] { P + "InventoryPage/Detail", P + "InventoryPage/Sort" });
    pairs.Add(new[] { P + "InventoryPage/Detail", P + "InventoryPage/Grid/Slot_05" });
    pairs.Add(new[] { P + "InventoryPage/Grid", P + "InventoryPage/Equipment" });
}
else if (%PAGE% == 1)
{
    for (int i = 0; i < 5; i++) pairs.Add(new[] { P + "CharacterPage/StatsBox", P + "CharacterPage/AttrBox_" + i });
    pairs.Add(new[] { P + "CharacterPage/DeleteSave", P + "CharacterPage/AttrBox_4" });
}
else
{
    for (int i = 0; i < 3; i++) pairs.Add(new[] { P + "MagicPage/Row_" + i, P + "MagicPage/Row_" + (i + 1) });
    pairs.Add(new[] { P + "MagicPage/Row_3", P + "MagicPage/Mana" });
}
pairs.Add(new[] { P + "TabMagie", P + "Close" });
int overlaps = 0; var which = "";
foreach (var pr in pairs) if (hit(rectOf(pr[0]), rectOf(pr[1]))) { overlaps++; which += pr[1].Substring(pr[1].LastIndexOf('/') + 1) + " "; }
var scaler = menu.GetComponent<UnityEngine.UI.CanvasScaler>();
System.Func<float, float, bool> fits = (w, h) =>
{
    float s = UnityEngine.Mathf.Min(w / scaler.referenceResolution.x, h / scaler.referenceResolution.y);
    return w / s >= 1759.5f && h / s >= 959.5f;
};
return "Seite=" + menu.Page + " Ueberlappungen=" + overlaps + (which != "" ? " (" + which.Trim() + ")" : "") + " Expand=" + (scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand) + " 16:9=" + fits(1920, 1080) + " 4:3=" + fits(1024, 768);
