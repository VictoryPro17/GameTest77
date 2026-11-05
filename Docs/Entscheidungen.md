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

## Wichtige Dateien

- `Art/Blender/Reborn_Characters.blend` – Spieler (Szene „Characters“) und Waldschleim (Szene „Slime“)
- `Assets/_Project/Scenes/Room_Test.unity` – Test-Raum
- `Assets/_Project/Scripts/` – Spiel-Code (Player, Combat, Enemies, Loot, Progression, UI, Animation, Core)
- `Assets/_Project/Scripts/Editor/RebornBuilder.cs` – baut alles automatisch
