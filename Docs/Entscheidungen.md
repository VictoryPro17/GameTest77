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

## Meilenstein 7: Stadt & Gilden

Wunsch: große, schöne Stadt mit Stadtmauer und Soldaten, mehr Häuser und NPCs, Einkaufen und Verkaufen, drei Gilden (Magier, Krieger, Assassinen) mit begehbaren Gebäuden und Quests, das Portal in einer Höhle. Aufgeteilt in 7a (Stadt), 7b (begehbare Räume) und 7c (Gilden und Quests).

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| Q1 | Gilden | Man tritt nur **einer** Gilde bei (Wechsel kostet Gold) |
| Q2 | Gilden-Vorteile | Magier Intelligenz +2 und Gildenstab, Krieger Stärke +2 und Gildenschwert, Assassine Geschick +2 und Dolche; mit jedem Rang etwas mehr |
| Q3 | Quests | 3 pro Gilde (9), danach Rang-Aufstieg |
| Q4 | Gegner für Quests | Wolf und Spinne bekommen in 7c eine Kampf-Logik |
| Q5 | Begehbar | Die 3 Gildenhallen und die Portal-Höhle; Läden bedient man von außen |
| Q6 | Soldaten | Nur Deko, nicht angreifbar |
| Q7 | Bewohner | Laufen umher; Sätze beim Ansprechen folgen später |
| Q8 | Reihenfolge | Erst Meilenstein 6 committen, dann 7a, 7b, 7c, jeweils mit Tests und Commit |

## Meilenstein 7a: Stadt

### Was gebaut ist

- Die Szene `Base` ist jetzt eine Stadt mit 48 × 36 Kacheln (`RebornBuilder.Town.cs`). Startpunkt, Händler und Schmied stehen an denselben Stellen wie in der Basis aus Meilenstein 6.
- **Stadtmauer** rundum: Norden und Süden als Mauerbild mit Zinnen, Westen und Osten als Wehrgang (2 Kacheln), 4 Ecktürme, 4 Mauertürme, Stadttor im Süden (geschlossen). Die Kollision sind feste Kästen.
- **Soldaten:** 5 patrouillieren auf der Mauer (vor der Mauer gezeichnet), 2 Wachen stehen am Tor, 1 Wache läuft die Hauptstraße ab.
- **Marktplatz** mit Pflaster und Springbrunnen (2 Bilder), Händlerstand, Schmiede mit Esse, Rauch und Lichtschein, Rathaus, Gasthaus, Bänke, Laternen mit Licht, Blumenkästen, Brunnen, Karren.
- **Ladenstraße** im Süden: Waffenhändler (Holzschwert, Eisenschwert, Eichenstab, Holzschild), Rüstungshändler (Leder- und Eisen-Set), Alchemistin (4 Tränke), Gemischtwarenladen. Jeder Händler nutzt das Handelsfenster aus Meilenstein 6.
- **Wohnviertel** West, Ost und Süd: 15 Häuser in 6 Varianten (auch zweistöckig und breit), Gärten mit Zäunen und Heu, Teich mit Steg und 2 Fischern.
- **Gildenviertel** im Norden: Kriegerhalle, Magierturm, Assassinenhaus (1,5-mal so groß wie normale Häuser). Betreten folgt in 7b.
- **Höhle** im Nordwesten: Felsen mit Höhleneingang. Im Eingang liegt das Portal zum Kampfraum (das sichtbare Portal ist entfernt, nur das Leuchten bleibt). Rückkehr aus dem Kampfraum: vor der Höhle.
- **Bewohner:** 10 Figuren laufen zwischen festen Wegpunkten (`Wanderer.cs`, ohne Collider, damit sie niemanden einsperren).
- Freie Grasflächen bekommen Bäume, Büsche und Grasbüschel nach festem Muster; Laufwege und Teststrecken bleiben frei.

### Grafik (Blender)

- `build_town.py` (Szene „Town“, `Reborn_Town.blend`): 43 Bilder. Rathaus, Gasthaus, Schmiede, Tor, Eckturm, Felsen mit Höhle (384 px); Gildenhäuser (768 px); Läden und Häuser (256/384 px); Mauerstück, Hecke und Zaun (128 px, nahtlos alle 1 Einheit); Kleinzeug; Rauch (3 Bilder); Kacheln (64 px): Pflaster (3), Pflaster mit Grasrand (wird für die anderen Seiten gedreht), Wasser (2), Steg.
- `build_villagers.py`: 11 Figuren auf Basis der Spielfigur (Bäuerin, Wache, Gelehrter, Soldat, Bauer, Edeldame, Barde, Fischer, Waffenhändler, Rüstungshändler, Alchemistin), je „idle“ (4) und „walk“ (8) in 4 Richtungen, 528 Bilder. `Reborn_Characters.blend` wird nicht gespeichert.
- `build_all.ps1` hat die neuen Gruppen `town` und `villagers`.

### Befunde

- **Dächer verkehrt herum:** Die Dachflächen waren nach innen statt nach außen geneigt und sahen deshalb flach aus. In `build_town.py` behoben (mit Ziegelreihen und First). Das Haus der alten Basis (`build_base.py`) hat den Fehler noch, wird aber nicht mehr benutzt.
- **Schwarze Erdwege:** Die Stadt benutzte die Weg-Kachel aus dem ersten Szenenbau, die nach dem Szenenwechsel ungültig war (gleiche Ursache wie der leere Boden in Meilenstein 6). Die Stadt lädt alle Kacheln jetzt nach dem Szenenwechsel.
- **Am Magierturm vorbei:** Die Kollision war schmaler als Sockel und Kristallsäulen; jetzt 2 m statt 1,5 m halbe Breite.

### Tests

`run_all.ps1` (look, magic, wall, m2, play, iframe, kb, hp0, base, town): 137 Prüfungen, 0 Fehler (17.11.2026). Neu ist der Stadt-Test T1–T6; der Basis-Test läuft jetzt in der Stadt (5 NPCs, Stadtmauer, Rückkehr vor der Höhle).

### Bekannte Grenzen

