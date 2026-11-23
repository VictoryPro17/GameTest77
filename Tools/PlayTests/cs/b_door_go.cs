// Spieler mitten auf eine Tuer stellen (Collider-Mitte des Spielers liegt 0,12 ueber dem Fusspunkt)
//#HEADER
var go = UnityEngine.GameObject.Find("%NAME%");
if (go == null) return "Tuer %NAME% fehlt";
rb.position = (UnityEngine.Vector2)go.transform.position - new UnityEngine.Vector2(0f, 0.12f);
p.transform.position = rb.position;
rb.linearVelocity = UnityEngine.Vector2.zero;
return "auf Tuer %NAME%";