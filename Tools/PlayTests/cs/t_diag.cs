var p = UnityEngine.GameObject.FindGameObjectWithTag("Player");
var pc = p.GetComponent<Reborn.Player.PlayerController>();
var move = pc.actions.FindAction("Gameplay/Move");
var pad = UnityEngine.InputSystem.InputSystem.GetDevice("TestPad") as UnityEngine.InputSystem.Gamepad;
return "frame=" + UnityEngine.Time.frameCount + " t=" + UnityEngine.Time.time.ToString("F2") + " scale=" + UnityEngine.Time.timeScale
    + " mapEnabled=" + move.actionMap.enabled + " move=" + move.ReadValue<UnityEngine.Vector2>()
    + " pad=" + (pad != null ? pad.leftStick.ReadValue().ToString() : "none")
    + " devices=" + UnityEngine.InputSystem.InputSystem.devices.Count
    + " behavior=" + UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode
    + " bgBehavior=" + UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior
    + " controls=" + string.Join(",", System.Linq.Enumerable.Select(move.controls, c => c.path))
    + " pos=" + p.transform.position;
