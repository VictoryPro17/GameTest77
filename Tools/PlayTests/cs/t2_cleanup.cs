// Test-Geraete und Test-Werte entfernen
foreach (var n in new[] { "TestPad", "TestKeyboard" })
{
    var d = UnityEngine.InputSystem.InputSystem.GetDevice(n);
    if (d != null) UnityEngine.InputSystem.InputSystem.RemoveDevice(d);
}
foreach (var k in new[] { "_t_dmg", "_t_pot", "_t_sub", "_t_hits", "_t_x", "_t_mismatch", "_t_frames_idle", "_t_frames_walk", "_t_frames_attack", "_t_frames_dodge", "_t_frames_cast" }) UnityEngine.PlayerPrefs.DeleteKey(k);
return "entfernt";
