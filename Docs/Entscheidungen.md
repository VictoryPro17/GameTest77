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
"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b Art/Blender/Reborn_Characters.blend --python Art/Blender/Scripts/build_equipment.py -- D:\GameTest77
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

## Meilenstein 4: Neues Aussehen

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| U1 | Stil | Dunkles Fantasy: Stein, Bronze, Gold, Edelstein in der Seltenheitsfarbe |
| U2 | Schrift | Cinzel (Überschriften) und Alegreya Sans (Text), SIL Open Font License 1.1, Lizenztexte in `Assets/_Project/Fonts` |
| U3 | Bildgröße | Figuren, Ausrüstung, Schleim, Truhe und Zauber in 96 × 96 bei 96 Pixel pro Einheit (gleiche Weltgröße) |
| U4 | Rüstung sichtbar | Ja, als Bildebenen (siehe Technik) |
| U5 | Grundfigur | Ohne Waffe; das Schwert erscheint erst angelegt |
| U6 | Schadenszahlen | Weiß, bei Zaubern in der Elementfarbe, beim Spieler rot |
| U7 | Manatränke | Taste R / Steuerkreuz oben, eigener HUD-Platz |
| U8 | Reihenfolge | Aussehen jetzt; Dungeon, Gegner, Boss und Geister später |

### Oberfläche

- `UiTheme` (ScriptableObject) mit Schriften, Farben und 9-teiligen Rahmen. Die Rahmen erzeugt `RebornBuilder.Theme.cs` per Code (Pixel-Stil, 50 Pixel pro Einheit = doppelte Pixelgröße).
- HUD: Porträt mit Level, Leisten für Leben, Mana und Erfahrung, Aktionsleiste unten (Zauber 1–4, Heiltrank Q, Manatrank R), Ausruf, Banner, Schadenszahlen.
- Menü im gleichen Stil; die Namen der bedienbaren Teile sind gleich geblieben, damit die Tests weiterlaufen.

### Sichtbare Ausrüstung (Technik)

- `build_player.py` baut die detailliertere Grundfigur ohne Waffe. `build_equipment.py` baut 14 Teile am selben Skelett und rendert jedes Teil in allen Animationen und 4 Richtungen als eigene Bildebene (840 Bilder). Der Körper ist dabei „Holdout“: unsichtbar, verdeckt aber das Teil, damit vorne/hinten stimmt.
- In Unity zeigt `EquipmentVisuals` eine Ebene pro Platz über dem Körper (Reihenfolge Hose, Schuhe, Brust, Handschuhe, Helm, Nebenhand, Hauptwaffe) und folgt dem Ereignis `OnFrameChanged` des Körpers. Eine `SortingGroup` hält Körper und Ebenen zusammen.
- Alle 22 Item-Icons kommen aus Blender (`Sprites/Items/Item_<id>.png`, 32 px); die alten Pixel-Icons aus Code sind entfernt.

### Neue Beute

| ID | Name | Platz | Seltenheit | Werte | Quelle |
|---|---|---|---|---|---|
| `lederhandschuhe` | Lederhandschuhe | Handschuhe | Gewöhnlich | Verteidigung +1 | Schleim 6 % |
| `eisenhelm` | Eisenhelm | Helm | Selten | Verteidigung +3 | Truhe 2 |
| `eisenbrust` | Eisenbrustplatte | Brust | Selten | Verteidigung +4, Leben +10 | später |
| `eisenhose` | Eisenbeinschienen | Hose | Selten | Verteidigung +3 | später |
| `eisenstiefel` | Eisenstiefel | Schuhe | Selten | Verteidigung +2 | später |
| `eisenhandschuhe` | Eisenhandschuhe | Handschuhe | Selten | Verteidigung +2, Stärke +1 | später |
| `eichenstab` | Eichenstab | Hauptwaffe | Selten | Angriff +1, Intelligenz +3 | Truhe 2 |
| `manatrank_klein` | Kleiner Manatrank | Verbrauch | Gewöhnlich | +20 Mana | Schleim 20 %, Truhe 2 (×2) |
| `manatrank` | Manatrank | Verbrauch | Selten | +40 Mana | später |
| `saphirring` | Saphirring | Ring | Episch | Intelligenz +2, Glück +1 | Truhe 2 |

Truhe 2 steht bei (8, 3) in Room_Test.

### Befunde

