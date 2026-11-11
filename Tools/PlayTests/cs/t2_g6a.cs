//#HEADER
inv.Clear();
int rest = inv.Add(db.Get("schleimgel"), 130);
return "130 Schleimgel: Rest=" + rest + " Platz0=" + inv.slots[0].count + " Platz1=" + inv.slots[1].count + " Platz2leer=" + inv.slots[2].IsEmpty;
