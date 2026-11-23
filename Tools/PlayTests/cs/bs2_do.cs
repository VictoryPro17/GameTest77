// Boss Stufe 2 (Meilenstein 8b): timers:<ult>:<woelfe>:<dornen>:<ansturm>[:<wurzeln>] | placec:<abstand> | place:<dx>:<dy> | inv:true/false | hp:<max> | hit:<n> | clock:<minuten>
//#HEADER
var boss = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.BossAI>();
var parts = "%ACT%".Split(':');
var ci = System.Globalization.CultureInfo.InvariantCulture;
switch (parts[0])
{
    case "timers": if (boss != null) boss.SetTimers(float.Parse(parts[1], ci), float.Parse(parts[2], ci), float.Parse(parts[3], ci), float.Parse(parts[4], ci), parts.Length > 5 ? float.Parse(parts[5], ci) : -1f); break;
    case "placec":
        // Spieler im Abstand <d> vom Boss Richtung Raummitte (weg von den Waenden)
        if (boss == null) return "kein Boss";
        var toCenter = -(UnityEngine.Vector2)boss.transform.position;
        if (toCenter.sqrMagnitude < 0.01f) toCenter = UnityEngine.Vector2.right;
        rb.position = (UnityEngine.Vector2)boss.transform.position + toCenter.normalized * float.Parse(parts[1], ci);
        p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero; break;
    case "place":
        if (boss == null) return "kein Boss";
        rb.position = (UnityEngine.Vector2)boss.transform.position + new UnityEngine.Vector2(float.Parse(parts[1], ci), float.Parse(parts[2], ci));
        p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero; break;
    case "inv": hp.invulnerable = parts[1] == "true"; break;
    case "hp": hp.SetMaxHealth(int.Parse(parts[1]), true); break;
    case "hit": if (boss != null) boss.Health.TakeDamage(int.Parse(parts[1]), UnityEngine.Vector2.zero); break;
    case "clock":
        Reborn.Save.SaveSystem.ClockOffset = System.TimeSpan.FromMinutes(double.Parse(parts[1], ci));
        p.GetComponent<Reborn.Save.SaveSystem>().Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Room_Test");
        return "Uhr +" + parts[1] + " min, Szene neu geladen";
}
return boss != null ? "ok Boss " + boss.Health.Current + "/" + boss.Health.maxHealth + " Stufe " + boss.Stage : "ok (kein Boss)";