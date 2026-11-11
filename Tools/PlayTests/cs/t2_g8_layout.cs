// Ueberlappt das Detail-Fenster andere Teile? Passt das Menue in den Bildschirm?
var ic = System.Globalization.CultureInfo.InvariantCulture;
var menu = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.GameMenu>();
var canvasRt = (UnityEngine.RectTransform)menu.transform;
System.Func<string, UnityEngine.Rect> rectOf = path =>
{
    var rt = (UnityEngine.RectTransform)menu.transform.Find(path);
    var c = new UnityEngine.Vector3[4];
    rt.GetWorldCorners(c);
    return UnityEngine.Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
};
var detail = rectOf("Root/Panel/InventoryPage/Detail");
var panel = rectOf("Root/Panel");
string[] others = { "Root/Panel/InventoryPage/Sort", "Root/Panel/InventoryPage/Filter_3", "Root/Panel/InventoryPage/Grid/Slot_05", "Root/Panel/InventoryPage/Grid/Slot_23", "Root/Panel/Close" };
const float eps = 0.5f;
int overlaps = 0;
var which = "";
foreach (var o in others)
{
    var r = rectOf(o);
    bool hit = r.xMin < detail.xMax - eps && r.xMax > detail.xMin + eps && r.yMin < detail.yMax - eps && r.yMax > detail.yMin + eps;
    if (hit) { overlaps++; which += o.Substring(o.LastIndexOf('/') + 1) + " "; }
}
bool inside = detail.xMin >= panel.xMin - eps && detail.xMax <= panel.xMax + eps && detail.yMin >= panel.yMin - eps && detail.yMax <= panel.yMax + eps;
var size = canvasRt.rect.size;
bool fits = size.x >= 1759.5f && size.y >= 959.5f;
return "Ueberlappungen=" + overlaps + (which != "" ? " (" + which.Trim() + ")" : "") + " DetailImPanel=" + inside + " Canvas=" + size.x.ToString("F0", ic) + "x" + size.y.ToString("F0", ic) + " passt=" + fits;