- Gildenhäuser und Höhle sind von außen fertig, aber noch nicht begehbar (7b). Gilden und Quests folgen in 7c.
- Bewohner sagen noch nichts; Soldaten und Wachen stehen nur.
- Das Wasser des Teichs ist nicht animiert (die Kachel-Animation braucht ein weiteres Paket).
- Das Stadttor ist zu; außerhalb der Mauer gibt es nichts.
- Häuser und Läden kann man nicht betreten.

## Meilenstein 7b: Begehbare Räume

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| R1 | Aufbau | Eine eigene Szene pro Raum: `Guild_Warrior`, `Guild_Mage`, `Guild_Assassin`, `Cave` |
| R2 | Betreten | In die Tür laufen, kurz abdunkeln (wie das Portal); drinnen bringt die Fußmatte am Ausgang zurück vor die Tür |
| R3 | Einrichtung | Krieger: Holzdielen, Waffenständer, Übungspuppe, Banner, Tisch. Magier: Steinboden mit Runenkreis, Bücherregale, Alchemietisch, schwebender Kristall. Assassinen: dunkles Holz, Vorhänge, Zielscheibe, Kisten, Kerzen |
| R4 | Gildenmeister | Je einer pro Halle; beim Ansprechen eine Sprechblase mit einem Satz. Beitreten und Quests folgen in 7c |
| R5 | Höhle | Der Höhleneingang führt in die Höhle; dort steht das Portal zum Kampfraum. Rückkehr aus dem Kampfraum in die Höhle, Ausgang in die Stadt vor den Felsen |
| R6 | Licht | Drinnen dunkler (Grundlicht 0,6, Höhle 0,4) mit Fackeln, Kerzen und Kristallen als Lichtquellen |

### Was gebaut ist

- 4 neue Szenen (je 16 × 12 Kacheln, friedlich, eigener Boden und eigene Wände): `Guild_Warrior` (Kriegerhalle, Holzdielen, Licht 0,6), `Guild_Mage` (Magierturm, Steinboden, Licht 0,6), `Guild_Assassin` (Assassinenhaus, dunkles Holz, Licht 0,45), `Cave` (Höhle, Felsboden, Licht 0,4). Gebaut von `RebornBuilder.Interiors.cs`; alle 6 Szenen stehen in der Build-Liste (Base 0, Room_Test 1, dann die 4 Räume).
- **Einrichtung:** Kriegerhalle mit 2 Waffenständern, 2 Übungspuppen, 2 Bannern, langem Tisch, 2 Fackeln. Magierturm mit Runenkreis, schwebendem Kristall (Licht), 4 Bücherregalen, 2 Alchemietischen, Kerzenständern, 2 Fackeln. Assassinenhaus mit 2 Vorhängen, 2 Zielscheiben, 3 Kerzenständern (Licht), Kiste. Höhle mit 5 Stalagmiten, 4 leuchtenden Kristallgruppen und dem Portal zum Kampfraum.
- **Gildenmeister:** Kriegerin Brunhild, Erzmagier Aldric, Schattenmeisterin Nyx. Ansprechen (E, Enter, Steuerkreuz unten) zeigt eine Sprechblase mit einem Satz für 3 s (`World/SpeechBubble.cs`, neue NpcKind `Gildenmeister` mit Feld `greeting`). Kein Fenster; Beitreten und Quests folgen in 7c.
- **Türen:** Neue Klasse `World/Door.cs` (verhält sich wie ein Portal: hineinlaufen, speichern, abdunkeln, Szene wechseln). In der Stadt je eine Tür vor Kriegerhalle, Magierturm und Assassinenhaus (`Tuer_Gilde_Krieger`, `Tuer_Gilde_Magier`, `Tuer_Gilde_Assassine`), Ankunft davor (`Tuer_Krieger`, `Tuer_Magier`, `Tuer_Assassine`). Drinnen: Fußmatte unten in der Mitte = Ausgang (`Tuer_Ausgang`), Ankunft „Eingang“.
- **Höhlen-Weg:** Der Höhleneingang in der Stadt heißt jetzt `Tuer_Hoehle` und führt in die Höhle (Ankunft „Eingang“). In der Höhle führt `Portal_Kampf` in den Kampfraum. Das Rückkehr-Portal im Kampfraum führt jetzt in die Höhle (Ankunft „Portal“), der Höhlenausgang in die Stadt vor den Felsen.
- `NewWorldScene` hat einen optionalen Kachel-Lader (`tileLoader`), damit Räume eigene Kacheln bekommen.

### Grafik (Blender)

- `build_interiors.py` (eigene Datei `Reborn_Interiors.blend`): 22 Bilder in `Sprites/Interiors` – 8 Kacheln 64 px (Floor_Wood, Floor_Stone, Floor_Dark, Floor_Cave, Wall_Wood, Wall_Mage, Wall_Dark, Wall_Cave) und 14 Einrichtungsteile (weapon_rack, dummy, banner, table_long, bookshelf, alchemy_table, crystal_float, rune_circle, curtain, target, candles, stalagmite, crystal_cluster, doormat). 0 Randwarnungen.
- `build_guildmasters.py`: 3 Gildenmeister auf Basis der Spielfigur, je „idle“ nach unten, 4 Bilder (12 Bilder). `Reborn_Characters.blend` wird nicht gespeichert.
- `build_all.ps1` hat die neuen Gruppen `interiors` und `guildmasters`.

### Befunde

- Nach dem Umbau fand `FindAnyObjectByType<Portal>` auch Türen (`Door` erbt von `Portal`). Der Basis-Test sucht den Höhleneingang jetzt über den Namen.
- Im Stadt-Test hielt der Test den Stick nach der Ankunft in der Höhle weiter gedrückt, die Figur lief gleich ins Portal. Der Stick wird jetzt nach 0,9 s losgelassen.

### Tests

- Neuer Raum-Test `interiortest.ps1` (Kürzel `interior`): jede Gildenhalle hinein und hinaus mit Ankunftspunkt, rechte Wand hält, Gildenmeister spricht, Licht/Kacheln/Bilder, Höhle → Portal → Kampfraum → Höhle → Stadt, Gold bleibt über 10 Wechsel erhalten. Basis- und Stadt-Test auf den Weg über die Höhle angepasst.
- `run_all.ps1` (look, magic, wall, m2, play, iframe, kb, hp0, base, town, interior): 160 Prüfungen, 0 Fehler (23.11.2026).

