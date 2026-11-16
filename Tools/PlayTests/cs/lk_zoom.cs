// Kamera fuer Figuren-Bilder naeher heran (Platzhalter SIZE), Spieler in die Raummitte
var cam = UnityEngine.Camera.main;
cam.orthographicSize = %SIZE%f;
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var rb = p.GetComponent<UnityEngine.Rigidbody2D>();
rb.position = new UnityEngine.Vector2(%X%f, %Y%f);
p.transform.position = rb.position;
return "Kamera " + cam.orthographicSize;
