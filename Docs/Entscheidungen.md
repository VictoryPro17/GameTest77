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
```

Danach in Unity: Menü **Reborn → Meilenstein 1 bauen** (baut Sprites-Import, Daten, Prefabs und die Szene `Room_Test` neu).

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
| D13 | Tastatur/Touch nur teilweise getestet | nachgetestet per Controller-Simulation, Touch auf echtem Handy steht noch aus |

Spielprobleme aus Review 1:
- **Keine Schutzzeit nach einem Treffer:** `Health.hitInvulnerability` eingeführt, beim Spieler 0,6 s.
- **Schleim greift sofort an:** Startposition (0,4) → (4,−4).
- **Figuren zu klein:** Kamera `orthographicSize` 6 → 3,5.

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

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler (Szene „Characters“) und Waldschleim (Szene „Slime“)
- `Assets/_Project/Scenes/Room_Test.unity` – Test-Raum
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