### Bekannte Grenzen

- Die Räume wirken noch recht leer, die Möbel sind im Maßstab der Figur klein.
- Gildenmeister sagen nur einen Satz; Beitreten, Ränge und Quests folgen in 7c.
- Läden und Wohnhäuser kann man weiterhin nicht betreten (so entschieden, Q5).
- Touch auf einem echten Handy ist noch nicht getestet.

## Meilenstein 7c: Gilden und Quests

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| G1 | Kampfgebiet | Neue Szene „Wald“ über ein zweites Portal in der Höhle: 4 Schattenwölfe, 3 Riesenspinnen, 2 Schleime, Bäume und Felsen. Der Kampfraum bleibt der Schleim-Übungsraum |
| G2 | Gegner-KI | `SlimeAI` wird allgemeine Kreaturen-KI (Name bleibt) mit Werten pro Art. Wolf: schnell, 40 Leben, 8 Schaden, 50 EP, 3–6 Gold. Spinne: langsam, 60 Leben, 12 Schaden, 70 EP, 5–9 Gold. Angriffs-Animation bei Berührung |
| G3 | Neue Beute | Wolfsfell (Wolf 60 %), Spinnenseide (Spinne 60 %); Material für Quests, verkaufbar |
| G4 | Ränge | Beitritt = Rang 1 „Anwärter“ (+2 auf das Gilden-Attribut, Gildenwaffe geschenkt); nach 3 Quests Rang 2 „Geselle“ (+4, Waffe auf Stufe +2) |
| G5 | Wechseln | Andere Gilde kostet 200 Gold; Rang und Quests der alten Gilde gehen verloren, die Waffe bleibt |
| G6 | Quests | Je eine aktiv. Krieger: 5 Schleime → 3 Wölfe → 4 Wolfsfelle bringen. Magier: 5 Gegner mit Zaubern → 3 Spinnenseiden bringen → 3 Spinnen. Assassine: 3 Wölfe → 3 Spinnen → 6 Schleimgel und 2 Spinnenseiden bringen. Belohnung 50/80/120 Gold und 60/100/150 EP |
| G7 | Gilden-Fenster | Beim Gildenmeister: Beitreten/Wechseln, Rang, Quest mit Fortschritt, Annehmen, Abgeben; Quest-Hinweis im HUD oben rechts |
| G8 | Gildenwaffen | Gildenschwert (Angriff +5, Stärke +1), Gildenstab (Angriff +2, Intelligenz +4), Schattendolche (Angriff +4, Geschick +2); nur von der eigenen Gilde |
| G9 | Speichern | Spielstand Version 4: Gilde, Rang, Quest-Nummer, Fortschritt; alte Stände laden ohne Gilde |

### Was gebaut ist

- **Gilden-Logik:** `Guilds/GuildData.cs` (ScriptableObject: Gilde, Attribut, Waffe, 3 Quests; Assets in `Data/Guilds`) und `Guilds/PlayerGuild.cs` am Spieler: Beitreten (Rang 1 Anwärter, +2 Attribut, Gildenwaffe ins Inventar), Wechsel für 200 Gold, eine aktive Quest, Fortschritt über das Ereignis `SlimeAI.OnAnyKilled` (Besiegen) oder das Inventar (Bringen), Abgeben mit Gold und EP, nach 3 Quests Rang 2 Geselle (+4, Gildenwaffe auf Schmiede-Stufe +2). Der Bonus fließt in `PlayerStats` ein.
- **Zauber-Treffer:** `Health.LastHitBySpell` (gesetzt von `SpellEffect`) für die Quest „Gegner mit Zaubern besiegen“.
- **Gilden-Fenster:** neue Seite `UI/GuildView.cs` im Handelsfenster: Begrüßung, Rang und Bonus, Quest mit Fortschritt und Belohnung, Knöpfe Beitreten/Wechseln, Annehmen, Abgeben. Gildenmeister öffnen dieses Fenster (`Npc.guild`). Quest-Hinweis im HUD oben rechts (z. B. „Wölfe besiegen 2/3“).
- **Gegner:** `SlimeAI` ist jetzt die allgemeine Kreaturen-KI (`creatureId`, Angriffs-Animation bei Treffer). Prefabs `Wolf` (40 Leben, Tempo 3,2, 8 Schaden, 50 EP, Erkennung 6) und `Spider` (60 Leben, Tempo 1,6, 12 Schaden, 70 EP, Erkennung 5). Beute: `Wolf_Loot` (Wolfsfell 60 %, kleiner Heiltrank 15 %, 3–6 Gold), `Spider_Loot` (Spinnenseide 60 %, kleiner Manatrank 15 %, 5–9 Gold).
- **Neue Gegenstände:** Wolfsfell (Verkauf 8), Spinnenseide (10), Gildenschwert (Angriff +5, Stärke +1), Gildenstab (Angriff +2, Intelligenz +4), Schattendolche (Angriff +4, Geschick +2), je Verkauf 70.
- **Wald:** neue Szene `Forest` (24 × 16 Kacheln, Licht 0,8, nicht friedlich) mit 4 Schattenwölfen, 3 Riesenspinnen, 2 Schleimen, 12 Bäumen, 3 Felsen, umgestürzten Bäumen, Stümpfen, Pilzen und Farn. Erreichbar über das grüne `Portal_Wald` in der Höhle (Ankunft „Ankunft“); `Portal_Hoehle` im Wald führt zurück (Ankunft „Wald“ in der Höhle). Steht in der Build-Liste.
- **Spielstand Version 4:** `guild`, `guildRank`, `questIndex`, `questActive`, `questProgress`; ältere Stände laden ohne Gilde.

### Grafik (Blender)

