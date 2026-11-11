// Tastatur-Test: Eingabe-Einstellungen fuer den Test, virtuelle Tastatur, Test-Schutz an
var s = UnityEngine.InputSystem.InputSystem.settings;
s.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
s.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard") ?? UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("TestKeyboard");
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
p.GetComponent<Reborn.Combat.Health>().invulnerable = true;
UnityEngine.PlayerPrefs.SetFloat("_t_x", p.transform.position.x);
return "Tastatur " + kb.name + " bereit, start x=" + p.transform.position.x;
