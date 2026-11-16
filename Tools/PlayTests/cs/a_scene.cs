// A7: Testraum mit Blender-Kacheln (4 Gras-Varianten + Weg, 64 px bei 64 Pixel pro Einheit),
// Deko mit Bildern und Schaufenster (Wolf, Waldwaechter, Spinne spielen "idle")
var ground = UnityEngine.GameObject.Find("Grid/Ground").GetComponent<UnityEngine.Tilemaps.Tilemap>();
var used = new System.Collections.Generic.HashSet<string>();
bool tileSize = true;
foreach (var pos in ground.cellBounds.allPositionsWithin)
{
    var sp = ground.GetSprite(pos);
    if (sp == null) continue;
    used.Add(sp.name);
    if ((int)sp.rect.width != 64 || !UnityEngine.Mathf.Approximately(sp.pixelsPerUnit, 64f)) tileSize = false;
}
var names = new System.Collections.Generic.List<string>(used);
names.Sort();
var deco = UnityEngine.GameObject.Find("Decoration");
int decoCount = 0, decoMissing = 0;
if (deco != null) foreach (var sr in deco.GetComponentsInChildren<UnityEngine.SpriteRenderer>()) { decoCount++; if (sr.sprite == null) decoMissing++; }
var show = UnityEngine.GameObject.Find("Showcase");
var clips = new System.Collections.Generic.List<string>();
if (show != null) foreach (var a in show.GetComponentsInChildren<Reborn.Animation.DirectionalSpriteAnimator>())
    clips.Add(a.transform.parent.name.Replace("Showcase_", "") + ":" + a.CurrentClip + (a.GetComponent<UnityEngine.SpriteRenderer>().sprite != null ? "" : "(leer)"));
return "Boden=" + string.Join(",", names) + " 64px=" + tileSize + " Deko=" + decoCount + " ohneBild=" + decoMissing + " Schaufenster=" + string.Join(",", clips);
