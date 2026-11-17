// Spieler in den Trigger eines Portals stellen
//#HEADER
var go = UnityEngine.GameObject.Find("%NAME%");
if (go == null) return "Portal %NAME% fehlt";
rb.position = (UnityEngine.Vector2)go.transform.position + new UnityEngine.Vector2(0f, 0.25f);
p.transform.position = rb.position;
return "im Portal %NAME%";