- **Builder zu langsam:** Bei rund 1000 Sprites lief der Builder in die 30-s-Grenze der CLI. `ConfigureTexture` importiert jetzt nur noch neu, wenn sich die Einstellungen geändert haben.
- **Zwei Truhen:** Tests suchen Truhen über ihre ID (`chestById`), nicht mehr „irgendeine“.
- **Kurze Animationen im Test:** Angriff (L3) und Ausweichen (Tastatur-Test) werden über `OnFrameChanged` gezählt statt mit einem einzelnen Blick geprüft.
- **Simulierte Eingaben:** Am 16.11. kamen sie in zwei Gesamtläufen zeitweise nicht an. Später, mit Unity im Vordergrund, liefen alle 89 Prüfungen. Die genaue Ursache ist nicht bewiesen (wahrscheinlich Fensterfokus). Der dafür gebaute Test `diag` hat sich als nicht verlässlich erwiesen (er liest den Editor-Zustand statt des Spiels). Das Projekt lädt beim Play-Start den Code nicht neu; Tests legen ihre Test-Geräte deshalb bei jedem Start frisch an.
- **Spielstand:** Bei der Fehlersuche hat ein Diagnose-Skript außerhalb von `run_all.ps1` einmal den Spielstand überschrieben, weil es ohne Sicherung weiterlief. Er wurde aus einer Sicherung wiederhergestellt (Prüfsumme gleich). Seitdem laufen Diagnosen nur noch über `run_all.ps1`.

### Abweichungen vom Plan

- `run_all.ps1` teilt `-Tests a,b` jetzt selbst auf (bei `powershell -File` kam die Liste als ein Text an) und hat eine neue Standardliste mit allen Tests außer `diag`.
- Die Sichtbarkeitsprüfung der Game-Ansicht ist nur ein Hinweis, weil sie unzuverlässig ist.
- Die alten Rahmen `Frame_*` und die Code-Icons `Icon_*` wurden gelöscht.

### Tests

`run_all.ps1` (look, magic, wall, m2, play, iframe, kb, hp0): 89 Prüfungen, 0 Fehler (16.11.2026).

### Bekannte Grenzen

- Der Filter-Knopf „Ausrüstung“ ist für die neue Schrift zu schmal; der Text bricht um.
- Die Schwert- und Stab-Icons sind sehr dünn.
- Das Eisen-Set (außer Helm) und der große Manatrank haben noch keine Quelle.
- Touch auf einem echten Handy ist noch nicht getestet.

## Meilenstein 5: Blender-Grafik neu

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| G1 | Stil | Comic-Look: Cel-Shading mit 3 harten Helligkeitsstufen und dunkler Kontur |
| G2 | Formen | Echte modellierte Formen (Kugeln, Kegel, abgerundete Quader) statt Klötzchen |
| G3 | Bildgröße | 128 × 128 bei 128 Pixel pro Einheit (gleiche Weltgröße); Boss und Bäume 256 px |
| G4 | Animation | Mehr Bilder und neu „hurt“ (Treffer) und „death“ (Tod) |
| G5 | Richtungen | 4 Richtungen wie bisher |
| G6 | Schatten | Weicher ovaler Blob-Schatten unter jeder Figur |
| G7 | Neue Gegner | Schattenwolf, Riesenspinne und Waldwächter (Boss) nur als Grafik; Kampf-Logik später |
| G8 | Umgebung | Bodenkacheln und Deko kommen aus Blender |

### Technik

- `Art/Blender/Scripts/reborn_lib.py` ist die gemeinsame Bibliothek: Render-Setup, Kamera (45° von oben, orthografisch, 2,1 m pro 128 px), Comic-Material (`cel`), Kontur (Solidify mit umgedrehten Flächen), Formen per bmesh, Skelett, `Poser` für Schlüsselposen und das Rendern aller Animationen. Gespeichert wird mit Wiederholung, falls Unity eine Datei gerade importiert.
- Fußpunkt aller Figuren und Gegenstände: (0,5 / 0,2138) im Bild.
- `build_all.ps1` rendert alles in der richtigen Reihenfolge: Figur → Ausrüstung → Gegner → Boss → Gegenstände → Kacheln → Zauber (`-Only boss,spells` für einzelne Gruppen). `build_slime.py` ist durch `build_enemies.py` ersetzt.
- Blender-Dateien: `Reborn_Characters.blend` (Figur, Ausrüstung), `Reborn_Enemies.blend` (Schleim, Wolf, Spinne), `Reborn_Boss.blend`, `Reborn_World.blend` (Gegenstände, Kacheln), `Reborn_Spells.blend`.

