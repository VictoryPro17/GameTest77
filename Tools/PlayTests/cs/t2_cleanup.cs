// Test-Geraete und Test-Werte entfernen
foreach (var n in new[] { "TestPad", "TestKeyboard" })
{
    var d = UnityEngine.InputSystem.InputSystem.GetDevice(n);
    if (d != null) UnityEngine.InputSystem.InputSystem.RemoveDevice(d);
}
foreach (var k in new[] { "_t_dmg", "_t_pot", "_t_sub", "_t_hits", "_t_x" }) UnityEngine.PlayerPrefs.DeleteKey(k);
return "entfernt";
