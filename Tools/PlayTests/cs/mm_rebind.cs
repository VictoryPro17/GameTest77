// Tastenbelegung: state | apply:<Aktion>:<Teil oder ->:<kb|pad>:<Pfad> | reset
var views = UnityEngine.Object.FindObjectsByType<Reborn.UI.RebindingView>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
Reborn.UI.RebindingView rv = null;
foreach (var v in views) if (v.gameObject.activeInHierarchy && v.RowCount > 0) { rv = v; break; }
if (rv == null) foreach (var v in views) if (v.RowCount > 0) { rv = v; break; }
if (rv == null) return "keine Tastenbelegung";
string act = "%ACT%";
string res = "-";
if (act.StartsWith("apply:"))
{
    var parts = act.Split(new char[] { ':' }, 5);
    res = rv.ApplyForTest(parts[1], parts[2] == "-" ? null : parts[2], parts[3] == "kb", parts[4]);
}
else if (act == "reset") rv.resetButton.onClick.Invoke();
System.Func<string, string> keyOf = name =>
{
    var asset = Reborn.Core.GameSettings.Actions;
    if (asset == null) return "-";
    var map = asset.FindActionMap("Gameplay", false);
    var a = map != null ? map.FindAction(name, false) : null;
    if (a == null) return "-";
    foreach (var b in a.bindings)
        if (!b.isComposite && !b.isPartOfComposite && b.path.StartsWith("<Keyboard>")) return b.effectivePath;
    return "-";
};
return "Zeilen=" + rv.RowCount + " Meldung=" + rv.Message + " Attack=" + keyOf("Attack") + " Dodge=" + keyOf("Dodge") + " Ergebnis=" + res;
