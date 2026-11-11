// Laeuft das Spiel? Gibt die Frame-Nummer zurueck.
if (!UnityEditor.EditorApplication.isPlaying) return "kein Play-Modus";
return UnityEngine.Time.frameCount.ToString();
