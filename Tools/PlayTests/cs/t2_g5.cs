//#HEADER
int lv0 = prog.Level;
prog.AddXP(100);
int pts = st.UnspentPoints, max0 = hp.maxHealth;
bool ok = st.AddPoint(Reborn.Progression.PlayerAttribute.Vitalitaet);
return "Level " + lv0 + "->" + prog.Level + " freie Punkte " + pts + " | Vitalitaet+1=" + ok + " max. Leben " + max0 + "->" + hp.maxHealth + " Punkte jetzt " + st.UnspentPoints;