- `build_equipment.py`: 3 Gildenwaffen als sichtbare Ebenen (384 Bilder), 5 neue Icons; neuer Filter `item:<id>`, um nur einzelne Teile neu zu rendern.
- `build_forest.py` (eigene Datei `Reborn_Forest.blend`): 4 Kacheln (Waldboden 2×, Waldweg, Dornenhecke als Rand) und 4 Deko-Teile (umgestürzter Baum, Pilze, Farn, Baumstumpf). 0 Randwarnungen.
- `build_all.ps1`: neue Gruppe `forest`.

### Befunde

- Nach den neuen Gegenständen erwarteten 3 alte Prüfungen noch den alten Stand (19 statt 22 Animationssets, Spielstand-Version 3 statt 4); angepasst.
- Im 0-HP-Test war die Verbindung zu Unity einmal kurz weg (nur eine Protokollzeile, keine Prüfung betroffen).

### Tests

- Neuer Gilden-Test `guildtest.ps1` (Kürzel `guild`): Beitreten mit Bonus und Waffe, alle 3 Krieger-Quests bis Rang 2, Abgeben zu früh geht nicht, Wechsel mit zu wenig und genug Gold, Zauber-Quest, Speichern und Laden Version 4, Wald mit Gegnern, Wolf und Spinne treffen, sterben und lassen Beute fallen, Rückweg in die Höhle. Der Raum-Test prüft jetzt, dass der Gildenmeister das Gilden-Fenster öffnet.
- `run_all.ps1` (12 Tests: look, magic, wall, m2, play, iframe, kb, hp0, base, town, interior, guild): 186 Prüfungen, 0 Fehler (23.11.2026).

### Bekannte Grenzen

- Der Hinweistext unten im Gilden-Fenster nennt noch „Kaufen · Verkaufen · Verbessern“.
- Der Wald wirkt etwas leer; Bäume sind im Maßstab klein.
- Die Icons für Wolfsfell und Spinnenseide sind schwach erkennbar.
- Gegner erscheinen bei jedem Betreten neu; es gibt nur Rang 1 und 2.
- Der Boss (Waldwächter) hat noch keine Kampf-Logik.
- Touch auf einem echten Handy ist noch nicht getestet.

## Meilenstein 8: Golem-Boss und schönere Gildenhallen

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| B1 | Ort | Das Schaufenster im Kampfraum entfällt (Wolf und Spinne kämpfen im Wald). Der Golem (Waldwächter) steht oben in der Mitte und wacht auf, wenn man ihm auf 4 Einheiten nahe kommt |
| B2 | Stärke | 1200 Leben, Berührung 20, Schlag 30 (0,5 s Ausholen), Wurzeln 25 (rote Warnfläche, 0,8 s, ausweichbar) |
| B3 | Phasen | 50 %: Brüllen, 3 Wölfe, schneller, öfter Angriffe. 25 %: Wut, Wurzelangriff an 3 Stellen, schnellerer Schlag |
| B4 | Anzeige | Große Lebensleiste oben in der Mitte „Waldwächter“, nur wenn er wach ist |
| B5 | Belohnung | 400 Gold, 600 EP, fest das Amulett „Herz des Waldes“ (Leben +25, Vitalität +3) |
| B6 | Besiegt | Gespeichert (Spielstand Version 5); er erscheint nicht wieder. Nach dem eigenen Tod hat er beim nächsten Versuch volles Leben |
| B7 | Gildenhallen | Größere Möbel (256 px): Krieger Teppich, Kamin, Rüstungsständer, Waffenwand, Festtafel; Magier Teppich, Kessel, Globus, Teleskop, Bücherwand, Sternkarte; Assassinen Teppich, Kartentisch, Dolchwand, Schränke, Laternen; je 2 Gildenmitglieder laufen umher |
| B8 | Tests | `t2_setup` schaltet auch den Boss aus, damit die alten Tests im Kampfraum stabil bleiben; Prüfungen am Schaufenster entfallen |
| B9 | Token sparen | Spiel-Logik, Builder und Tests in der Hauptsitzung; Blender-Grafik von einem Sonnet-Hilfsagenten, Doku von einem günstigen Modell |

### Was gebaut ist

- **Golem-Boss:** `Enemies/BossAI.cs` – schläft, wacht in 3,2 Einheiten Nähe auf (Brüllen), läuft (Tempo 1,2, Phase 2: 1,8), Schlag aus der Nähe (0,5 s Ausholen, in Wut 0,3 s; 30 Schaden im Kreis vor ihm; Abklingzeit 2,2 s, ab Phase 2 1,4 s), Wurzelangriff aus mindestens 2,4 Einheiten Abstand (Abklingzeit 4 s, ab Phase 2 2,6 s), Berührung 20. 1200 Leben. Phase 2 bei 50 %: Brüllen, 3 Wölfe. Phase 3 (Wut) bei 25 %: Wurzeln an 3 Stellen. Prefab `Golem`, steht im Kampfraum oben in der Mitte (statt Schaufenster).
- **Wurzelangriff:** `Enemies/RootSpike.cs` – rote Warnfläche, nach 0,8 s Wurzeln (3 Bilder), 25 Schaden im Radius 0,7; Ausweichen schützt.
- **Lebensleiste:** `UI/BossBar.cs` oben in der Mitte mit Name und Phase („Phase 2“, „Wut!“), nur solange der Boss wach ist.
- **Beute:** `Boss_Loot`: Amulett „Herz des Waldes“ (Halskette, Leben +25, Vitalität +3), 2 Heiltränke, 400 Gold; dazu 600 EP.
- **Spielstand Version 5:** Liste `defeatedBosses`; ein besiegter Boss erscheint nicht wieder.
- **Gildenhallen:** neue große Möbel: Krieger Teppich, Kamin mit Licht, Waffenwand, 2 Rüstungsständer, Festtafel; Magier Teppich, Bücherwand, Kessel mit grünem Licht, Globus, Teleskop, Sternkarte; Assassinen Teppich, Dolchwand, Kartentisch, 2 Schränke, 2 rote Laternen mit Licht. Je 2 Gildenmitglieder laufen umher (`Wanderer`, Animationssets `Member_*_Anim`).
- `t2_setup` schaltet auch den Boss aus, damit die alten Tests im Kampfraum stabil bleiben.