| Gruppe | Animationen (Bilder je Richtung) | Bilder |
|---|---|---|
| Spieler und 14 Ausrüstungsteile | idle 4, walk 8, attack 5, dodge 4, cast 5, hurt 2, death 4 | 128 + 1792 |
| Schleim, Wolf, Spinne | idle 4, walk 6, attack 5, hurt 2, death 4 | je 84 |
| Waldwächter (256 px) | idle 4, walk 6, attack 6, rootattack 6, roar 4, hurt 2, death 6 | 136 |
| Gegenstände | Truhe zu/offen, Eiche, Tanne, Fels, Busch, Grasbüschel, Fackel (2 Bilder) | 9 |
| Kacheln (64 px) | Gras 4 Varianten, Weg, Steinmauer | 6 |
| Zauber | Drachenkopf, Drachenkörper, Wasserkugel, Feuerball (2), 4 Icons (48 px) | 9 |
| Item-Icons | 22 Icons, 48 px | 22 |

### Unity

- `RebornBuilder`: 128 Pixel pro Einheit und Fußpunkt für Spieler, Ausrüstung, Schleim, Wolf, Spinne, Boss und Gegenstände; neue Animationssets `Wolf_Anim`, `Spider_Anim`, `Boss_Anim`; Kacheln aus Blender (Gras-Varianten fest gemischt, Weg in Zeile y = −2); die alte gemalte Graskachel ist gelöscht.
- `BlobShadow` (neu, `Scripts/World`): Schatten unter Spieler, Schleimen und Schaufenster-Figuren, sortiert unter allen Figuren, verblasst beim Tod.
- `PlayerController` spielt bei Treffern „hurt“ (0,2 s) und beim Tod „death“; das letzte Sterbe-Bild bleibt stehen. Die Ausrüstungs-Ebenen folgen automatisch.
- `SlimeAI` spielt beim Verfolgen „walk“.
- `RebornBuilder.World.cs` (neu): Deko im Testraum abseits der Testwege (Eiche, Tanne, Fels, 2 Büsche, Grasbüschel, 2 flackernde Fackeln mit Licht) und ein Schaufenster oben in der Mitte mit Wolf, Waldwächter und Spinne (nur „idle“, ohne Collider).

### Befunde

- **Gesperrte Dateien beim Rendern:** Unity importiert neue PNGs, während Blender noch schreibt. Ein Render brach deshalb einmal ab. `render_still` versucht das Speichern jetzt bis zu 6-mal mit kurzer Pause. `build_all.ps1` wertet nur noch echte Abbrüche (`Traceback`) als Fehler.
- **Sterbe-Animation aus dem Bild gefallen:** Die Figur kippte nach hinten um ihre Füße und lag dadurch zum Teil außerhalb des 128-px-Bildes. Die Tests merkten das nicht, weil sie nur Bildnamen prüfen. Lösung: Der Tod hat jetzt eine eigene Fassung pro Richtung (`Player_death_<richtung>`, Liste `dir_actions` in der .blend-Datei). Die Figur fällt immer quer zum Bildschirm und liegt mittig über dem Fußpunkt. `reborn_lib.edge_pixels` zählt sichtbare Pixel am Bildrand, und `build_player.py` meldet jede Berührung (jetzt 0).
- **Nur einzelne Ebenen neu rendern:** `build_equipment.py -- <projekt> death` rendert nur die genannten Animationen (224 statt 1792 Bilder).
- **Builder im Play-Modus:** Ich hatte den Builder einmal gestartet, während im Editor gespielt wurde. Er brach beim Neuaufbau der Szene ab, ohne Schaden. Den Play-Modus vorher prüfen und nie selbst beenden.

### Abweichungen vom Plan

- Die Sterbe-Animation hat eine eigene Fassung pro Richtung, statt einer für alle (siehe Befunde).
- A5 schlug zuerst fehl, weil `lk_number.cs` (L5) beim selben Schleim `detectRadius = 0` setzt. `a_slime_go.cs` setzt den Wert wieder auf 5. Das Spiel selbst war richtig.
- Die Zauber liegen in einer eigenen Datei `Reborn_Spells.blend` statt in `Reborn_Characters.blend`, damit sie parallel zur Ausrüstung gerendert werden können.

### Tests

`run_all.ps1` (look, magic, wall, m2, play, iframe, kb, hp0): 97 Prüfungen, 0 Fehler (16.11.2026). Neu im Aussehen-Test sind A1–A8: Bildanzahl, Größe und Fußpunkt von 19 Animationssets (2308 Bilder), Treffer- und Sterbe-Animation, letztes Sterbe-Bild bleibt, Schleim läuft beim Verfolgen, Schatten, Kacheln, Deko und Schaufenster.

