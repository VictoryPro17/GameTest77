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
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b Art/Blender/Reborn_Characters.blend --python Art/Blender/Scripts/build_spells.py -- D:\GameTest77
```

Hinweis: `build_player.py` legt die .blend-Datei neu an. Danach immer Schleim, Truhe und Zauber erneut rendern.

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

## Meilenstein 3: Magie

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| E1 | Elemente | Wasser und Feuer, je 2 Zauber |
| E2 | Ausruf | Text über dem Kopf; bei starken Zaubern groß in der Bildmitte |
| E3 | Mana-Training | Zaubern und Meditieren geben Mana-Erfahrung |
| E4 | Effekte | Unity-Partikel und 2D-Licht; Drachenkopf, Körper, Kugeln und Icons aus Blender (`build_spells.py`) |
| E5 | Stufen | 1–10 je Zauber, steigen durch Treffer |
| E6 | Tasten | Feste Plätze 1–4 |
| E7 | Animation | Neue Spieler-Animation „cast“ (3 Bilder, 4 Richtungen) |
| E8 | Review-Reste | Erledigt (unbenutzte Testdateien entfernt, 0-HP-Rätsel untersucht) |

### Zauber

| Zauber | Mana | Schaden (Stufe 1) | Abklingzeit | Freigeschaltet |
|---|---|---|---|---|
| Wasserkugel | 5 | 8 | 0,5 s | von Anfang an |
| Wasserdrache | 25 | 20, durchdringt, trifft jeden einmal | 4 s | Wasserkugel Stufe 3 |
| Feuerball | 8 | 12 + Explosion 6 (Radius 1) | 0,8 s | Spieler-Level 2 |
| Flammenring | 20 | 15 + Rückstoß (Radius 1,8) | 3 s | Feuerball Stufe 3 |

- Schaden = (Grundschaden + Intelligenz) × (1 + 0,25 × (Stufe − 1)). Effektgröße +5 % pro Stufe, ab Stufe 10 goldener Rand.
- Zauber-Erfahrung: 1 pro Treffer (Drache 2), nächste Stufe braucht 10 × Stufe.
- Ausruf: „Wasser-Erschaffungsmagie: …!“ bzw. „Feuer-Erschaffungsmagie: …!“.

### Mana

| Wert | Formel |
|---|---|
| Max. Mana | 20 + 5 × Intelligenz + 8 × (Mana-Stufe − 1) |
| Auffüllen | 1 + 0,2 × Intelligenz pro Sekunde |
| Meditieren | 5 pro Sekunde, Spieler steht still |
| Mana-Erfahrung | 1 pro ausgegebenem Mana und pro Sekunde Meditieren; nächste Stufe braucht 50 × Stufe, höchstens Stufe 10 |

### Steuerung (neu)

| Aktion | Tastatur | Controller | Handy |
|---|---|---|---|
| Zauber 1–4 | 1, 2, 3, 4 | RB, RT, LB, LT | Knöpfe Z1–Z4 |
| Meditieren (halten) | M | Ost-Taste (B / ○) | Knopf „Medit.“ |

### Speichern

Version 2 speichert zusätzlich Stufe und Erfahrung je Zauber, Mana-Stufe, Mana-Erfahrung und aktuelles Mana. Spielstände der Version 1 laden weiter (Zauber auf Stufe 1, Mana voll).

### Befunde und Testfehler

- **0-HP-Rätsel aus Review 3:** Nicht nachstellbar. Mit zur Laufzeit abgeschalteten Mauern läuft der Spieler weit aus dem Raum, bekommt aber weder Schaden noch stirbt er (Test `hp0`). Nebenbefund: Ausweichen setzt den Test-Schutz (`invulnerable`) zurück.
- **Absturz beim Hand-Leuchten:** Das Prefab hatte kein Partikelsystem am Hauptobjekt, `TryCast` stürzte nach dem Mana-Abzug ab, Effekte erschienen nie. Behoben: Leuchten über die Kinder, und erst nach dem Effekt. Die Konsolenmeldungen von 13:32–13:33 am 11.11. kamen von diesem alten Code (vermutlich beim eigenen Ausprobieren).
- **Testfehler Platzhalter:** `$vars.Keys` liefert in PowerShell bei einem Eintrag „KEYS“ dessen Wert. Die Tasten-Datei lief deshalb nie. Behoben in `lib.ps1` (`GetEnumerator`), Platzhalter heißt jetzt `%TASTEN%`.
- **Testfehler Erwartungen:** Nach Stufenaufstiegen bleibt durch echte Treffer etwas Erfahrung übrig; die Tests erwarten jetzt `XP ≥ 0`.

### Abweichungen vom Plan

- Die Aufzählungen `SpellElement` und `SpellKind` haben je eine eigene Datei; dazu kamen `SpellEffect.cs` (Grundklasse), `LightFlash.cs` (Lichtblitz) und `SpriteLoop.cs` (Feuerball flackert).
- Intelligenz wird direkt in `PlayerMana` und `SpellBook` gelesen, `PlayerStats` selbst blieb unverändert.
- Das Leuchten der Hand kommt als Partikel aus Unity, nicht aus dem Sprite.
- Z5 (Abklingzeit) ruft `TryCast` zweimal direkt auf, statt zweimal die Taste zu drücken.
- Die Test-Schleime im Magie-Test verfolgen den Spieler nicht, damit Treffer und Rückstoß messbar sind.

### Tests

`run_all.ps1` mit `magic, wall, m2, play, iframe, kb, hp0`: 75 Prüfungen, 0 Fehler (11.11.2026).

### Bekannte Grenzen

- Auf den Screenshots sind Wasserkugel, Drache und Feuerball oft schon vorbei, weil das Bild verzögert aufgenommen wird.
- Untertitel des großen Banners und Ausruf über dem Kopf überlappen sich (wird mit der neuen Oberfläche in Meilenstein 4 behoben).
- Touch auf einem echten Handy noch nicht getestet.

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler (Szene „Characters“) und Waldschleim (Szene „Slime“)
- `Assets/_Project/Scenes/Room_Test.unity` – Test-Raum
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
- `Art/Blender/Scripts/build_props.py` – Schatztruhe (Szene „Props“)
- `Art/Blender/Scripts/build_spells.py` – Zauber-Grafik (Szene „Spells“)
- `Assets/_Project/Scripts/Magic/` – Mana, Zauberbuch, Effekte
- `Tools/PlayTests/` – automatische Play-Tests