### Grafik (Blender)

- Grafik von einem Sonnet-Hilfsagenten nach Vorgabe erstellt.
- `build_boss_extras.py` (`Reborn_BossExtras.blend`): Wurzelstacheln (3 Bilder), Warnfläche, Icon „Herz des Waldes“.
- `build_interiors2.py` (`Reborn_Interiors2.blend`): 16 neue Möbel (128/256 px).
- `build_guildmembers.py`: 6 Gildenmitglieder (krieger_1/2, magier_1/2, assassine_1/2), idle und walk in 4 Richtungen, je 48 Bilder.
- `build_all.ps1`: neue Gruppen `bossextras`, `interiors2`, `guildmembers`.

### Befunde

- Der Grafik-Agent brach einmal mit einem Server-Fehler (529, überlastet) ab, hatte aber alle Bilder schon erzeugt.
- 3 alte Prüfungen erwarteten noch Spielstand-Version 4; angepasst.
- Die Mana-Prüfung Z4 (Magie-Test) war zeitabhängig: Mana erholt sich mit der Zeit um 1. Sie erlaubt jetzt 3 oder 4.
- Der vollständige Testlauf dauert jetzt über 10 Minuten und läuft im Hintergrund.

### Tests

- Neuer Boss-Test `bosstest.ps1` (Kürzel `boss`): schläft auf Abstand, wacht auf, Lebensleiste mit Namen, Schlag trifft, Wurzeln treffen (beim Ausweichen nicht), Phase 2 mit 3 Wölfen, Phase 3 mit 3 Wurzeln auf einmal, Sieg mit Herz des Waldes, Gold und EP, gespeichert, nach Neustart kein Boss, Tod gegen den Boss → Stadt. Raum-Test prüft je 2 Gildenmitglieder. Aussehen-Test ohne Schaufenster.
- `run_all.ps1` (13 Tests): 198 Prüfungen, 0 Fehler (23.11.2026).

### Bekannte Grenzen

- Der Boss-Name über der Lebensleiste ist im Bild nicht zu sehen (der Text ist gesetzt); wird in 8b behoben.
- Waffen- und Dolchwand wirken eher wie liegende Tische; dem Kessel fehlt das grüne Leuchten im Bild (nur das Licht leuchtet).
- Der Kampfraum ist für den Boss-Kampf eng.
- Touch auf einem echten Handy ist noch nicht getestet.

## Meilenstein 8b: Boss mit zwei Stufen

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| C1 | Stufe 2 | Bei 0 Leben erwacht der Golem (3 s Verwandlung, unverwundbar) als „Uralter Waldwächter“: 1500 Leben, schneller, Schlag 35. Beute erst nach Stufe 2 |
| C2 | Neue Fähigkeiten | Dornenwelle (Ring breitet sich vom Boss aus, 25 Schaden, weggehen oder ausweichen). Ansturm (1 s Ziel-Linie am Boden, dann rennt er geradeaus, 30 Schaden, zur Seite gehen) |
| C3 | Ultimate „Zorn des Waldes“ | Nur in Stufe 2, frühestens nach 20 s, dann alle 30 s; 4 s Aufladung mit großem roten Kreis (Radius 5,5) und Hinweis im Bild; wer drin steht, verliert 50 % seines maximalen Lebens; rauslaufen oder im Moment ausweichen schützt |
| C4 | Wolfsruf | In Stufe 2 alle 15 s 2 verwundete Wölfe mit halbem Leben (20 statt 40); die 3 Wölfe in Stufe 1 bei 50 % bleiben |
| C5 | Wiederkehr | Ganz besiegt erscheint er nach 10 Minuten wieder (echte Zeit, im Spielstand, gilt auch nach Neustart). Amulett nur beim ersten Sieg, danach 300 Gold und Tränke |
| C6 | Anzeige | Lebensleiste zeigt „Stufe 2“ und eine Ladeleiste für die Ultimate; der Boss-Name ist sichtbar |

### Was gebaut ist

- **Stufe 2:** `BossAI` erwacht bei 0 Leben in Stufe 1 (3 s unverwundbar, Brüllen) als „Uralter Waldwächter“: 1500 Leben, Tempo 2,1, Schlag 35 (schnelleres Ausholen), eigenes Animationsset `Boss2_Anim`. Wölfe aus Stufe 1 verschwinden. Beute erst nach Stufe 2.
- **Dornenwelle:** `Enemies/ThornWave.cs` – Ring wächst in 1,2 s von 0,6 auf 4,5 Einheiten, 25 Schaden, trifft höchstens einmal; Abklingzeit 6 s, wenn der Spieler in 4 Einheiten Nähe ist.
- **Ansturm:** `Enemies/ChargeTelegraph.cs` – 1 s blinkende Ziel-Linie, dann rennt der Boss 0,7 s mit Tempo 9 geradeaus; 30 Schaden bei Berührung oder Mittelpunkt-Abstand ≤ 1,3, höchstens einmal, danach bleibt er stehen; Abklingzeit 7 s, wenn der Spieler weiter als 3 Einheiten weg ist.
- **Ultimate „Zorn des Waldes“:** `Enemies/UltimateZone.cs` – erst 20 s nach dem Erwachen, dann alle 30 s; 4 s Aufladung, großer pulsierender Kreis (Radius 5,5), Warnung „Zorn des Waldes! Lauf weg oder weiche aus!“; wer drin steht, verliert 50 % seines maximalen Lebens.
- **Wolfsruf:** alle 15 s 2 Wölfe mit 20 Leben.
- **Wiederkehr:** Spielstand Version 6 mit `bossRecords` (Zeitpunkt des Siegs, Anzahl der Siege). Nach 10 Minuten (echte Zeit, `SaveSystem.BossRespawnMinutes`) ist der Boss beim nächsten Betreten wieder da. Ab dem zweiten Sieg Beute `Boss_Loot_Repeat` (300 Gold, 2 Heiltränke, 1 Manatrank) statt Amulett. Spielstände Version 5 laden: dort besiegte Bosse gelten als längst wieder da.
- **Lebensleiste:** Name je Stufe („Uralter Waldwächter (Stufe 2)“), orange Lade-Leiste der Ultimate, Warnhinweis in der Bildmitte. Der Name war in Meilenstein 8 unsichtbar, weil das Textfeld für die Schrift zu niedrig war (Unity blendet Text dann aus); behoben mit höherem Feld und Overflow.

