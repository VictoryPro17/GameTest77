# RE:BORN – Echoes of Eternity: Projektentscheidungen

Stand: 2026-11-05 · Meilenstein 1 („Mini-Spiel“)

## Getroffene Entscheidungen

| # | Thema | Entscheidung | Abweichung vom Design-Dokument v1.0 |
|---|---|---|---|
| E1 | Engine | **Unity 6 (6000.6.8f1), C#, URP 2D** | Dokument empfahl Godot 4 / GDScript |
| E2 | Kamera | **Top-Down** (Kamera schräg von oben, 4 Blickrichtungen) | Dokument empfahl Side-Scrolling |
| A1 | Grafik-Pipeline | **3D in Blender → 64×64-Pixel-Sprites** in 4 Richtungen | – |
| A2 | Wer macht Grafik | **Claude baut Platzhalter per Skript**, später ersetzbar | – |
| A3 | Handy | **Android zuerst** (iPhone braucht einen Mac) | – |
| A4 | Touch-Steuerung | Joystick links, Knöpfe **Angriff** + **Ausweichen** rechts | – |
| A5 | Git | Arbeit auf Branch `feature/meilenstein-1` | – |

## Steuerung

| Aktion | Tastatur | Maus | Controller | Handy |
|---|---|---|---|---|
| Laufen | WASD / Pfeiltasten | – | linker Stick | Joystick links |
| Angriff | J | linke Taste | West-Taste (X / □) | Knopf „Angriff“ |
| Ausweichen | Leertaste / Shift | – | Süd-Taste (A / ✕) | Knopf „Ausw.“ |

## Grafik neu erzeugen (Blender 5.2)

```
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b --factory-startup --python Art/Blender/Scripts/build_player.py -- D:\GameTest77
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b Art/Blender/Reborn_Characters.blend --python Art/Blender/Scripts/build_slime.py -- D:\GameTest77
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b Art/Blender/Reborn_Characters.blend --python Art/Blender/Scripts/build_props.py -- D:\GameTest77
```

Danach in Unity: Menü **Reborn → Spiel neu bauen** (baut Sprites-Import, Icons, Daten, Prefabs, Menü und die Szene `Room_Test` neu).

## Abweichungen vom Plan (Review 1)

| # | Abweichung | Entscheidung |
|---|---|---|
| D1 | Spieler-Modell mit Haaren, Armen, Beinen und Schwert statt Kapsel | behalten |
| D2 | Freestyle-Umrisslinien | **entfernt** (in beiden Blender-Skripten) |
| D3 | Pivot an den Füßen (Spieler 0,5/0,3 · Schleim 0,5/0,41) | behalten, damit die Y-Sortierung stimmt |
| D4 | Treffer-Aufblitzen rot statt weiß | behalten (weiß bräuchte einen eigenen Shader) |
| D5 | Spieler-Collider Radius 0,2 statt 0,3 | behalten |
| D6 | Trefferkreis 0,55 / 0,45 statt 0,7 / 0,6 | behalten, Skript-Standardwerte angepasst |
| D7 | Zusätzliche Skript-Funktionen (Rückstoß, Bewegungs-Sperre usw.) | behalten |
| D8 | Zusätzliche Dateien (RebornBuilder, Item-Icons, Wand-Kachel) | behalten |
| D9 | Sortierachse (0,1,0) und Polygons-Collider an der Wand | behalten |
| D10 | Neues Global Light 2D in Room_Test | behalten |
| D11 | Selbst gewählte Werte (siehe Tabelle unten) | behalten, hier festgehalten |
| D12 | Erster Commit enthielt Änderungen, die schon vorher da waren (ProjectSettings, Build-Liste ohne SampleScene) | so gelassen |
| D13 | Tastatur/Touch nur teilweise getestet | nachgetestet im Editor mit virtuellem Controller und virtueller Tastatur (D, J, Leertaste); Touch auf echtem Handy steht noch aus |

Spielprobleme aus Review 1:
- **Keine Schutzzeit nach einem Treffer:** `Health.hitInvulnerability` eingeführt, beim Spieler 0,6 s.
- **Schleim greift sofort an:** Startposition (0,4) → (4,−4).
- **Figuren zu klein:** Kamera `orthographicSize` 6 → 3,5.
- **Schutzzeit-Test:** höchstens 6 Treffer (30 HP) in 3 s, weil der erste Treffer bei 0 s zählt. Gemessen mit 5 Schleimen direkt am Spieler: 5 Treffer in 3 s, kleinster Abstand 0,600 s, 25 HP Verlust, Spieler lebt.

