// 0-HP-Raetsel: Mauern nur zur Laufzeit abschalten und Schaden, Tod und Positionsspruenge protokollieren
//#HEADER
var walls = UnityEngine.GameObject.Find("Walls");
foreach (var c in walls.GetComponents<UnityEngine.Collider2D>()) c.enabled = false;
System.Func<string> who = () =>
{
    var lines = System.Environment.StackTrace.Split('\n');
    var r = "";
    foreach (var l in lines) if (l.Contains("Reborn.") && !l.Contains("Health.") ) { r += l.Trim() + " | "; if (r.Length > 300) break; }
    return r == "" ? "(kein Reborn-Aufrufer)" : r;
};
hp.OnDamaged += (a, k) => UnityEngine.Debug.Log("ZHP_DMG t=" + UnityEngine.Time.time.ToString("F2", ic) + " amount=" + a + " hp=" + hp.Current + " pos=" + p.transform.position + " von: " + who());
hp.OnDied += () => UnityEngine.Debug.Log("ZHP_DIED t=" + UnityEngine.Time.time.ToString("F2", ic) + " pos=" + p.transform.position + " von: " + who());
var last = (UnityEngine.Vector2)p.transform.position;
UnityEngine.InputSystem.InputSystem.onAfterUpdate += () =>
{
    if (p == null) return;
    var now = (UnityEngine.Vector2)p.transform.position;
    if ((now - last).magnitude > 0.8f) UnityEngine.Debug.Log("ZHP_JUMP t=" + UnityEngine.Time.time.ToString("F2", ic) + " von " + last + " nach " + now + " hp=" + hp.Current);
    last = now;
};
return "Mauern aus, Protokoll an. HP " + hp.Current + " unverwundbar=" + hp.invulnerable;
