// Schadenszahl bei einem Treffer an einem Schleim
//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
Reborn.Enemies.SlimeAI slime = null;
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include)) { slime = sl; break; }
slime.gameObject.SetActive(true);
slime.detectRadius = 0f;
slime.transform.position = new UnityEngine.Vector3(2f, 0f, 0f);
var numbers = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.DamageNumbers>();
slime.GetComponent<Reborn.Combat.Health>().TakeDamage(7, UnityEngine.Vector2.zero);
var r = "Zahl=" + numbers.LastText + " sichtbar=" + numbers.ActiveCount;
slime.gameObject.SetActive(false);
return r;
