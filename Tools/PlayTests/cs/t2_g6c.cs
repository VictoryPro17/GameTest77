//#HEADER
var pk = UnityEngine.GameObject.Find("TestPickupVoll");
return "Kupferring liegt noch=" + (pk != null) + " im Inventar=" + inv.Count(db.Get("kupferring")) + " Eisenschwerter=" + inv.Count(db.Get("eisenschwert"));
