// Ist die Game-Ansicht sichtbar? (Minimiertes Unity-Fenster oder verdeckter Game-Reiter: simulierte Eingaben gehen verloren)
var gvType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
var views = UnityEngine.Resources.FindObjectsOfTypeAll(gvType);
var isVisible = typeof(UnityEditor.EditorWindow).GetProperty("isVisible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
foreach (var v in views)
    if (isVisible != null && (bool)isVisible.GetValue(v)) return "sichtbar";
return "unsichtbar (Game-Ansichten: " + views.Length + ")";
