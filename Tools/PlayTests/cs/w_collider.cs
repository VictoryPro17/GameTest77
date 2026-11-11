// W1: Hat der Mauer-Collider Formen, und trifft man die vier Mauern?
var walls = UnityEngine.GameObject.Find("Walls");
var comp = walls.GetComponent<UnityEngine.CompositeCollider2D>();
UnityEngine.Physics2D.SyncTransforms();
var pts = new[] { new UnityEngine.Vector2(-9.5f, 0f), new UnityEngine.Vector2(9.5f, 0f), new UnityEngine.Vector2(0f, 5.5f), new UnityEngine.Vector2(0f, -5.5f) };
int hits = 0;
foreach (var pt in pts) if (UnityEngine.Physics2D.OverlapPoint(pt, 1 << 9) != null) hits++;
bool middleFree = UnityEngine.Physics2D.OverlapPoint(new UnityEngine.Vector2(0f, 2f), 1 << 9) == null;
return "Formen=" + comp.shapeCount + " Mauertreffer=" + hits + "/4 Raummitte frei=" + middleFree;
