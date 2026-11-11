var p = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/_Project/Prefabs/Player.prefab");
var h = p.GetComponent<Reborn.Combat.Health>();
var a = p.GetComponent<Reborn.Combat.PlayerAttack>();
var slimes = UnityEngine.Object.FindObjectsByType<Reborn.Enemies.SlimeAI>(UnityEngine.FindObjectsSortMode.None);
var pos = "";
foreach (var s in slimes) pos += s.transform.position + ";";
return "inv=" + h.hitInvulnerability + " range=" + a.range + " radius=" + a.radius
    + " cam=" + UnityEngine.Camera.main.orthographicSize + " slimes=" + pos;
