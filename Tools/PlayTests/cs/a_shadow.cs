// A6: Blob-Schatten unter Spieler, Schleimen und Schaufenster-Figuren (unter allen Figuren sortiert, halb durchsichtig)
//#HEADER
var all = UnityEngine.Object.FindObjectsByType<Reborn.World.BlobShadow>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
int player = 0, slime = 0, show = 0, bad = 0;
foreach (var b in all)
{
    var sr = b.GetComponent<UnityEngine.SpriteRenderer>();
    if (sr.sprite == null || sr.sortingOrder != -1) bad++;
    if (b.gameObject.activeInHierarchy && UnityEngine.Mathf.Abs(sr.color.a - 0.35f) > 0.02f) bad++;
    if (b.transform.parent == p.transform) player++;
    else if (b.GetComponentInParent<Reborn.Enemies.SlimeAI>(true) != null) slime++;
    else if (b.transform.parent != null && b.transform.parent.name.StartsWith("Showcase_")) show++;
}
return "Spieler=" + player + " Schleime=" + slime + " Schaufenster=" + show + " Fehler=" + bad;
