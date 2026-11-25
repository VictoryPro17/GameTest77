using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reborn.Core
{
    /// <summary>
    /// Sprache (Meilenstein 10a): Der deutsche Text ist der Schluessel. Auf Englisch wird er ueber die Tabelle
    /// Resources/Localization/Loc_en.json uebersetzt; fehlt ein Eintrag, bleibt der deutsche Text stehen.
    /// T("Inventar") uebersetzt, F("Gold: {0}", 5) uebersetzt die Vorlage und setzt Werte ein.
    /// </summary>
    public static class Loc
    {
        public const string TablePath = "Localization/Loc_en";

        [Serializable]
        public class Entry
        {
            public string de;
            public string en;
        }

        [Serializable]
        public class Table
        {
            public List<Entry> entries = new List<Entry>();
        }

        static Dictionary<string, string> _en;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => _en = null;

        static Dictionary<string, string> English
        {
            get
            {
                if (_en != null) return _en;
                _en = new Dictionary<string, string>();
                var asset = Resources.Load<TextAsset>(TablePath);
                if (asset == null) return _en;
                try
                {
                    var table = JsonUtility.FromJson<Table>(asset.text);
                    if (table != null && table.entries != null)
                        foreach (var e in table.entries)
                            if (e != null && !string.IsNullOrEmpty(e.de) && !string.IsNullOrEmpty(e.en)) _en[e.de] = e.en;
                }
                catch (Exception ex) { Debug.LogWarning("Uebersetzung konnte nicht gelesen werden: " + ex.Message); }
                return _en;
            }
        }

        /// <summary>Anzahl englischer Eintraege (fuer Tests)</summary>
        public static int EnglishCount => English.Count;

        public static bool HasEnglish(string de) => !string.IsNullOrEmpty(de) && English.ContainsKey(de);

        public static string T(string de)
        {
            if (string.IsNullOrEmpty(de) || !GameSettings.English) return de;
            return English.TryGetValue(de, out var en) ? en : de;
        }

        public static string F(string deFormat, params object[] args)
        {
            string format = T(deFormat);
            try { return string.Format(format, args); }
            catch (FormatException) { return string.Format(deFormat, args); }
        }
    }
}
