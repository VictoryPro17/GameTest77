//#HEADER
int a0 = st.Attack, h0 = hp.maxHealth, d0 = st.Defense;
bool e1 = eq.Equip(inv.IndexOf(db.Get("holzschwert")));
int a1 = st.Attack;
bool e2 = eq.Equip(inv.IndexOf(db.Get("lederweste")));
return "Schwert angelegt=" + e1 + " Angriff " + a0 + "->" + a1 + " | Weste angelegt=" + e2 + " Leben " + h0 + "->" + hp.maxHealth + " Verteidigung " + d0 + "->" + st.Defense
    + " | Hauptwaffe=" + (eq.Get(5) != null ? eq.Get(5).id : "-") + " Brust=" + (eq.Get(1) != null ? eq.Get(1).id : "-") + " Schwert noch im Inventar=" + (inv.IndexOf(db.Get("holzschwert")) >= 0);