## Werte (Stand Meilenstein 1)

| Wert | Spieler | Waldschleim |
|---|---|---|
| HP | 50 (+10 pro Level) | 30 |
| Schutzzeit nach Treffer | 0,6 s | 0 s |
| Schaden | 10 pro Schlag (Abklingzeit 0,4 s) | 5 bei Berührung (Abklingzeit 1 s) |
| Trefferkreis | Abstand 0,55 · Radius 0,45 | – |
| Collider-Radius | 0,2 | 0,22 |
| Tempo | 5 (Ausweichen 12 für 0,2 s) | 2, erkennt den Spieler ab 5 |
| XP | 100 × Level bis zum nächsten Level | gibt 30 XP |
| Animations-fps | idle 3 · walk 8 · attack 12 · dodge 15 | idle 6 · hurt 8 · death 10 |
| Kamera | orthographicSize 3,5 · Sprites 64 px pro Einheit | |

**Richtigstellung zu D9 (Stand Meilenstein 2):** Die Collider-Art „Polygons“ war nicht der Grund, warum der Spieler durch die Mauer gedrückt wurde. Der Builder hat die Szene gespeichert, bevor Unity die Mauer-Geometrie berechnet hatte. Der Mauer-Collider war deshalb seit Meilenstein 1 leer, und **die Mauern hielten nichts auf**. Ein Mauer-Test vor der Korrektur hat das bestätigt: Der Spieler lief bis x = −12,2 hinaus. Behoben in Meilenstein 2 (siehe unten).

## Meilenstein 2: Charakter & Ausrüstung

Umfang: GDD-Phasen 3 und 4 (Inventar, Gegenstände, Tränke, Level und Ausrüstung) plus Speichern. Räume, Boss, weitere Monster sowie Tod und Wiederbelebung folgen in Meilenstein 3.

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| M1 | Umfang | Inventar, Ausrüstung, Attribute, Tränke, Speichern |
| M2 | Item-Icons | Pixel-Icons per Code (RebornBuilder), später ersetzbar |
| M3 | Schatztruhe | In Blender gebaut (zu/offen, 64 px), öffnet sich beim Berühren |
| M4 | Ausrüstung an der Figur | Nicht sichtbar, wirkt nur auf die Werte |
| M5 | Intelligenz | Attribut vorhanden, wirkt erst ab Fähigkeiten |
| M6 | Git | Branch `feature/meilenstein-2` |
| N1 | Testskripte | Im Projekt unter `Tools/PlayTests/` |
| N2 | HUD | Zeigt „+N Punkte“, wenn Attributpunkte frei sind |
| N3 | Builder-Menü | „Reborn → Spiel neu bauen“ |

### Gegenstände

| ID | Name | Platz | Seltenheit | Werte | Verkauf | Stapel |
|---|---|---|---|---|---|---|
| `schleimgel` | Schleimgel | Material | Gewöhnlich | – | 2 | 99 |
| `heiltrank_klein` | Kleiner Heiltrank | Verbrauch | Gewöhnlich | heilt 20 | 5 | 10 |
| `heiltrank` | Heiltrank | Verbrauch | Selten | heilt 40 | 15 | 10 |
| `holzschwert` | Holzschwert | Hauptwaffe | Gewöhnlich | Angriff +3 | 10 | 1 |
| `eisenschwert` | Eisenschwert | Hauptwaffe | Selten | Angriff +6 | 40 | 1 |
| `holzschild` | Holzschild | Nebenhand | Gewöhnlich | Verteidigung +2 | 10 | 1 |
| `lederkappe` | Lederkappe | Helm | Gewöhnlich | Verteidigung +1 | 6 | 1 |
| `lederweste` | Lederweste | Brust | Gewöhnlich | Verteidigung +2, Leben +5 | 12 | 1 |
| `lederhose` | Lederhose | Hose | Gewöhnlich | Verteidigung +1 | 8 | 1 |
| `lederstiefel` | Lederstiefel | Schuhe | Gewöhnlich | Verteidigung +1, Geschick +1 | 8 | 1 |
| `kupferring` | Kupferring | Ring | Selten | Glück +2 | 25 | 1 |
| `wolfszahnkette` | Wolfszahnkette | Halskette | Episch | Stärke +2, Angriff +1 | 60 | 1 |

Beute: Schleim gibt Schleimgel 100 % (1–2), Kleiner Heiltrank 25 %, Lederkappe, Lederhose und Lederstiefel je 6 %, Kupferring 3 %. Die Truhe bei (−8, 0) enthält Holzschwert, Lederweste, Holzschild und 2× Kleiner Heiltrank. Eisenschwert, Heiltrank und Wolfszahnkette haben noch keine Quelle (Meilenstein 3).

