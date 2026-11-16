// Zaehlt jedes gezeigte Bild der Ausweich-Animation (robuster als ein einzelner Blick)
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
UnityEngine.PlayerPrefs.SetInt("_t_frames_dodge", 0);
anim.OnFrameChanged += (clip, facing, index) =>
{
    if (clip == "dodge") UnityEngine.PlayerPrefs.SetInt("_t_frames_dodge", UnityEngine.PlayerPrefs.GetInt("_t_frames_dodge", 0) + 1);
};
return "Ausweich-Bilder werden gezaehlt";