### Grafik (Blender)

Grafik von einem Sonnet-Hilfsagenten nach Vorgabe erstellt.

- `build_boss2.py`: Stufe 2 aus `Reborn_Boss.blend` umgefärbt (dunkle Rinde, glühende Risse, Hörner, Kristalle), alle 136 Bilder nach `Sprites/Boss2`; die Datei wird nicht gespeichert.
- `build_boss2_fx.py` (`Reborn_Boss2Fx.blend`): Dornenring (3 Bilder), Ansturm-Pfeil, Ultimate-Kreis. Eigene Pivots: Ring 0,5/0,4747, Kreis 0,5/0,5, Pfeil 0,0476/0,5 (Start links).
- `build_all.ps1`: neue Gruppen `boss2`, `boss2fx`.

### Befunde

- Der Ansturm traf im ersten Test nicht: Die Körper stoßen zusammen, bevor die Mittelpunkte nah genug kommen. Treffer zählt jetzt auch bei Berührung (Abstand ≤ 1,3). Der Test stellt den Spieler Richtung Raummitte und schaltet die Wurzeln dafür ab (`SetTimers` mit 5. Wert).
- 9 Boss-Bilder (Tod und Brüllen) berühren knapp den Bildrand; das war schon in Stufe 1 so.

### Tests

- Boss-Test erweitert (24 Prüfungen): Erwachen (unverwundbar), Dornenwelle (trifft, beim Ausweichen nicht), Ansturm trifft, Ultimate (Warnung, 50 % im Kreis, außerhalb kein Schaden), Wolfsruf (2 Wölfe, 20 Leben), Sieg mit Amulett (Version 6), nach Neustart weg, nach 11 Minuten (vorgespult) wieder da, zweiter Sieg ohne Amulett, Tod → Stadt.
- `run_all.ps1` (13 Tests): 210 Prüfungen im vollen Lauf; 3 alte Prüfungen (B10, B14, G6) erwarteten noch Spielstand-Version 5 und prüfen jetzt versionsunabhängig (`Version=\d+`). Danach Basis- und Gilden-Test erneut 56/56, alle übrigen Tests im vollen Lauf bestanden (23.11.2026).

### Bekannte Grenzen

- Der Boss erscheint erst beim nächsten Betreten des Kampfraums wieder, nicht während man drin steht.
- Stufe 2 wirkt eher grau-violett als schwarz; Hörner und Kristalle sind klein.
- Der Kampfraum ist für Ansturm und Ultimate eng.
- Touch auf einem echten Handy ist noch nicht getestet.

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler und Ausrüstung (Szene „Characters“)
- `Art/Blender/Reborn_Enemies.blend`, `Reborn_Boss.blend`, `Reborn_World.blend`, `Reborn_Spells.blend` – Gegner, Boss, Umgebung, Zauber
- `Art/Blender/Scripts/build_all.ps1` – rendert alle Blender-Grafiken
- `Assets/_Project/Scenes/Base.unity` – Basis (Startszene), `Assets/_Project/Scenes/Room_Test.unity` – Kampfraum
- `Art/Blender/Reborn_Base.blend`, `Art/Blender/Scripts/build_base.py`, `build_npcs.py` – Grafik der Basis und der NPCs
- `Assets/_Project/Scripts/Economy/` – Gold, Händler-Waren, Schmiede-Stufen
- `Assets/_Project/Scripts/Editor/RebornBuilder.Town.cs` – baut die Stadt (Szene `Base`), `Assets/_Project/Scripts/World/Wanderer.cs` – laufende Bewohner
- `Art/Blender/Reborn_Town.blend`, `Art/Blender/Scripts/build_town.py`, `build_villagers.py` – Grafik der Stadt und der Stadtfiguren
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core, World)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
- `Art/Blender/Scripts/build_props.py`, `build_tiles.py` – Gegenstände und Kacheln (Szenen „Props“, „Tiles“)
- `Art/Blender/Scripts/build_spells.py` – Zauber-Grafik (Szene „Spells“)
- `Assets/_Project/Scripts/Magic/` – Mana, Zauberbuch, Effekte
- `Tools/PlayTests/` – automatische Play-Tests
- `Assets/_Project/Scenes/Guild_Warrior.unity`, `Guild_Mage.unity`, `Guild_Assassin.unity`, `Cave.unity` – begehbare Räume (Meilenstein 7b)
- `Art/Blender/Reborn_Interiors.blend`, `Art/Blender/Scripts/build_interiors.py`, `build_guildmasters.py` – Grafik der Räume und Gildenmeister
- `Assets/_Project/Scripts/Guilds/` – Gilden, Ränge, Quests (Meilenstein 7c); `Assets/_Project/Scenes/Forest.unity` – Wald
- `Art/Blender/Reborn_Forest.blend`, `Art/Blender/Scripts/build_forest.py` – Grafik des Waldes
- `Assets/_Project/Scripts/Enemies/BossAI.cs`, `RootSpike.cs`, `Assets/_Project/Scripts/UI/BossBar.cs` – Golem-Boss (Meilenstein 8)
- `Art/Blender/Scripts/build_boss_extras.py`, `build_interiors2.py`, `build_guildmembers.py` – Boss-Extras, neue Möbel, Gildenmitglieder
- `Assets/_Project/Scripts/Enemies/ThornWave.cs`, `ChargeTelegraph.cs`, `UltimateZone.cs`; `Art/Blender/Scripts/build_boss2.py`, `build_boss2_fx.py` – Boss Stufe 2 (Meilenstein 8b)

## Meilenstein 9: Schlüssel-Portal, Frostgipfel, Gilden-Fähigkeiten, Handwerk, Begleiter

