//#HEADER
var book = p.GetComponent<Reborn.Magic.SpellBook>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var chant = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.ChantText>();
var banner = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.CastBanner>();
int effects = UnityEngine.Object.FindObjectsByType<Reborn.Magic.SpellEffect>().Length;
return "Mana=" + UnityEngine.Mathf.FloorToInt(mana.Current) + " Effekte=" + effects + " Zauber=" + book.CastCount + " LastFail=" + book.LastFail + " Ausruf=" + chant.CurrentText + " | Banner=" + banner.CurrentText;
