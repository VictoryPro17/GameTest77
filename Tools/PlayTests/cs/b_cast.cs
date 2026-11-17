// B2: In der Basis keine Zauber
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var book = p.GetComponent<Reborn.Magic.SpellBook>();
bool ok = book.TryCast(0);
return "gezaubert=" + ok + " Grund=" + book.LastFail;