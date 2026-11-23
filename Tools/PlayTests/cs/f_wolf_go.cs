// Einen Wolf (oder eine Spinne) einschalten und direkt neben den Spieler stellen; Spieler verwundbar
//#HEADER
hp.invulnerable = false;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
{
    if (a.creatureId != "%ART%") continue;
    var pos = (UnityEngine.Vector2)p.transform.position + new UnityEngine.Vector2(0.9f, 0f);
    a.gameObject.SetActive(true);
    a.transform.position = pos;
    a.GetComponent<UnityEngine.Rigidbody2D>().position = pos;
    a.detectRadius = 5f;
    a.gameObject.name = "Test_%ART%";
    UnityEngine.PlayerPrefs.SetInt("_t_hp", hp.Current);
    UnityEngine.PlayerPrefs.SetInt("_t_attack", 0);
    var anim = a.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
    anim.OnFrameChanged += (clip, f, i) => { if (clip == "attack") UnityEngine.PlayerPrefs.SetInt("_t_attack", 1); };
    return "%ART% neben dem Spieler, HP " + hp.Current;
}
return "keine Kreatur %ART%";