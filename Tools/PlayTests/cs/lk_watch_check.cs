// Zaehler der Bild-Beobachtung auslesen
return "walk=" + UnityEngine.PlayerPrefs.GetInt("_t_frames_walk", 0) + " attack=" + UnityEngine.PlayerPrefs.GetInt("_t_frames_attack", 0)
    + " cast=" + UnityEngine.PlayerPrefs.GetInt("_t_frames_cast", 0) + " Abweichungen=" + UnityEngine.PlayerPrefs.GetInt("_t_mismatch", 0);
