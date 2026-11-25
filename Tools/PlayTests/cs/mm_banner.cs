// Ortsname und "Gespeichert": state | save
//#HEADER
var lb = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.LocationBanner>(UnityEngine.FindObjectsInactive.Include);
if (lb == null) return "kein Banner";
if ("%ACT%" == "save") p.GetComponent<Reborn.Save.SaveSystem>().Save();
return "Ort=" + lb.CurrentTitle + " Gespeichert=" + lb.SavedVisible + " Szene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
