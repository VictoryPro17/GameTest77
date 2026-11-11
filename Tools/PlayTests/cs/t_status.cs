// Tasten loslassen und Zustand melden
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
if (pad != null) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState());
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var prog = p.GetComponent<Reborn.Progression.PlayerProgression>();
var inv = p.GetComponent<Reborn.Inventory.PlayerInventory>();
var hp = p.GetComponent<Reborn.Combat.Health>();
var sb = new System.Text.StringBuilder();
sb.Append("Lv " + prog.Level + " XP " + prog.XP + "/" + prog.XPToNextLevel + " HP " + hp.Current + "/" + hp.maxHealth);
sb.Append(" | Schleime: ");
foreach (var s in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>()) sb.Append(s.GetComponent<Reborn.Combat.Health>().Current + " ");
sb.Append("| Beute am Boden: " + UnityEngine.Object.FindObjectsByType<Reborn.Loot.Pickup>().Length);
sb.Append(" | Inventar: ");
foreach (var sl in inv.slots) if (!sl.IsEmpty) sb.Append(sl.item.itemName + " x" + sl.count + "; ");
return sb.ToString();
