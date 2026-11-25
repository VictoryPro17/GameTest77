// Einstellungen (Hauptmenue oder Pause-Menue): state | click:<Knopf-Feldname> | reset (Sitzungs-Standard)
var views = UnityEngine.Object.FindObjectsByType<Reborn.UI.SettingsView>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
Reborn.UI.SettingsView sv = null;
foreach (var v in views) if (v.root != null && v.root.activeSelf) { sv = v; break; }
if (sv == null && views.Length > 0) sv = views[0];
if (sv == null) return "keine Einstellungen";
var ic = System.Globalization.CultureInfo.InvariantCulture;
string act = "%ACT%";
if (act == "reset") Reborn.Core.GameSettings.UseSessionDefaults();
else if (act.StartsWith("click:"))
{
    var f = typeof(Reborn.UI.SettingsView).GetField(act.Substring(6));
    var b = f != null ? f.GetValue(sv) as UnityEngine.UI.Button : null;
    if (b == null) return "kein Knopf " + act;
    b.onClick.Invoke();
}
var d = Reborn.Core.GameSettings.Current;
float refX = 0f;
foreach (var sc in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.CanvasScaler>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
    if (sc.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize) { refX = sc.referenceResolution.x; break; }
return "Sprache=" + Reborn.Core.GameSettings.Language
    + " Master=" + d.masterVolume.ToString("0.##", ic)
    + " Effekte=" + d.sfxVolume.ToString("0.##", ic)
    + " Vollbild=" + d.fullscreen
    + " Qualitaet=" + d.quality
    + " FPS=" + d.frameRate
    + " targetFPS=" + UnityEngine.Application.targetFrameRate
    + " UI=" + d.uiScale.ToString("0.##", ic)
    + " Zahlen=" + d.damageNumbers
    + " Handy=" + d.mobileControls
    + " Listener=" + UnityEngine.AudioListener.volume.ToString("0.00", ic)
    + " Scaler=" + refX.ToString("0", ic)
    + " Werte=" + sv.languageValue.text + "|" + sv.masterValue.text + "|" + sv.frameRateValue.text
    + " Inventar=" + Reborn.Core.Loc.T("Inventar")
    + " EN=" + Reborn.Core.Loc.EnglishCount
    + " Sitzung=" + Reborn.Core.GameSettings.SessionDefaults;
