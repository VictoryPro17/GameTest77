// Meilenstein 10a: Standard-Einstellungen (Deutsch, Standard-Tasten) nur fuer diese Test-Sitzung - deine Einstellungen bleiben unberuehrt
Reborn.Core.GameSettings.UseSessionDefaults();
return "Einstellungen: Standard (Sitzung) Sprache=" + Reborn.Core.GameSettings.Language + " gespeichert=" + !Reborn.Core.GameSettings.SessionDefaults;
