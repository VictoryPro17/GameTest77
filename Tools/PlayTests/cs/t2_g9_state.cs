//#HEADER
var move = p.GetComponent<Reborn.Player.PlayerController>().actions.FindAction("Gameplay/Move");
return "Datei vorhanden=" + System.IO.File.Exists(Reborn.Save.SaveSystem.SavePath) + " Menue offen=" + Reborn.UI.GameMenu.IsOpen + " timeScale=" + UnityEngine.Time.timeScale.ToString("F0", ic) + " MoveAktiv=" + move.enabled;
