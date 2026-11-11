// Stellt den Spieler links neben den naechsten lebenden Schleim und drueckt Stick rechts + Angriffstaste (West)
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
Reborn.Enemies.SlimeAI target = null;
foreach (var s in UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>())
{
    if (!s.GetComponent<Reborn.Combat.Health>().IsDead) { target = s; break; }
}
var hp = p.GetComponent<Reborn.Combat.Health>();
hp.invulnerable = true; // nur fuer den Test
if (target == null) return "keine Schleime mehr";
var rb = p.GetComponent<UnityEngine.Rigidbody2D>();
rb.position = (UnityEngine.Vector2)target.transform.position + new UnityEngine.Vector2(-0.6f, 0f);
p.transform.position = rb.position;
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
if (pad == null) pad = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>("TestPad");
var state = new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(0.3f, 0f) }
    .WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.West);
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, state);
return "Angriff auf " + target.name + " hp=" + target.GetComponent<Reborn.Combat.Health>().Current;
