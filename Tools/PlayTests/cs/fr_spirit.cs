// Genau einen Frostgeist einschalten, Spieler 4 Einheiten links davon, verwundbar mit viel Leben
//#HEADER
Reborn.Enemies.SlimeAI spirit = null;
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
    if (spirit == null && a.creatureId == "frostgeist" && !a.name.StartsWith("Dragon_Spirit")) spirit = a;
if (spirit == null) return "kein Frostgeist";
foreach (var a in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
    if (a != spirit) a.gameObject.SetActive(false);
spirit.gameObject.SetActive(true);
rb.position = (UnityEngine.Vector2)spirit.transform.position + new UnityEngine.Vector2(-4f, 0f);
p.transform.position = rb.position;
rb.linearVelocity = UnityEngine.Vector2.zero;
hp.invulnerable = false;
hp.SetMaxHealth(1000, true);
spirit.SetShootTimer(0f);
return "ok";
