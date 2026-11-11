var s = UnityEngine.InputSystem.InputSystem.settings;
s.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.PointersAndKeyboardsRespectGameViewFocus;
s.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.ResetAndDisableNonBackgroundDevices;
return s.editorInputBehaviorInPlayMode + " | " + s.backgroundBehavior + " | playing=" + UnityEditor.EditorApplication.isPlaying;
