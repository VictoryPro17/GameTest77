//#HEADER
var vis = p.GetComponentInChildren<Reborn.Animation.EquipmentVisuals>();
var anim = p.GetComponentInChildren<Reborn.Animation.DirectionalSpriteAnimator>();
var bodySr = anim.GetComponent<UnityEngine.SpriteRenderer>();
var mana = p.GetComponent<Reborn.Magic.PlayerMana>();
var chest = chestById("room_test_truhe_2");
inv.Clear();
rb.position = (UnityEngine.Vector2)chest.transform.position + new UnityEngine.Vector2(0f, -0.75f);
p.transform.position = rb.position;
UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad, new UnityEngine.InputSystem.LowLevel.GamepadState { leftStick = new UnityEngine.Vector2(0f, 1f) });
return "vor Truhe 2, offen=" + chest.IsOpen;