Status: beschlossen. Umsetzung in zwei Teilen: 9a und 9b.

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| D1 | Frostschlüssel | Der Golem lässt beim ersten Sieg fest den Frostschlüssel fallen. Bei späteren Siegen nur, solange das Siegel-Portal noch nicht geöffnet ist und der Spieler keinen Schlüssel hat |
| D2 | Siegel-Portal | Drittes Portal in der Höhle, grau und versiegelt. Mit Schlüssel und E (Interagieren) wird es blau und bleibt offen (gespeichert), der Schlüssel wird verbraucht. Ohne Schlüssel Hinweis „Versiegelt – ein Schlüssel fehlt“ |
| D3 | Frostgipfel | Neue Szene, 28 × 18 Kacheln (Schnee, Eis, gefrorene Bäume, Eisfelsen). 4 Eiswölfe (60 Leben, schnell, 12 Schaden), 3 Frostgeister (70 Leben, schweben, schießen Eissplitter aus der Ferne, 10 Schaden; erster Fernkampf-Gegner). Hinten eine Arena mit dem Boss |
| D4 | Eisdrache | 3000 Leben, wacht bei Nähe auf, eigene Lebensleiste. Eisatem (Kegel, 1 s Warnung), Eisregen (5 Warnkreise, Eiszapfen), Flügelschlag (Rückstoß-Welle). Ab 50 %: Flugphase 5 s unverwundbar, ruft 2 Frostgeister. Ultimate „Blizzard“: 5 s Aufladung, ganze Arena, Schutz nur hinter 3 Eissäulen, sonst 60 % Leben Schaden. Erscheint wie der Golem nach 10 Minuten wieder |
| D5 | Drachen-Beute | Erster Sieg: Drachenschuppe × 3, Eiskristall × 2, 600 Gold, 1000 EP. Danach Gold und Eiskristalle |
| D6 | Gilden-Fähigkeit | Ab Rang 2. Taste F, Controller Steuerkreuz rechts, eigener Handy-Knopf. Krieger Wirbelschlag (Rundumschlag, 2× Angriff, 8 s Abklingzeit). Magier Kettenblitz (springt auf 3 Gegner, 15 Mana, 6 s). Assassine Schattensprung (hinter den nächsten Gegner, nächster Treffer 2× Schaden, 7 s) |
| D7 | Handwerk | Schmied-Reiter „Herstellen“. Wolfsfell-Rüstung (Brust, Verteidigung 3, Leben +15; 5 Wolfsfell + 50 Gold). Seidenrobe (Brust, Intelligenz +3, Verteidigung 1; 4 Spinnenseide + 50 Gold). Heiltrank (3 Schleimgel + 10 Gold). Frostklinge (Angriff 9; 3 Drachenschuppen + 2 Eiskristalle + 200 Gold) |
| D8 | Begleiter | Wolfspfeife beim Händler (250 Gold). Ruft den Wolfswelpen: folgt, greift Gegner in 4 Einheiten Nähe an, steigt bis Stufe 3 auf (4 / 7 / 10 Schaden). Bei 0 Leben fällt er 20 s aus und steht wieder auf. Wird gespeichert |
| D9 | Spielstand | Version 7: geöffnete Portale, Drache (Zeit wie beim Golem), Begleiter (Stufe, EP). Alte Stände laden weiter |
| D10 | Arbeitsteilung | Kern-Logik und Prüfung macht die Hauptsitzung (Opus). Blender-Grafik, Szenen-Aufbau Frostgipfel und Test-Schnipsel günstige Sonnet-Hilfsagenten. Doku Haiku |

### Umsetzung 9a (Schlüssel-Portal, Frostgipfel, Eisdrache)

#### Was gebaut ist

- **Welt-Schalter:** `World/WorldFlags.cs` – dauerhafte Schalter der Welt, im Spielstand Version 7 im Feld `worldFlags`.
- **Siegel-Portal:** `World/SealedPortal.cs` erbt von `Portal`. Dafür hat `Portal` jetzt eine virtuelle Methode `CanEnter`. Mit dem Frostschlüssel öffnet E das Siegel (Sprechblase „E: Siegel öffnen“), der Schlüssel wird verbraucht und das Portal bleibt offen. Ohne Schlüssel erscheint „Versiegelt – ein Schlüssel fehlt“. Das Portal `Portal_Frost` liegt links in der Höhle, Ankunft „Frost“.
- **Schlüssel:** `BossAI` lässt den Frostschlüssel (`bonusKey`) beim ersten Sieg fallen. Bei späteren Siegen nur, wenn das Siegel noch zu ist und der Spieler keinen Schlüssel hat.
- **Slime mit Fernkampf:** `SlimeAI` hat die Felder `projectilePrefab`, Schussweite 5,5, Abstand halten 3,2, Abklingzeit 2,2 s und Schaden 10. Er hält Abstand zum Spieler.
- **Eissplitter:** `Enemies/IceShard.cs` – Geschoss ohne Physik.
- **Eisdrache:** `Enemies/DragonAI.cs`
  - Eisatem: Kegel 4,2 Einheiten, 30°, 1 s Warnung, 30 Schaden, alle 5 s.
  - Eisregen: 5 Eiszapfen, 25 Schaden, alle 9 s.
  - Flügelschlag (nah): 15 Schaden mit Rückstoß, alle 6 s.
  - Flugphase bei 50 %: 5 s unverwundbar, ruft 2 Frostgeister.
  - Blizzard: erst nach 25 s, dann alle 35 s, 5 s Aufladung, 60 % Schaden außer hinter einer Eissäule. Die Deckung wird per Linecast auf `IcePillar` geprüft.
