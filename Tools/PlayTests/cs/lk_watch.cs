// Beobachtet jedes Bild des Koerpers: Zeigen alle belegten Ebenen danach dasselbe Bild?
// Der Vergleich laeuft im naechsten Input-Update, wenn die Ebenen nachgezogen haben.
// Zaehler in PlayerPrefs: _t_frames_<clip> und _t_mismatch.
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
foreach (var c in new[] { "idle", "walk", "attack", "dodge", "cast" }) UnityEngine.PlayerPrefs.DeleteKey("_t_frames_" + c);
UnityEngine.PlayerPrefs.SetInt("_t_mismatch", 0);
bool pending = false;
string pendingClip = "";
anim.OnFrameChanged += (clip, facing, index) => { pending = true; pendingClip = clip; };
UnityEngine.InputSystem.InputSystem.onAfterUpdate += () =>
{
    if (!pending || vis == null || bodySr == null) return;
    pending = false;
    string body = bodySr.sprite != null ? bodySr.sprite.name : "-";
    foreach (var slot in Reborn.Animation.EquipmentVisuals.LayerOrder)
    {
        var n = vis.SpriteNameFor(slot);
        if (n != "" && n != body) UnityEngine.PlayerPrefs.SetInt("_t_mismatch", UnityEngine.PlayerPrefs.GetInt("_t_mismatch", 0) + 1);
    }
    var key = "_t_frames_" + pendingClip;
    UnityEngine.PlayerPrefs.SetInt(key, UnityEngine.PlayerPrefs.GetInt(key, 0) + 1);
};
return "Beobachtung an";
