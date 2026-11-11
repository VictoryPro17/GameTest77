// Gemeinsamer Kopf: lib.ps1 setzt ihn an der Kopf-Markierung eines Schnipsels ein.
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var inv = p.GetComponent<Reborn.Inventory.PlayerInventory>();
var eq = p.GetComponent<Reborn.Inventory.PlayerEquipment>();
var st = p.GetComponent<Reborn.Progression.PlayerStats>();
var hp = p.GetComponent<Reborn.Combat.Health>();
var prog = p.GetComponent<Reborn.Progression.PlayerProgression>();
var db = p.GetComponent<Reborn.Save.SaveSystem>().database;
var rb = p.GetComponent<UnityEngine.Rigidbody2D>();
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard") as UnityEngine.InputSystem.Keyboard;
var ic = System.Globalization.CultureInfo.InvariantCulture;
var es = UnityEngine.EventSystems.EventSystem.current;
