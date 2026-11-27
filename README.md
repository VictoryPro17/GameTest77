# RE:BORN

**Echoes of Eternity** – ein 2D-Top-Down-Fantasy-Action-RPG

Ein Mini-Projekt von Victory · Beta 0.9.0

**Demo im Browser spielen:** https://okhomina.dev/projekte/reborn/#demo

<p align="center">
  <img src="Docs/Bilder/m10a_hauptmenue.png" width="720" alt="Hauptmenü von RE:BORN">
</p>

<p align="center">
  <img src="Docs/Bilder/m9a_frostgipfel.png" width="340" alt="Frostgipfel">
  <img src="Docs/Bilder/m9a_eisdrache.png" width="340" alt="Der Eisdrache">
</p>

<p align="center">
  <img src="Docs/Bilder/m9a_blizzard.png" width="340" alt="Blizzard-Angriff des Eisdrachen">
  <img src="Docs/Bilder/m10a_einstellungen.png" width="340" alt="Einstellungen">
</p>

---

## English summary

RE:BORN – Echoes of Eternity is a top-down 2D fantasy action RPG made in Unity 6 (URP 2D) by Victory as a hobby project. You fight slimes, wolves and spiders, cast water and fire magic, equip gear, trade in a walled city, and join one of three guilds to take on quests and bosses. The game is in beta (version 0.9.0), supports German and English, and can be played with keyboard, mouse or controller (touch controls exist but are not yet tested on real phones).

---

## Über das Spiel

RE:BORN spielt in einer Welt aus Stadt, Höhlen, Wäldern und einem verschneiten Gipfel. Die Kamera blickt schräg von oben auf die Figur; Grafik und Animationen werden in Blender gerendert und als 128-Pixel-Sprites eingesetzt. Gold, Ausrüstung und Magie bauen den Charakter Schritt für Schritt aus. Die Spielanleitung mit allen Details steht in [Docs/Spielanleitung.md](Docs/Spielanleitung.md).

## Features

### Kampf & Magie
- Nahkampf mit Angriff und Ausweichen, Schutzzeit nach Treffern
- Vier Zauberplätze: Wasserkugel, Wasserdrache, Feuerball, Flammenring (je Stufe 1–10)
- Mana, Meditieren und Mana-Training
- Heiltränke und Manatränke
- Schadenszahlen, Ausrufe und Bannertexte beim Zaubern

### Welt & Orte
- **Stadt** mit Stadtmauer, Soldaten, Marktplatz, Läden und Wohnvierteln
- **Gildenhallen** (Krieger, Magier, Assassinen) und **Höhle** als begehbare Räume
- **Wald** mit Schattenwölfen und Riesenspinnen
- **Frostgipfel** mit Eiswölfen und Frostgeistern, die aus der Ferne schießen
- Portale zwischen den Orten, ein Siegel-Portal mit Schlüssel

### Bosse
- **Waldwächter** (Golem) mit zwei Stufen, Dornenwelle, Ansturm und Zorn des Waldes
- **Eisdrache** mit Flugphase, Eisregen und Blizzard
- Besiegte Bosse kehren nach 10 Minuten zurück

### Gilden & Quests
- Drei Gilden mit eigenen Vorteilen und Gildenwaffen
- Rang-Aufstieg nach Quests, Gildenmeister mit Quest-Fenster
- Quest-Hinweis im HUD

### Wirtschaft
- Gold als Währung (Gegner, Quests, Bosse)
- Händler zum Kaufen und Verkaufen, Schmied zum Verbessern von Ausrüstung (Stufe +1 bis +5)
- Gegenstände wegwerfen, einzeln oder alle auf einmal

### Einführung und Ton
- Einführung mit Mentor Aldric nach „Neues Spiel“: 7 Schritte, Sprechblasen, Zielpfeil, überspringbar, Belohnung
- 15 Soundeffekte (im Code erzeugt), Regler für Gesamt- und Effekt-Lautstärke

