// Beweis, dass deine gespeicherten Einstellungen (PlayerPrefs) nicht veraendert werden
string s = UnityEngine.PlayerPrefs.GetString(Reborn.Core.GameSettings.PrefsKey, "");
return "Prefs=" + s.Length + "|" + s.GetHashCode() + "|" + (s.Length > 40 ? s.Substring(0, 40) : s);
