// A1-A3: Animationssets aus Blender (Meilenstein 5): Bildanzahl je Clip und Richtung, kein Bild fehlt,
// Groesse 128 px (Boss 256 px), 128 Pixel pro Einheit, Fusspunkt 0,2138.
var sets = new System.Collections.Generic.List<(string path, string spec, int size)>
{
    ("Assets/_Project/Data/Animations/Player_Anim.asset", "idle:4,walk:8,attack:5,dodge:4,cast:5,hurt:2,death:4", 128),
    ("Assets/_Project/Data/Animations/Slime_Anim.asset", "idle:4,walk:6,attack:5,hurt:2,death:4", 128),
    ("Assets/_Project/Data/Animations/Wolf_Anim.asset", "idle:4,walk:6,attack:5,hurt:2,death:4", 128),
    ("Assets/_Project/Data/Animations/Spider_Anim.asset", "idle:4,walk:6,attack:5,hurt:2,death:4", 128),
    ("Assets/_Project/Data/Animations/Boss_Anim.asset", "idle:4,walk:6,attack:6,rootattack:6,roar:4,hurt:2,death:6", 256),
};
int equip = 0;
foreach (var guid in UnityEditor.AssetDatabase.FindAssets("Equip_ t:SpriteAnimationSet", new[] { "Assets/_Project/Data/Animations" }))
{
    sets.Add((UnityEditor.AssetDatabase.GUIDToAssetPath(guid), "idle:4,walk:8,attack:5,dodge:4,cast:5,hurt:2,death:4", 128));
    equip++;
}
var errors = new System.Collections.Generic.List<string>();
int ok = 0, sprites = 0;
foreach (var (path, spec, size) in sets)
{
    var set = UnityEditor.AssetDatabase.LoadAssetAtPath<Reborn.Animation.SpriteAnimationSet>(path);
    string name = System.IO.Path.GetFileNameWithoutExtension(path);
    if (set == null) { errors.Add(name + " fehlt"); continue; }
    int before = errors.Count;
    foreach (var part in spec.Split(','))
    {
        var kv = part.Split(':');
        int n = int.Parse(kv[1]);
        var clip = set.Find(kv[0]);
        if (clip == null) { errors.Add(name + "." + kv[0] + " fehlt"); continue; }
        foreach (var facing in new[] { Reborn.Animation.Facing.Down, Reborn.Animation.Facing.Up, Reborn.Animation.Facing.Left, Reborn.Animation.Facing.Right })
        {
            var frames = clip.Get(facing);
            if (frames == null || frames.Length != n) { errors.Add(name + "." + kv[0] + "." + facing + " Bilder=" + (frames == null ? 0 : frames.Length)); continue; }
            foreach (var sp in frames)
            {
                if (sp == null) { errors.Add(name + "." + kv[0] + "." + facing + " leer"); continue; }
                sprites++;
                float pivotY = sp.pivot.y / sp.rect.height;
                if ((int)sp.rect.width != size || !UnityEngine.Mathf.Approximately(sp.pixelsPerUnit, 128f) || UnityEngine.Mathf.Abs(pivotY - 0.2138f) > 0.002f)
                    errors.Add(sp.name + " " + sp.rect.width + "px ppu=" + sp.pixelsPerUnit + " pivot=" + pivotY.ToString("F4"));
            }
        }
    }
    if (errors.Count == before) ok++;
}
string first = errors.Count > 0 ? " erster=" + errors[0] : "";
return "Sets ok=" + ok + "/" + sets.Count + " Ausruestung=" + equip + " Bilder=" + sprites + " Fehler=" + errors.Count + first;
