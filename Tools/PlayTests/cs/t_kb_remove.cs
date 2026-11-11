var kb = UnityEngine.InputSystem.InputSystem.GetDevice("TestKeyboard");
if (kb != null) UnityEngine.InputSystem.InputSystem.RemoveDevice(kb);
UnityEngine.PlayerPrefs.DeleteKey("_t_x");
return "TestKeyboard entfernt";
