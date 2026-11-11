//#HEADER
var wpn = eq.Get(5);
return "Hauptwaffe=" + (wpn != null ? wpn.id : "-") + " Angriff=" + st.Attack + " Platz1=" + (inv.slots[1].IsEmpty ? "leer" : inv.slots[1].item.id) + " Bestaetigungen=" + UnityEngine.PlayerPrefs.GetInt("_t_sub", 0);