### Komfort & Einstellungen
- Hauptmenü mit Weiter, Neues Spiel, Einstellungen, Neuigkeiten und Mitwirkende
- Pause-Menü mit Einstellungen und Rückkehr zum Hauptmenü
- Deutsch und Englisch (beim ersten Start nach Systemsprache)
- Tastenbelegung für Tastatur und Controller getrennt änderbar
- Ton, Vollbild, Grafikqualität, Bildrate, Oberflächengröße, Schadenszahlen, Handy-Knöpfe
- Hinweis „Gespeichert“ beim Speichern

## Steuerung

Die Standardbelegung. Alle Aktionen (außer Bewegung über die Maus) lassen sich im Menü unter **Einstellungen → Tasten** ändern.

| Aktion | Tastatur | Maus | Controller | Handy |
|---|---|---|---|---|
| Laufen | WASD / Pfeiltasten | – | linker Stick | Joystick links |
| Angriff | J | linke Maustaste | West-Taste (X / □) | Knopf „Angriff“ |
| Ausweichen | Leertaste / Linke Umschalt | – | Süd-Taste (A / ✕) | Knopf „Ausw.“ |
| Zauber 1–4 | 1, 2, 3, 4 | – | RB, RT, LB, LT | Knöpfe Z1–Z4 |
| Meditieren (halten) | M | – | Ost-Taste (B / ○) | Knopf „Medit.“ |
| Heiltrank | Q | – | Nord-Taste (Y / △) | Knopf „Trank“ |
| Manatrank | R | – | Steuerkreuz oben | HUD-Knopf |
| Sprechen / Interagieren | E, Enter | – | Steuerkreuz unten | Knopf „Sprechen“ (nahe am NPC) |
| Menü öffnen / schließen | Tab, I, Esc | – | Start | Knopf „Menü“ oben rechts, im Menü „X“ |
| Im Menü wählen | Pfeiltasten | – | Steuerkreuz / Stick | Antippen |
| Anlegen / Benutzen / Ablegen | Enter | – | Süd-Taste | Knopf im Detail-Fenster |

## Spielstand & Einstellungen

- **Spielstand:** `%USERPROFILE%\AppData\LocalLow\DefaultCompany\GameTest77\reborn_save.json` (Windows). Gespeichert wird beim Level-Aufstieg, beim Schließen von Menüs und Handelsfenstern, vor Szenenwechseln, beim Kaufen, Verkaufen und Verbessern, beim Öffnen einer Truhe, beim Pausieren der App (Handy) und beim Beenden.
- **Spielstand löschen:** im Reiter „Charakter“. Danach startet das Spiel mit Startwerten in der Stadt.
- **Einstellungen:** getrennt vom Spielstand in `PlayerPrefs` (Schlüssel `reborn.settings`), also auch im Browser verfügbar.
- **Schutz der Spielstände:** Die Play-Tests sichern den Spielstand vor jedem Lauf mit SHA-256-Prüfsumme und spielen ihn danach zurück, auch wenn ein Test abbricht.

Das Dateiformat ist versioniert (aktuell Version 7); ältere Stände laden weiter. Details dazu in [Docs/Entwicklung.md](Docs/Entwicklung.md).

## Roadmap

Geplant, in dieser Reihenfolge (Stand Beta 0.9.0; Einführung, Ton und Browser-Demo sind umgesetzt):

1. **Gilden-Fähigkeiten** (Meilenstein 9b): Taste F, Ability ab Gildenrang 2
2. **Handwerk** beim Schmied, **Wolfswelpe** als Begleiter
3. **Musik**

Die Änderungen je Version stehen in der [CHANGELOG.md](CHANGELOG.md).

## Projektstruktur

