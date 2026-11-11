# Play-Tests für RE:BORN

Automatische Tests, die den **offenen Unity-Editor** über die Unity CLI steuern: Play-Modus starten, virtuelle Tastatur und Controller drücken, Werte abfragen, Screenshots machen.

## Starten

Das Projekt muss in Unity geöffnet sein, der Play-Modus aus. Während der Tests (etwa 2–3 Minuten) Unity bitte nicht bedienen.

```
powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1
powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1 -Tests wall,m2
```

Starte die Tests immer über `run_all.ps1`. Es sichert vorher deinen Spielstand mit Prüfsumme und spielt ihn danach zurück, auch wenn ein Test abbricht. Es setzt außerdem die Eingabe-Einstellungen zurück und entfernt die Test-Geräte. Die Ausgabe landet auch in `%TEMP%\reborn_playtests\last_run.log`.

## Tests

| Name | Datei | Prüft |
|---|---|---|
| `wall` | `walltest.ps1` | Mauer-Collider, Laufen und Ausweichen gegen alle vier Mauern, Schleim drückt gegen die Mauer |
| `m2` | `m2test.ps1` | Truhe, Ausrüstung, Verteidigung, Trank, Attribute, Inventar, Speichern, Menü mit Controller und Tastatur, Spielstand löschen |
| `play` | `playtest.ps1` | Laufen, Kampf gegen alle 5 Schleime, Beute, Level-Aufstieg |
| `iframe` | `iframetest.ps1` | Schutzzeit nach einem Treffer |
| `kb` | `kbtest.ps1` | Tastatur: D, J, Leertaste |

Jede Prüfung schreibt `OK` oder `FEHLER`, jeder Test am Ende `BESTANDEN` oder `NICHT BESTANDEN`. Screenshots landen in `Screenshots/`.

## Über den Plan hinaus (Review 4)

Diese Teile der Testwerkzeuge standen nicht im Korrektur-Plan 4 und wurden beim Bau ergänzt:

- `lib.ps1`: zusätzlich `CheckRange`, `CheckTrue`, `RequireSession`, `TestResult`, `WaitReady` und Platzhalter in `Eval`. `StartPlay` hält den Editor auch ohne Fokus am Laufen (`set_auto_tick`, `run_in_background`) und prüft, ob die Frames wirklich laufen. `Shot` übernimmt das Seitenverhältnis der Game-Ansicht.
- `run_all.ps1`: bricht ab, wenn schon eine alte Sicherung liegt (statt sie zu übergehen) oder Unity im Play-Modus ist; schreibt ein Log; setzt am Ende die Editor-Einstellungen von `StartPlay` zurück.
- `t_release.cs` und `t_kb_release.cs` geben Zahlen mit Punkt aus; die Regressionstests prüfen selbst mit OK/FEHLER.
- Zusatzprüfungen: W4 „Schleim hat wirklich getroffen“, Bestätigungs-Zähler und G10 im M2-Test.
- `unity recompile` kann nach 120 s abbrechen, obwohl der Code geladen ist. Prüfen, ob die neuen Klassen da sind, statt sich auf den Rückgabewert zu verlassen.

## Aufbau

- `lib.ps1`: gemeinsame Funktionen (`Eval`, `StartPlay`, `StopPlay`, `Shot`, `Check` …)
- `cs/`: kleine C#-Schnipsel, die im Editor ausgeführt werden. Die Markierung `//#HEADER` ersetzt `lib.ps1` durch `cs/_header.cs`. Platzhalter wie `%PX%` füllt `Eval 'datei.cs' @{ PX = '1' }`. Platzhalter nie `Keys`, `Values` oder `Count` nennen: Das sind Eigenschaften von PowerShell-Tabellen, und genau so ist der erste Magie-Test ins Leere gelaufen.
- Testskripte bleiben reines ASCII (ohne BOM liest Windows PowerShell 5.1 Umlaute und Sonderzeichen falsch). Sonderzeichen in Erwartungen als `·` usw. schreiben.
