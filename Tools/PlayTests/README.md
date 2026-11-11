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

## Aufbau

- `lib.ps1`: gemeinsame Funktionen (`Eval`, `StartPlay`, `StopPlay`, `Shot`, `Check` …)
- `cs/`: kleine C#-Schnipsel, die im Editor ausgeführt werden. Die Markierung `//#HEADER` ersetzt `lib.ps1` durch `cs/_header.cs`. Platzhalter wie `%PX%` füllt `Eval 'datei.cs' @{ PX = '1' }`.
