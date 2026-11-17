var w = UnityEngine.Object.FindAnyObjectByType<Reborn.UI.TradeWindow>();
w.Close();
return "geschlossen offen=" + Reborn.UI.TradeWindow.IsOpen + " Zeit=" + UnityEngine.Time.timeScale.ToString("F0");