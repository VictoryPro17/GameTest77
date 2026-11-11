// Test-Vorbereitung: Eingabe auch ohne Fokus, virtuelle Geraete, Spieler geschuetzt, Schleime aus
var s = UnityEngine.InputSystem.InputSystem.settings;
s.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
s.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
if (UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") == null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>("TestPad");
if (UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard") == null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("TestKeyboard");
//#HEADER
hp.invulnerable = true;
int n = 0;
foreach (var sl in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) { sl.gameObject.SetActive(false); n++; }
// Nur im Test: eine gehaltene Richtung im Menue nicht wiederholen (sonst wandert die Auswahl weiter)
var module = UnityEngine.Object.FindAnyObjectByType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
module.moveRepeatDelay = 10f;
int used = 0;
foreach (var slot in inv.slots) if (!slot.IsEmpty) used++;
return "Setup: Schleime aus=" + n + " HP " + hp.Current + "/" + hp.maxHealth + " Angriff " + st.Attack + " Level " + prog.Level + " Inventar belegt=" + used + " Wiederholung=" + module.moveRepeatDelay.ToString("F0", ic);
