// Ton pruefen: state | play:<SfxId> | mute | unmute | click
string[] parts = "%ACT%".Split(':');
string knopf = "-";
switch (parts[0])
{
    case "state": break;
    case "play":
        Reborn.Core.Sfx.Play((Reborn.Core.SfxId)System.Enum.Parse(typeof(Reborn.Core.SfxId), parts[1]));
        break;
    case "mute": Reborn.Core.GameSettings.Change(d => d.sfxVolume = 0f); break;
    case "unmute": Reborn.Core.GameSettings.Change(d => d.sfxVolume = 0.8f); break;
    case "click":
    {
        UnityEngine.UI.Button target = null;
        foreach (var b in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None))
        {
            if (!b.isActiveAndEnabled || !b.interactable || b.GetComponent<Reborn.Core.ClickSoundMarker>() == null) continue;
            string bn = b.name.ToLowerInvariant();
            if (bn.Contains("quit") || bn.Contains("exit") || bn.Contains("beenden")) continue;   // nichts Gefaehrliches ausloesen
            target = b;
            break;
        }
        if (target == null)
        {
            // kein sichtbarer Knopf: einen Test-Knopf anlegen, von HookButtons versehen lassen und anklicken
            var go = new UnityEngine.GameObject("TestKnopf", typeof(UnityEngine.RectTransform), typeof(UnityEngine.UI.Button));
            Reborn.Core.Sfx.HookButtons();
            target = go.GetComponent<UnityEngine.UI.Button>();
            knopf = "Test";
            target.onClick.Invoke();
            UnityEngine.Object.Destroy(go);
        }
        else
        {
            knopf = target.name;
            target.onClick.Invoke();
        }
        break;
    }
    default: return "unbekannt %ACT%";
}
var host = UnityEngine.GameObject.Find("Sfx");
return "Gespielt=" + Reborn.Core.Sfx.Played
    + " Stumm=" + Reborn.Core.Sfx.Muted
    + " Letzt=" + Reborn.Core.Sfx.LastId
    + " Lautst=" + Reborn.Core.Sfx.LastVolume.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
    + " Klicks=" + Reborn.Core.Sfx.CountOf(Reborn.Core.SfxId.Click)
    + " Swing=" + Reborn.Core.Sfx.CountOf(Reborn.Core.SfxId.Swing)
    + " Bank=" + Reborn.Core.SoundBank.Count
    + " Quellen=" + (host != null ? host.GetComponents<UnityEngine.AudioSource>().Length : 0)
    + " Listener=" + UnityEngine.AudioListener.volume.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
    + " Knopf=" + knopf;
