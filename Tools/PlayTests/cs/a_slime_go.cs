// A5: Einen (ausgeschalteten) Schleim 2,5 Einheiten rechts vom Spieler wieder einschalten, er soll verfolgen
//#HEADER
var slimes = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
if (slimes.Length == 0) return "kein Schleim";
var s = slimes[0];
var target = (UnityEngine.Vector2)p.transform.position + new UnityEngine.Vector2(2.5f, 0f);
s.detectRadius = 5f;   // L5 (lk_number.cs) setzt beim selben Schleim 0, damit er stillhaelt
s.transform.position = target;
s.gameObject.SetActive(true);
s.GetComponent<UnityEngine.Rigidbody2D>().position = target;
return "Schleim an " + target + " aktiv=" + s.gameObject.activeInHierarchy;