```
GameTest77/
├── Art/                 Blender-Dateien (.blend) und Skripte zum Rendern der Grafik
│   └── Blender/Scripts/ build_all.ps1 und Blender-Python-Skripte (build_*.py, reborn_lib.py)
├── Assets/              Unity-Assets
│   └── _Project/        Eigene Inhalte des Spiels
│       ├── Data/        ScriptableObjects (Gegenstände, Gilden, Waren)
│       ├── Fonts/       Schriften (Cinzel, Alegreya Sans) mit Lizenztexten
│       ├── Input/       Steuerung (RebornControls.inputactions)
│       ├── Prefabs/     Prefabs (Spieler, Gegner, UI, Zauber)
│       ├── Resources/   Texte, Lokalisierung (Loc_en.json), Credits, Neuigkeiten
│       ├── Scenes/      Szenen (Base = Stadt, Room_Test, Gilden, Höhle, Wald, Frost)
│       ├── Scripts/     C#-Code, nach Themen gegliedert (siehe Docs/Entwicklung.md)
│       └── Sprites/     Sprites, Animationen, Icons
├── Docs/                Dokumentation (Spielanleitung, Entwicklung, Entscheidungen, Bilder)
├── Packages/            Unity-Pakete
├── ProjectSettings/     Unity-Projekteinstellungen
├── Screenshots/         Screenshots aus den Play-Tests
└── Tools/               Play-Tests, Lokalisierungs- und Build-Werkzeuge
    ├── Localization/    extract_strings.ps1 (Übersetzungsschlüssel sammeln)
    ├── Build/           build_web.ps1 (Browser-Demo bauen)
    └── PlayTests/       Automatische Tests über den offenen Unity-Editor
```

## Entwicklung

- **Engine:** Unity 6000.6.8f1 mit C#, Universal Render Pipeline (URP 2D), neues Input System
- **Grafik:** 3D-Modelle in Blender 5.2, gerendert zu Pixel-Sprites in 4 Blickrichtungen
- **Zielplattformen:** Windows (Editor und Build), Browser (Web-Demo, Meilenstein 10c), Android (Touch-Steuerung)
- **Spiel bauen:** In Unity über das Menü **Reborn → Spiel neu bauen** (`RebornBuilder.BuildAll`). Der Builder erzeugt Szenen, Prefabs, Menüs, Daten und Importeinstellungen neu.
- **Grafik neu rendern:** `powershell -File Art/Blender/Scripts/build_all.ps1` (optional `-Only boss,spells` für einzelne Gruppen). Danach den Unity-Builder ausführen.
- **Lokalisierung:** Der deutsche Text ist der Schlüssel (`Loc.T("…")`). Die englische Übersetzung steht in `Assets/_Project/Resources/Localization/Loc_en.json`. `Tools/Localization/extract_strings.ps1` sammelt alle deutschen Texte und meldet fehlende englische Einträge.
- **Play-Tests:** `powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1` (Unity-Editor muss offen und im Stopp-Modus sein). Details in [Tools/PlayTests/README.md](Tools/PlayTests/README.md).

- **Browser-Demo:** `powershell -File Tools/Build/build_web.ps1` (Unity-Editor offen, nicht im Play-Modus). Details in [Docs/Entwicklung.md](Docs/Entwicklung.md#web-demo-bauen).

Die technischen Details (Architektur, Speicherformat, Konventionen, Grafik-Pipeline) stehen in [Docs/Entwicklung.md](Docs/Entwicklung.md).

## Dokumentation

- [Docs/Spielanleitung.md](Docs/Spielanleitung.md): Einstieg, Steuerung, Kampf, Magie, Handel, Gilden, Orte, Bosse
- [Docs/Entwicklung.md](Docs/Entwicklung.md): Architektur, Speicherformat, Lokalisierung, Grafik-Pipeline, Tests, Konventionen
- [Docs/Entscheidungen.md](Docs/Entscheidungen.md): Projektentscheidungen und Ergebnisse aller Meilensteine
- [CHANGELOG.md](CHANGELOG.md): Änderungen je Version
- [Tools/PlayTests/README.md](Tools/PlayTests/README.md): Play-Tests

## Mitwirkende

- **Idee, Spieldesign und Leitung:** Victory
- **Programmierung, Grafik (Blender) und Tests:** mit Unterstützung von Claude (Anthropic)
- **Engine:** Unity 6
- **3D-Grafik:** gerendert mit Blender
- **Schriften:** Cinzel und Alegreya Sans, SIL Open Font License 1.1 (Lizenztexte in `Assets/_Project/Fonts`)

Danke an alle Beta-Tester!

## Lizenz

Lizenz: noch nicht festgelegt – alle Rechte vorbehalten.