### Formeln (alle Attribute starten bei 1, pro Level-Aufstieg 3 Punkte)

| Wert | Formel |
|---|---|
| Max. Leben | 45 + 5 × Vitalität + 10 × (Level − 1) + Leben aus Ausrüstung |
| Angriff | 9 + Stärke + Angriff aus Ausrüstung |
| Verteidigung | Summe aus Ausrüstung |
| Erlittener Schaden | max(1, round(Schaden × 20 / (20 + Verteidigung))) |
| Angriffs-Abklingzeit | 0,4 × (1 − 0,02 × Geschick), mindestens 0,25 s |
| Ausweich-Abklingzeit | 0,6 × (1 − 0,02 × Geschick), mindestens 0,4 s |
| Beute-Chance | Basis × (1 + 0,03 × Glück) |

### Steuerung (neu)

| Aktion | Tastatur | Controller | Handy |
|---|---|---|---|
| Menü | Tab, I, Esc | Start | Knopf „Menü“ oben rechts, im Menü „X“ |
| Trank | Q | Nord-Taste (Y / △) | Knopf „Trank“ |
| Im Menü wählen | Pfeiltasten | Steuerkreuz / Stick | Antippen |
| Anlegen / Benutzen / Ablegen | Enter | Süd-Taste | Knopf im Detail-Fenster |

### Speichern

- Datei: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\GameTest77\reborn_save.json`
- Laden beim Start der Szene. Speichern bei Level-Aufstieg, beim Schließen des Menüs, beim Öffnen einer Truhe, wenn die App pausiert (Handy) und beim Beenden.
- „Spielstand löschen“ im Reiter Charakter startet das Spiel mit Startwerten neu.
- Schleime erscheinen bei jedem Start neu. Raum-Fortschritt folgt in Meilenstein 3.

### Korrekturen aus Review 3

- Mauern: Der Builder erzeugt die Mauer-Geometrie vor dem Speichern und bricht ab, wenn sie leer ist. Zusätzlich prüft `WallColliderGuard` beim Spielstart.
- Menü: skaliert mit „Expand“ und passt auf jedes Seitenverhältnis. „Sortieren“ und das Detail-Fenster überlappen nicht mehr.
- Nach „Anlegen“ oder „Benutzen“ über den Knopf im Detail-Fenster ist wieder der Platz ausgewählt. Beim Schließen des Menüs verschwindet der Auswahlrahmen.
- „Spielstand löschen“ bei offenem Menü gibt Laufen, Angriff, Ausweichen und Trank wieder frei. Ein Mausklick ins Leere löscht die Auswahl nicht mehr.
- Tränke: Die Beschreibung wiederholt die Heilung nicht mehr.

### Abweichungen vom Plan

- 10 statt 11 neue Icons: 2 der 12 Gegenstände hatten schon eines.
- Zusätzlicher Knopf „X“ im Menü: Ohne ihn käme man auf dem Handy nicht aus dem Menü, weil das Menü den Knopf „Menü“ verdeckt.
- Kamera der Truhe in Blender: Ausschnitt 1,6 und Höhe 0,42, weil der offene Deckel sonst abgeschnitten war.
- Der erste Testlauf meldete bei G8, die Süd-Taste lege nicht an. Ursache war der Test: Er hielt das Steuerkreuz so lange, dass die Menü-Wiederholung die Auswahl weiterschob. Der korrigierte Test besteht.

### Tests

Alle Tests laufen im offenen Editor und schützen deinen Spielstand (Sicherung mit Prüfsumme):

```
powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1
```

Ergebnis am 11.11.2026: 49 Prüfungen, 0 Fehler (Mauern, Meilenstein 2, Spieltest, Schutzzeit, Tastatur). Details in `Tools/PlayTests/README.md`.

### Bekannte Grenzen

- Maus-Klicks im Menü werden nicht automatisch getestet, nur Controller und Tastatur.
- Touch ist auf einem echten Handy noch nicht getestet.
- Das Fenster „Wirklich löschen?“ sperrt die Knöpfe dahinter nicht.
- Kein Tod und keine Wiederbelebung (Meilenstein 3).

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler (Szene „Characters“) und Waldschleim (Szene „Slime“)
- `Assets/_Project/Scenes/Room_Test.unity` – Test-Raum
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
- `Art/Blender/Scripts/build_props.py` – Schatztruhe (Szene „Props“)
- `Tools/PlayTests/` – automatische Play-Tests
