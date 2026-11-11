// P1-Test: Spieler OHNE Test-Schutz, jeder Treffer wird mit genauer Zeit ins Log geschrieben
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var hp = p.GetComponent<Reborn.Combat.Health>();
hp.invulnerable = false;
hp.OnDamaged += (amount, kb) => UnityEngine.Debug.Log("IFRAME_HIT t=" + UnityEngine.Time.time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " hp=" + hp.Current);
var slimes = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>();
int i = 0;
foreach (var s in slimes)
{
    float a = i * UnityEngine.Mathf.PI * 2f / slimes.Length;
    s.transform.position = p.transform.position + new UnityEngine.Vector3(UnityEngine.Mathf.Cos(a), UnityEngine.Mathf.Sin(a), 0f) * 0.7f;
    i++;
}
UnityEngine.Debug.Log("IFRAME_START t=" + UnityEngine.Time.time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " hp=" + hp.Current);
return "Start HP " + hp.Current + "/" + hp.maxHealth + " Schleime=" + slimes.Length;
