// Spieler an eine Stelle setzen (Stillstand)
//#HEADER
rb.position = new UnityEngine.Vector2(%X%f, %Y%f);
p.transform.position = rb.position;
rb.linearVelocity = UnityEngine.Vector2.zero;
return "Spieler bei (%X%, %Y%)";