### Bekannte Grenzen

- Wolf, Spinne und Waldwächter stehen nur im Schaufenster; Kampf-Logik und eigene Animationen im Kampf folgen später.
- Die Tests prüfen Bilder nur über Namen und Größe; ob etwas abgeschnitten ist, zeigt die Randprüfung beim Rendern (bisher nur für die Spielfigur).
- Ein kompletter Render aller Grafiken dauert rund 30 Minuten (davon die Ausrüstung über 20).

## Meilenstein 6: Basis

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| B1 | Währung | Gold, Start 0. Schleime lassen 2–4 Gold fallen (aufsammeln wie Beute), Verkaufen bringt Gold. Anzeige im HUD und im Menü |
| B2 | Preise | Kaufpreis `buyValue` = 4 × Verkaufswert. Der Händler verkauft Tränke und das Eisen-Set und kauft alles, was nicht getragen wird |
| B3 | Verbessern | Stufe +1 bis +5 je Gegenstandsart (alle Stücke einer Art gleich). Waffe: Angriff +1 pro Stufe, Rüstung und Schild: Verteidigung +1 pro Stufe. Kosten: Verkaufswert × Stufe × 3 Gold, ab Stufe 3 zusätzlich Stufe × 2 Schleimgel. Klappt immer. Schmuck wird nicht verbessert |
| B4 | Aufbau | Eigene Szene `Base.unity` als Startszene (Build-Index 0); `Room_Test` bleibt der Kampfraum |
| B5 | Portal | Hineinlaufen → speichern → 0,4 s abdunkeln → andere Szene. Das Rückkehr-Portal ist immer offen, Gegner erscheinen bei jedem Betreten neu, Truhen bleiben offen |
| B6 | Händler- und Schmiede-Fenster | Fenster im Menü-Stil; Händler mit den Reitern „Kaufen“ und „Verkaufen“, Schmied mit Liste, Stufe, Kosten und dem Knopf „Verbessern“ |
| B7 | Steuerung | Neue Aktion „Sprechen“ (`Interact`): E, Enter, Steuerkreuz unten. Auf dem Handy erscheint der Knopf „Sprechen“ nur nahe am NPC. In der Basis sind Angriff und Zauber aus |
| B8 | Tod | Text „Du wurdest besiegt“, danach zurück in die Basis mit vollem Leben und Mana; −10 % Gold, Gegenstände und Erfahrung bleiben |
| B9 | Schmied | Eigener NPC am Amboss |

### Werte

| Ware | Kaufpreis | Verkaufswert |
|---|---|---|
| Kleiner Heiltrank / Heiltrank | 20 / 60 | 5 / 15 |
| Kleiner Manatrank / Manatrank | 24 / 72 | 6 / 18 |
| Eisenschwert | 160 | 40 |
| Eisenbrustplatte / Eisenbeinschienen | 240 / 160 | 60 / 40 |
| Eisenstiefel / Eisenhandschuhe | 120 / 120 | 30 / 30 |

- Gold: Schleim 2–4 Gold (eigenes Münz-Bild, wird beim Berühren eingesammelt). Tod: −10 % Gold, abgerundet.
- Verbessern, Beispiel Holzschwert (Verkaufswert 10): +1 = 30 Gold, +2 = 60, +3 = 90 und 6 Schleimgel, +4 = 120 und 8, +5 = 150 und 10.
- Basis (16 × 12 Kacheln): Start (0 / −2), Händler (−1,4 / −2,75), Schmied (3,3 / −3,0), Portal (4,5 / 2,2), Rückkehr am Portal (4,6 / 0,9). Im Kampfraum: Rückkehr-Portal (−5,8 / −4,4), Ankunft (−4,6 / −3,1).
- Ansprechen ab 1,5 Einheiten Abstand.

### Steuerung (neu)

| Aktion | Tastatur | Controller | Handy |
|---|---|---|---|
| Sprechen (Händler, Schmied) | E, Enter | Steuerkreuz unten | Knopf „Sprechen“ (nur nahe am NPC) |
| Fenster schließen | Tab, Esc, I | Start | Knopf „X“ |

### Speichern

Version 3 speichert zusätzlich Gold, die Stufen beim Schmied und die Szene. Gespeichert wird zusätzlich beim Kaufen, Verkaufen, Verbessern, beim Schließen des Handelsfensters und vor jedem Szenenwechsel. Spielstände der Version 2 laden weiter (Gold 0, keine Stufen). Das Spiel startet immer in der Basis; „Spielstand löschen“ startet ebenfalls dort neu.

