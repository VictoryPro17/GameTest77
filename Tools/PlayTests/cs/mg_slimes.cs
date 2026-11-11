// Schleime an Positionen setzen (Platzhalter POS = "x,y;x,y"), volle HP; uebrige ausschalten
//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var list = "%POS%".Split(';');
var all = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.InstanceID);
int n = 0;
for (int i = 0; i < all.Length; i++)
{
    var sl = all[i];
    if (i < list.Length && list[i] != "")
    {
        var xy = list[i].Split(',');
        sl.gameObject.SetActive(true);
        var srb = sl.GetComponent<UnityEngine.Rigidbody2D>();
        srb.position = new UnityEngine.Vector2(float.Parse(xy[0], ic), float.Parse(xy[1], ic));
        sl.transform.position = srb.position;
        srb.linearVelocity = UnityEngine.Vector2.zero;
        sl.GetComponent<Reborn.Combat.Health>().SetMaxHealth(30, true);
        sl.detectRadius = 0f; // Test-Schleime bleiben stehen, damit Treffer und Rueckstoss messbar sind
        sl.name = "Slime_T" + n;
        n++;
    }
    else sl.gameObject.SetActive(false);
}
rb.position = UnityEngine.Vector2.zero;
p.transform.position = UnityEngine.Vector3.zero;
return "Schleime gesetzt=" + n;
