// Oeffnet vor dem Play-Start die gewuenschte Szene (Meilenstein 6: Base oder Room_Test), nur wenn noetig
var path = "Assets/_Project/Scenes/%SCENE%.unity";
var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (active.path == path) return "Szene schon offen: %SCENE%";
UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
return "Szene geoeffnet: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