- **Eissäulen:** `Enemies/IcePillar.cs` steht in einer eigenen Datei, weil Unity MonoBehaviours nur speichert, wenn Dateiname und Klassenname gleich sind. Vorher ging dadurch eine Säule verloren.
- **Lebensleiste:** `Enemies/IBossBarSource.cs` ist die gemeinsame Schnittstelle für die Leiste von Golem und Drache, `ActiveBoss.Current` liefert den wachen Boss.
- **Wiederkehr:** Der Drache erscheint wie der Golem nach 10 Minuten wieder. `SaveSystem` ist auf Version 7.
- **Gegenstände:** `frostschluessel`, `eiskristall`, `drachenschuppe`.
- **Frostgipfel (Builder):** `RebornBuilder.Frost.cs` baut die Szene „Frost“ (28 × 18 Kacheln): 4 Eiswölfe (60 Leben, 12 Schaden), 3 Frostgeister (70 Leben, Fernkampf), Arena mit Eisdrache und 3 Eissäulen, Rückkehr-Portal in die Höhle. Die Datei hat ein Sonnet-Hilfsagent geschrieben, die Hauptsitzung hat sie geprüft.
- **Drache:** Bild 1,6-fach vergrößert (vorher kaum größer als der Spieler), Trefferkreis 0,95.

#### Grafik (Blender)

Grafik von einem Sonnet-Hilfsagenten nach Vorgabe erstellt.

- `build_frost.py`: Kacheln, Deko, Portale, Effekte, Icons.
- `build_frost_creatures.py`: Eiswolf (aus dem Wolf umgefärbt) und Frostgeist.
- `build_dragon.py`: Eisdrache, 256 px, 8 Animationen.
- `build_all.ps1`: neue Gruppen `frost`, `frostcreatures`, `dragon`.

#### Tests

- Neuer Test `frosttest.ps1` mit F1–F15, 29 Prüfungen: Siegel zu, ohne und mit Schlüssel; bleibt offen nach Neustart; Reise zum Frostgipfel; Bewohner; Frostgeist schießt; Drache schläft und erwacht; Atem; Eisregen; Flügelschlag; Flugphase; Blizzard ungeschützt (60 %) und hinter Säule (geschützt); Sieg mit Beute; Wiederkehr nach 10 Minuten mit kleinerer Beute; Tod → Stadt.
- Boss-Test S11/S12 prüft jetzt den Frostschlüssel und Version 7.
- Snippets `fr_*.cs`.

#### Bekannte Grenzen

- Eissplitter fliegen durch Wände.
- Der Drache hat im Flug keinen eigenen Höhen-Effekt, nur die Animation.
- Die Säulen-Deckung gilt nur für die Linie zum Drachen.
- Eiswolf-Todesanimation etwas kleiner skaliert.
- Grafik des Drachen eher hell.

## Meilenstein 10: Beta-Release (Hauptmenü, Einführung, Einstellungen, Browser-Version)

Status: beschlossen, Umsetzung in 10a, 10b, 10c; 9b folgt danach als Update 0.10.

### Entscheidungen

| # | Thema | Entscheidung |
|---|---|---|
| D1 | Reihenfolge | Meilenstein 10 kommt vor 9b. 9b wird das erste Update „0.10“ |
| D2 | Veröffentlichen auf itch.io | Claude baut eine fertige ZIP-Datei für den Browser. Der Nutzer lädt sie mit seinem Konto hoch; Claude gibt keine Passwörter oder Schlüssel ein. GitHub Pages nur nach ausdrücklichem Ja |
| D3 | Version | „Beta 0.9.0“. Seite Neuigkeiten mit den Änderungen je Version und „Updates kommen bald!“ |
| D4 | Hauptmenü | Eigene Startszene: „RE:BORN – ein Mini-Projekt von Victory“. Weiter (nur mit Spielstand), Neues Spiel (Nachfrage, wenn ein Stand existiert), Einstellungen, Neuigkeiten, Mitwirkende, Beenden (nur PC). Bedienbar mit Maus, Tastatur, Controller und Touch |
| D5 | Pause-Menü | Bekommt Einstellungen und „Zum Hauptmenü“. Vorher wird gespeichert |
| D6 | Einstellungen | Getrennt vom Spielstand, auch im Browser. Steuerung neu belegbar (Tastatur und Controller getrennt, Zurücksetzen, Hinweis bei Doppelbelegung). Sprache Deutsch/English (erster Start nach Systemsprache). Ton (Gesamt, Effekte). Grafik (Vollbild, Qualität, Bildrate 30/60/unbegrenzt). Oberfläche (Größe 90/100/115 %, Schadenszahlen, Handy-Knöpfe an/aus/automatisch) |
| D7 | Sprache (Technik) | Der deutsche Text ist der Schlüssel. Englische Tabelle als eigene Datei, englische Gegenstandsnamen und -beschreibungen, Übersetzungs-Komponente an festen Beschriftungen. Übersetzung durch Haiku, Prüfung durch die Hauptsitzung |
| D8 | Einführung | Beim Neuen Spiel begleitet Mentor Aldric durch 7 Schritte (Laufen, Angreifen, Ausweichen, Zauber, Menü/Inventar, Händler, Höhle/Portal) mit Sprechblasen, Hinweis und Zielpfeil. Belohnung: 50 Gold und 3 kleine Heiltränke. Überspringbar, nur einmal, startet nur über das Hauptmenü |
| D9 | Kleine Fixes | Wegwerfen (1 oder alle, 2 s Aufhebsperre, Ausgerüstetes erst ablegen). Löschen mit Nachfrage. Ortsname beim Szenenwechsel. Hinweis „Gespeichert“ |
| D10 | Sound | Kurze Effekte werden im Code erzeugt (keine fremden Dateien). Musik später als Update |
| D11 | Browser-Optimierung | Kleinere Datei (Code-Stripping, Texturkompression, gzip). Eigene Ladeseite mit Ladebalken und „Ein Mini-Projekt von Victory“. 60 Bilder/s, 16:9, Vollbild-Knopf. Speichern im Browser wird geprüft. Lokaler Test im eingebauten Browser |
| D12 | Tests schützen Einstellungen | Tests nutzen Standard-Einstellungen nur für die Sitzung. Tastenbelegung und Sprache des Nutzers bleiben unverändert |
| D13 | Arbeitsteilung | Hauptsitzung: Kern-Abläufe und Prüfung. Sonnet: Oberflächen-Builder, Titelbild und Mentor-Grafik, erzeugte Sounds, Test-Schnipsel. Haiku: Übersetzung, Neuigkeiten und Credits, Doku |
