// Hauptmenue (Meilenstein 10a): Zustand der Knoepfe, Nachfragen, Text und Spielstand. Laeuft ohne Spieler (kein Kopf).
var mm = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.MainMenu>(UnityEngine.FindObjectsInactive.Include);
var scn = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
if (mm == null) return "kein Hauptmenue Szene=" + scn;
var lbl = mm.newGameButton != null ? mm.newGameButton.GetComponentInChildren<UnityEngine.UI.Text>(true) : null;
return "Szene=" + scn
    + " Weiter=" + mm.continueButton.gameObject.activeSelf
    + " Neu=" + mm.newGameButton.gameObject.activeSelf
    + " Beenden=" + mm.quitButton.gameObject.activeSelf
    + " Version=" + mm.versionText.text
    + " Nachfrage=" + mm.confirm.IsOpen
    + " Einst=" + mm.settings.IsOpen
    + " Text=" + mm.TextOpen
    + " Titel=" + mm.textTitle.text
    + " Tutorial=" + Reborn.UI.MainMenu.TutorialRequested
    + " Spielstand=" + Reborn.Save.SaveSystem.Exists
    + " NeuLabel=" + (lbl != null ? lbl.text : "-");
