// Nur fuer den Test: Spieler unverwundbar machen, damit 5 Schleime ihn nicht sofort besiegen
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var hp = p.GetComponent<Reborn.Combat.Health>();
hp.invulnerable = true;
// Nur fuer den Test: Tasten zaehlen auch, wenn das Game-Fenster keinen Fokus hat (wird danach zurueckgesetzt)
UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
return "Test-Schutz an, HP " + hp.Current + "/" + hp.maxHealth;
