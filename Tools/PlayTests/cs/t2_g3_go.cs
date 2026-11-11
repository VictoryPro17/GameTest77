//#HEADER
eq.Equip(inv.IndexOf(db.Get("holzschild")));
foreach (var id in new[] { "lederkappe", "lederhose", "lederstiefel" }) { inv.Add(db.Get(id), 1); eq.Equip(inv.IndexOf(db.Get(id))); }
UnityEngine.PlayerPrefs.DeleteKey("_t_dmg");
hp.OnDamaged += (amount, k) => UnityEngine.PlayerPrefs.SetInt("_t_dmg", amount);
hp.invulnerable = false;
Reborn.Enemies.SlimeAI slime = null;
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsInactive.Include)) { slime = sl; break; }
slime.transform.position = p.transform.position + new UnityEngine.Vector3(0.6f, 0f, 0f);
slime.gameObject.SetActive(true);
return "Verteidigung=" + st.Defense + " Schleim aktiv, HP " + hp.Current;
