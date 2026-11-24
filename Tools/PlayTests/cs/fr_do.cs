// Eisdrache (Meilenstein 9a): timers:<blizzard>:<atem>:<regen>:<fluegel> | place:<dx>:<dy> | dragonto:<x>:<y> | pillar:<i> | inv:true/false | hp:<max> | hit:<n> | wake | clock:<minuten>
//#HEADER
var dragon = UnityEngine.Object.FindAnyObjectByType<Reborn.Enemies.DragonAI>();
var parts = "%ACT%".Split(':');
var ci = System.Globalization.CultureInfo.InvariantCulture;
switch (parts[0])
{
    case "timers": if (dragon != null) dragon.SetTimers(float.Parse(parts[1], ci), float.Parse(parts[2], ci), float.Parse(parts[3], ci), float.Parse(parts[4], ci)); break;
    case "place":
        if (dragon == null) return "kein Drache";
        rb.position = (UnityEngine.Vector2)dragon.transform.position + new UnityEngine.Vector2(float.Parse(parts[1], ci), float.Parse(parts[2], ci));
        p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero; break;
    case "dragonto":
        if (dragon == null) return "kein Drache";
        var drb = dragon.GetComponent<UnityEngine.Rigidbody2D>();
        var tgt = new UnityEngine.Vector2(float.Parse(parts[1], ci), float.Parse(parts[2], ci));
        drb.position = tgt; dragon.transform.position = tgt; drb.linearVelocity = UnityEngine.Vector2.zero; break;
    case "pillar":
        if (dragon == null) return "kein Drache";
        var pil = UnityEngine.GameObject.Find("IcePillar_" + parts[1]);
        if (pil == null) return "keine Saeule " + parts[1];
        var dir = ((UnityEngine.Vector2)pil.transform.position - (UnityEngine.Vector2)dragon.transform.position).normalized;
        rb.position = (UnityEngine.Vector2)pil.transform.position + dir * 0.9f;
        p.transform.position = rb.position; rb.linearVelocity = UnityEngine.Vector2.zero; break;
    case "inv": hp.invulnerable = parts[1] == "true"; break;
    case "hp": hp.SetMaxHealth(int.Parse(parts[1]), true); break;
    case "hit": if (dragon != null) dragon.Health.TakeDamage(int.Parse(parts[1]), UnityEngine.Vector2.zero); break;
    case "wake": if (dragon != null) dragon.Wake(); break;
    case "clock":
        Reborn.Save.SaveSystem.ClockOffset = System.TimeSpan.FromMinutes(double.Parse(parts[1], ci));
        p.GetComponent<Reborn.Save.SaveSystem>().Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Frost");
        return "Uhr +" + parts[1] + " min, Szene neu geladen";
}
return dragon != null ? "ok Drache " + dragon.Health.Current + "/" + dragon.Health.maxHealth + " " + dragon.StateName : "ok (kein Drache)";
