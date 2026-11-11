// HP und Position der Test-Schleime (Slime_T0, T1 ...)
var ic = System.Globalization.CultureInfo.InvariantCulture;
var hps = new System.Collections.Generic.List<string>();
var dist = new System.Collections.Generic.List<string>();
for (int i = 0; i < 5; i++)
{
    var go = UnityEngine.GameObject.Find("Slime_T" + i);
    if (go == null) break;
    hps.Add(go.GetComponent<Reborn.Combat.Health>().Current.ToString());
    dist.Add(((UnityEngine.Vector2)go.transform.position).magnitude.ToString("F2", ic));
}
return "Schleime=" + string.Join(",", hps) + " Abstand=" + string.Join(",", dist);
