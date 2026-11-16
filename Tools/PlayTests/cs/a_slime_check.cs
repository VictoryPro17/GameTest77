// A5: Verfolgender Schleim zeigt die Lauf-Animation; danach wieder ausschalten
var slimes = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>();
if (slimes.Length == 0) return "kein aktiver Schleim";
var s = slimes[0];
var anim = s.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var sr = anim.GetComponent<UnityEngine.SpriteRenderer>();
string result = "clip=" + anim.CurrentClip + " bild=" + (sr.sprite != null ? sr.sprite.name : "-");
s.gameObject.SetActive(false);
return result;