### Abweichungen vom Plan

- „Sprechen“ liegt beim Controller auf dem Steuerkreuz unten statt auf der Süd-Taste, weil die Süd-Taste schon Ausweichen ist.
- Haus, Portal, Stand, Amboss, Fass, Kiste und Gold-Icon kommen aus einer eigenen Datei `Reborn_Base.blend` (`build_base.py`). Händler und Schmied sind die umgefärbte Spielfigur mit Zubehör (`build_npcs.py`); `Reborn_Characters.blend` wird dabei nicht gespeichert.
- Die Mauer der Basis hat feste Kästen statt der Kachel-Kollision: Unity erzeugt für die 2. Szene im selben Builder-Lauf keine Kachel-Geometrie.
- Zusätzliche Dateien: `SceneTravel.cs` (Abdunkeln und Szenenwechsel), `SceneRules.cs` (friedliche Szene), `TradeWindow.cs` (gemeinsames Fenster für Händler und Schmied), `SpawnPoint.LastArrival` für die Tests.

### Befunde

- **Basis ohne Boden:** Der Builder hat die Basis zweimal ohne Boden- und Mauerkacheln gespeichert (schwarzer Boden). Ursache: Eine neue Szene gibt nicht benutzte Assets frei, die Kacheln aus dem ersten Szenenbau waren danach ungültig, und `SetTile` setzte still nichts. Die Tests merkten es nicht, weil die Mauer aus festen Kästen besteht. Behoben: `NewWorldScene` lädt die Kacheln nach dem Szenenwechsel frisch und bricht ab, wenn Boden oder Mauer leer sind. Der Basis-Test prüft die Kacheln jetzt mit (B1).
- **Fehler aus der Code-Durchsicht:** „Spielstand löschen“ startete im Kampfraum neu statt in der Basis; der Schmied zeigte nur 12 von 14 Ausrüstungsarten (jetzt 18 Plätze). Beides behoben.
- **Testfehler:** Der Basis-Test erwartete, dass das erste Verbessern am Gold scheitert, der Spieler hatte aber genug. Jetzt setzt der Test vorher 20 Gold. Im Kampfraum stoßen die Schleime den Spieler sofort weg; der Test prüft dort deshalb den benutzten Ankunftspunkt statt der Position.

### Tests

`run_all.ps1` (look, magic, wall, m2, play, iframe, kb, hp0, base): 125 Prüfungen, 0 Fehler (17.11.2026). Neu ist der Basis-Test B1–B14: Start, friedliche Basis, Mauer und Kacheln, Händler (Kaufen, Verkaufen, zu wenig Gold, Inventar voll), E und Esc, Schmied mit Steuerkreuz und allen Stufen bis +3, Speichern Version 3, beide Portale, Tod mit −10 % Gold, alter Spielstand (Version 2).

### Bekannte Grenzen

- Das Haus der Basis hat verkehrt herum geneigte Dachflächen (sieht flach aus). Die Basis wird in Meilenstein 7a durch die Stadt ersetzt.
- Touch auf einem echten Handy ist noch nicht getestet.
- Der Händler hat unbegrenzte Waren und kauft zum festen Preis.

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler und Ausrüstung (Szene „Characters“)
- `Art/Blender/Reborn_Enemies.blend`, `Reborn_Boss.blend`, `Reborn_World.blend`, `Reborn_Spells.blend` – Gegner, Boss, Umgebung, Zauber
- `Art/Blender/Scripts/build_all.ps1` – rendert alle Blender-Grafiken
- `Assets/_Project/Scenes/Base.unity` – Basis (Startszene), `Assets/_Project/Scenes/Room_Test.unity` – Kampfraum
- `Art/Blender/Reborn_Base.blend`, `Art/Blender/Scripts/build_base.py`, `build_npcs.py` – Grafik der Basis und der NPCs
- `Assets/_Project/Scripts/Economy/` – Gold, Händler-Waren, Schmiede-Stufen
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core, World)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
- `Art/Blender/Scripts/build_props.py`, `build_tiles.py` – Gegenstände und Kacheln (Szenen „Props“, „Tiles“)
- `Art/Blender/Scripts/build_spells.py` – Zauber-Grafik (Szene „Spells“)
- `Assets/_Project/Scripts/Magic/` – Mana, Zauberbuch, Effekte
- `Tools/PlayTests/` – automatische Play-Tests
