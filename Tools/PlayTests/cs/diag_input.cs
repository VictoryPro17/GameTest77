// Kommen simulierte Eingaben an? (Test "diag")
var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard") as UnityEngine.InputSystem.Keyboard;
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var move = p.GetComponent<Reborn.Player.PlayerController>().actions.FindAction("Gameplay/Move");
var ic = System.Globalization.CultureInfo.InvariantCulture;
return "D=" + (kb != null && kb.dKey.isPressed) + " stick=" + (pad != null ? pad.leftStick.ReadValue().x.ToString("F1", ic) : "-")
    + " move=" + move.ReadValue<UnityEngine.Vector2>().x.ToString("F1", ic) + " fokus=" + UnityEngine.Application.isFocused;
