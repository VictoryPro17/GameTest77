//#HEADER
inv.Clear();
eq.ClearAll();
st.SetState(new[] { 1, 1, 1, 1, 1 }, 3);
st.Recalculate(true);
inv.SetSlot(0, db.Get("lederkappe"), 1);
inv.SetSlot(1, db.Get("eisenschwert"), 1);
inv.Add(db.Get("schleimgel"), 7);
inv.Add(db.Get("heiltrank_klein"), 3);
inv.Add(db.Get("kupferring"), 1);
inv.NotifyChanged();
// Zaehlt mit, wie oft ein Platz eine Bestaetigung (Enter / Sued-Taste) bekommt
UnityEngine.PlayerPrefs.SetInt("_t_sub", 0);
foreach (var v in UnityEngine.Object.FindObjectsByType<Reborn.UI.ItemSlotView>(UnityEngine.FindObjectsInactive.Include))
    v.Submitted += view => UnityEngine.PlayerPrefs.SetInt("_t_sub", UnityEngine.PlayerPrefs.GetInt("_t_sub", 0) + 1);
return "vorbereitet: Platz0=Lederkappe Platz1=Eisenschwert Platz2=Schleimgel Platz3=Trank Platz4=Ring, Angriff " + st.Attack + ", frei " + st.UnspentPoints;
