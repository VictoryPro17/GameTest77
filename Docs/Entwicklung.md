# RE:BORN – Entwicklerdokumentation

Technische Übersicht für alle, die am Projekt arbeiten. Die Entscheidungen hinter den Meilensteinen stehen in [Entscheidungen.md](Entscheidungen.md), die Spieler-Sicht in [Spielanleitung.md](Spielanleitung.md).

---

## Inhalt

1. [Projekt und Werkzeuge](#projekt-und-werkzeuge)
2. [Architektur](#architektur)
3. [Spielstand (Speicherformat)](#spielstand-speicherformat)
4. [Einstellungen](#einstellungen)
5. [Lokalisierung](#lokalisierung)
6. [Grafik-Pipeline (Blender)](#grafik-pipeline-blender)
7. [Builder](#builder)
8. [Play-Tests](#play-tests)
9. [Konventionen](#konventionen)
10. [Git-Workflow](#git-workflow)

---

## Projekt und Werkzeuge

| Bereich | Stand |
|---|---|
| Engine | Unity 6000.6.8f1, C#, Universal Render Pipeline (URP 2D) |
| Eingabe | Neues Input System, Aktionen in `Assets/_Project/Input/RebornControls.inputactions` |
| Grafik | Blender 5.2, Skripte in `Art/Blender/Scripts` |
| Tests | PowerShell 5.1, steuert den Unity-Editor über die Unity CLI (`Tools/PlayTests`) |
| Zielplattformen | Windows-Editor und Build, Android (Touch), Browser (in Vorbereitung) |
| Version | Beta 0.9.0 (`GameSettings.Version`) |

Wichtige Pfade:

- `Assets/_Project/Scenes/`: Szenen. `Base` (Stadt, Startszene, Build-Index 0), `Room_Test` (Kampfraum), `Guild_Warrior`, `Guild_Mage`, `Guild_Assassin`, `Cave`, `Forest`, `Frost`.
- `Assets/_Project/Resources/Text/`: `News_de.txt` (Neuigkeiten im Hauptmenü), `Credits_de.txt`.
- `Docs/Entscheidungen.md`: Entscheidungslog aller Meilensteine.

---

## Architektur

Der Code liegt unter `Assets/_Project/Scripts/`, gegliedert nach Themen. Die Namensräume folgen in der Regel den Ordnern (`Reborn.Combat`, `Reborn.Magic`, …). Der Builder liegt im Namensraum `Reborn.EditorTools`.

| Ordner | Aufgabe | Wichtige Klassen |
|---|---|---|
| `Animation/` | Richtungs-Animation der Sprites, Ausrüstungs-Ebenen | `DirectionalSpriteAnimator`, `SpriteAnimationSet`, `EquipmentVisuals` (folgt `OnFrameChanged` des Körpers), `Facing` |
| `Combat/` | Leben, Schaden, Angriff, Tod | `Health` (Leben, Schutzzeit, `LastHitBySpell`), `PlayerAttack` (Trefferkreis), `DeathHandler`, `DamageFlash` |
| `Core/` | Übergreifendes: Kamera, Einstellungen, Übersetzung | `CameraFollow`, `GameSettings`, `Loc` |
| `Economy/` | Gold, Waren, Schmiede-Stufen | `PlayerWallet`, `ShopData`, `UpgradeBook` |
| `Editor/` | Builder, erzeugt Szenen, Prefabs und Daten (nur im Editor) | `RebornBuilder` + Partial-Klassen (siehe [Builder](#builder)) |
| `Enemies/` | Gegner-KI und Bosse | `SlimeAI` (allgemeine Kreaturen-KI für Schleim, Wolf, Spinne, Eiswolf), `BossAI` (Golem, zwei Stufen), `DragonAI` (Eisdrache), `RootSpike`, `ThornWave`, `ChargeTelegraph`, `UltimateZone`, `IcePillar`, `IceShard`, `IBossBarSource`, `ActiveBoss` (Lebensleiste) |
| `Guilds/` | Gilden, Ränge, Quests | `GuildData` (ScriptableObject), `PlayerGuild` (am Spieler) |
| `Inventory/` | Inventar, Ausrüstung, Tränke | `PlayerInventory`, `PlayerEquipment`, `PotionUser` |
| `Loot/` | Gegenstände, Seltenheit, Beute | `ItemData`, `ItemDatabase`, `ItemCategory`, `ItemRarity`, `EquipSlot`, `StatBlock`, `LootTable`, `LootDropper`, `Pickup` |
| `Magic/` | Mana, Zauber, Effekte | `PlayerMana`, `SpellBook`, `SpellData`, `SpellEffect` (Grundklasse), `SpellProjectile`, `FlameRing`, `WaterDragon`, `LightFlash`, `SpriteLoop`, `SpellKind`, `SpellElement` |
| `Player/` | Bewegung und Eingabe des Spielers | `PlayerController` |
| `Progression/` | Level, Erfahrung, Attribute | `PlayerProgression`, `PlayerStats` (berechnet die Werte) |
| `Save/` | Speichern und Laden | `SaveSystem`, `SaveData` |
| `UI/` | Alle Oberflächen | `HUD`, `MainMenu`, `GameMenu`, `SettingsView`, `RebindingView`, `InventoryView`, `MagicView`, `CharacterView`, `EquipmentView`, `ItemDetailView`, `ItemSlotView`, `TradeWindow`, `ShopView`, `SmithView`, `GuildView`, `BossBar`, `CastBanner`, `ChantText`, `DamageNumbers`, `LocationBanner`, `ConfirmDialog`, `LocalizedText`, `UiTheme`, `MobileControlsToggle` |
| `World/` | Szenen-Logik, Türen, Portale, NPCs | `Portal`, `SealedPortal`, `Door`, `SceneTravel` (Abdunkeln und Szenenwechsel), `SceneRules` (friedliche Szenen), `SpawnPoint`, `Npc`, `Wanderer`, `SpeechBubble`, `TreasureChest`, `WorldFlags`, `BlobShadow`, `WallColliderGuard` |

### Wichtige Abläufe

- **Schaden:** `Health` nimmt Schaden an, prüft die Schutzzeit und meldet Treffer. `PlayerStats` rechnet die Verteidigung ein: erlittener Schaden = max(1, round(Schaden × 20 / (20 + Verteidigung))).
- **Gilden-Quests:** `SlimeAI.OnAnyKilled` (Besiegen), das Inventar (Bringen) und `Health.LastHitBySpell` (Zauber-Quest) füttern `PlayerGuild`.
- **Bosse:** `BossAI` und `DragonAI` implementieren `IBossBarSource`. `ActiveBoss.Current` liefert den wachen Boss an die `BossBar`.
- **Szenenwechsel:** Portale und Türen erben von `Portal` (`CanEnter` ist virtuell). `SceneTravel` speichert, blendet ab und lädt die Szene. `SealedPortal` prüft den Schlüssel.
- **Welt-Schalter:** `WorldFlags` speichert dauerhafte Zustände (z. B. das geöffnete Siegel) im Spielstand.

---

## Spielstand (Speicherformat)

Datei: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\GameTest77\reborn_save.json` (`SaveSystem`). Aktuelle Version: **7** (`SaveData.CurrentVersion`). Fehlende Felder behalten beim Laden ihre Startwerte, ältere Versionen laden daher weiter.

| Version | Meilenstein | Neue Felder |
|---|---|---|
| 1 | 2 | Stufe, Erfahrung, Attribute, Inventar, Ausrüstung, geöffnete Truhen, Leben |
| 2 | 3 | Zauber-Stufe und -Erfahrung, Mana-Stufe, Mana-Erfahrung, aktuelles Mana |
| 3 | 6 | Gold, Schmiede-Stufen (`upgrades`), letzte Szene (nur Info) |
| 4 | 7c | Gilde (`guild`, Text, leer = keine), Rang, Quest-Nummer, Quest aktiv, Fortschritt |
| 5 | 8 | besiegte Bosse (`defeatedBosses`) |
| 6 | 8b | Siegzeitpunkt und Anzahl der Siege je Boss (`bossRecords`), Wiederkehr nach `SaveSystem.BossRespawnMinutes` (10) |
| 7 | 9a | Welt-Schalter (`worldFlags`), z. B. geöffnetes Siegel-Portal |

Regeln:

- Gespeichert wird bei Level-Aufstieg, beim Schließen von Menüs und Handelsfenstern, beim Öffnen von Truhen, beim Kaufen, Verkaufen und Verbessern, vor jedem Szenenwechsel, bei Pause (Handy) und beim Beenden.
- Das Spiel startet immer in der Stadt (`Base`). Das gespeicherte Feld `lastScene` ist nur eine Information.
- Beim Erhöhen der Version: neues Feld in `SaveData` anlegen, Kommentar „ab Version N (Meilenstein X)“ ergänzen, `CurrentVersion` erhöhen, Test für den alten Stand schreiben (der Basis-Test prüft Version 2 und 3).

---

## Einstellungen

`Core/GameSettings.cs` speichert die Einstellungen **getrennt vom Spielstand** in `PlayerPrefs` unter dem Schlüssel `reborn.settings` (als JSON). Das funktioniert auch im Browser.

| Feld | Standard | Werte |
|---|---|---|
| `language` | Systemsprache | `de` oder `en` |
| `masterVolume` / `sfxVolume` | 0,8 | 0–1 |
| `fullscreen` | aus | |
| `quality` | 2 | 0 niedrig, 1 mittel, 2 hoch (wird auf die Qualitätsstufen des Projekts verteilt) |
| `frameRate` | 60 | 30, 60 oder -1 (unbegrenzt) |
| `uiScale` | 1 | 0,9 / 1 / 1,15 (begrenzt auf 0,8–1,3, wirkt auf alle `CanvasScaler`) |
| `damageNumbers` | an | |
| `mobileControls` | 0 | 0 automatisch, 1 an, 2 aus |
| `bindings` | leer | Tastenbelegung als `SaveBindingOverridesAsJson` |

- Änderungen laufen über `GameSettings.Change(...)`: ändern, anwenden, speichern.
- Die Tastenbelegung lädt `RegisterActions` beim Start der Menüs. `StoreBindings` und `ResetBindings` speichern sie.
- `UseSessionDefaults()` ist für Tests: Standardwerte gelten nur für diese Sitzung, nichts wird gespeichert.

---

## Lokalisierung

- Der **deutsche Text ist der Schlüssel**: `Loc.T("Angriff")` gibt in Deutsch den Originaltext zurück und in Englisch die Übersetzung aus der Tabelle. `Loc.F("…{0}…", args)` für Formatierungen.
- Die Tabelle liegt in `Assets/_Project/Resources/Localization/Loc_en.json` (Einträge `{ "de": "…", "en": "…" }`). `Loc.HasEnglish(de)` prüft, ob ein Eintrag existiert.
- Gegenstandsnamen und -beschreibungen werden im Builder erzeugt und ebenfalls übersetzt.
- `Tools/Localization/extract_strings.ps1` sammelt alle deutschen Schlüssel:
  - erstes Literal jedes `Loc.T(`/`Loc.F(` in `Scripts/**/*.cs`
  - Builder-Daten in `Scripts/Editor/*.cs` (`Item(…)`, `Q(…)`, `Option(…)`, `MenuButton(…)`)
  - `RebindingView.Rows`, `LocationBanner.Names`
  - `extra_keys.txt` für Schlüssel, die nur über Konstanten laufen
  - Ergebnis: `Tools/Localization/strings_de.txt`. Fehlende englische Einträge stehen in `missing_en.txt`.
- Ablauf bei neuen Texten: deutschen Text in den Code schreiben, Skript laufen lassen, englische Übersetzung in `Loc_en.json` ergänzen, prüfen.

---

## Grafik-Pipeline (Blender)

Alle Sprites entstehen in Blender 5.2 und werden per Skript gerendert. Die gemeinsame Bibliothek ist `Art/Blender/Scripts/reborn_lib.py` (Render-Setup, Kamera, Comic-Material `cel` mit 3 Helligkeitsstufen, Kontur, Skelett, `Poser`).

**Stil und Maße**

- Kamera: 45° von oben, orthografisch. Figuren: 128 × 128 px bei 128 px pro Einheit. Boss und Drache: 256 px. Kacheln: 64 px. Zauber-Icons und Item-Icons: 48 px.
- Fußpunkt aller Figuren und Gegenstände: (0,5 / 0,2138) im Bild, damit die Y-Sortierung stimmt.
- Vier Blickrichtungen bei allen Figuren und Gegnern.
- Rand-Prüfung: `edge_pixels` meldet Figuren, die den Bildrand berühren.

**Gruppen (Reihenfolge in `build_all.ps1`)**

| Gruppe (`-Only`) | Skript | Ausgangsdatei (.blend) | Inhalt |
|---|---|---|---|
| `player` | `build_player.py` | – (legt neu an) | Grundfigur ohne Waffe |
| `equipment` | `build_equipment.py` | `Reborn_Characters.blend` | Ausrüstungsebenen, Gildenwaffen, Icons |
| `enemies` | `build_enemies.py` | – | Schleim, Wolf, Spinne |
| `boss` | `build_boss.py` | – | Golem (Stufe 1) |
| `props` | `build_props.py` | – | Gegenstände der Welt: Truhe, Bäume, Fels, Busch, Fackel |
| `tiles` | `build_tiles.py` | `Reborn_World.blend` | Bodenkacheln |
| `spells` | `build_spells.py` | – | Zauber und Zauber-Icons |
| `npcs` | `build_npcs.py` | `Reborn_Characters.blend` | Händler und Schmied (speichert nicht) |
| `base` | `build_base.py` | – | Gebäude der Basis aus Meilenstein 6 |
| `interiors` | `build_interiors.py` | – | Kacheln und Einrichtung der Gildenhallen und Höhle |
| `guildmasters` | `build_guildmasters.py` | `Reborn_Characters.blend` | Gildenmeister |
| `forest` | `build_forest.py` | – | Waldkacheln und Deko |
| `bossextras` | `build_boss_extras.py` | – | Wurzelstacheln, Warnfläche, Amulett-Icon |
| `interiors2` | `build_interiors2.py` | – | Große Möbel der Gildenhallen |
| `guildmembers` | `build_guildmembers.py` | `Reborn_Characters.blend` | Sechs Gildenmitglieder |
| `boss2` | `build_boss2.py` | `Reborn_Boss.blend` | Golem Stufe 2 (speichert nicht) |
| `boss2fx` | `build_boss2_fx.py` | – | Dornenring, Ansturm-Pfeil, Ultimate-Kreis |
| `town` | `build_town.py` | – | Stadt: Gebäude, Mauer, Tor, Kacheln |
| `villagers` | `build_villagers.py` | `Reborn_Characters.blend` | Elf Stadtbewohner (speichert nicht) |
| `frost` | `build_frost.py` | – | Frost-Kacheln, Deko, Portale, Effekte, Icons |
| `frostcreatures` | `build_frost_creatures.py` | `Reborn_Enemies.blend` | Eiswolf und Frostgeist |
| `dragon` | `build_dragon.py` | – | Eisdrache (256 px, 8 Animationen) |
| `menuart` | `build_menu_art.py` | – | Hauptmenü-Bild, Logo-Rahmen, Zielpfeil |
| `mentor` | `build_mentor.py` | `Reborn_Characters.blend` | Mentor Aldric (Grafik für die Einführung) |

Aufruf:

```
powershell -File Art/Blender/Scripts/build_all.ps1                 # alles
powershell -File Art/Blender/Scripts/build_all.ps1 -Only boss,spells
powershell -File Art/Blender/Scripts/build_all.ps1 -Blender "<Pfad zu blender.exe>"
```

- Standardpfad: `C:\Program Files\Blender Foundation\Blender 5.2\blender.exe`.
- Ein Durchlauf aller Gruppen dauert rund 30 Minuten (Ausrüstung allein über 20). Einzelne Gruppen mit `-Only` neu rendern.
- Blender-Warnungen auf stderr gelten nicht als Fehler. Nur `Traceback` und fehlende `RESULT`-Zeilen zählen.
- Speichern wird bis zu 6-mal wiederholt, wenn Unity eine PNG-Datei gerade einliest (`render_still`).
- Die Figur-Blend-Datei wird von `build_player.py` neu angelegt. Danach die Gruppen `equipment`, `npcs`, `guildmasters`, `villagers`, `mentor` ebenfalls neu rendern.
- Ausrüstung läuft als Ebenen: Der Körper ist „Holdout“ (unsichtbar, verdeckt aber das Teil), damit vorne und hinten stimmt. Der Filter `item:<id>` rendert nur einzelne Teile.

Nach dem Rendern in Unity den Builder ausführen (siehe unten), damit die Importeinstellungen stimmen.

---

## Builder

`Assets/_Project/Scripts/Editor/RebornBuilder.cs` baut alles neu. Menüpunkt: **Reborn → Spiel neu bauen** (`RebornBuilder.BuildAll`). Er ist auch per Unity-CLI aufrufbar.

| Datei | Baut bzw. richtet ein |
|---|---|
| `RebornBuilder.cs` | Einstieg (`BuildAll`), Projekt- und Sprite-Einstellungen, Hauptszene Kampfraum, gemeinsame UI, Build-Liste |
| `RebornBuilder.Base.cs` | Gemeinsame Bausteine und die Basis-Szene aus Meilenstein 6 (von der Stadt abgelöst) |
| `RebornBuilder.Theme.cs` | `UiTheme` und die 9-teiligen Rahmen (per Code, Pixel-Stil) |
| `RebornBuilder.Magic.cs` | Zauber am Spieler (`AddMagicToPlayer`) und Magie-Reiter im Menü |
| `RebornBuilder.Boss.cs` | Golem im Kampfraum, Frostschlüssel an der Beute, Lebensleiste |
| `RebornBuilder.Frost.cs` | Szene `Frost` (Frostgipfel, Eisdrache) und das Siegel-Portal in der Höhle |
| `RebornBuilder.Guilds.cs` | Wald-Szene `Forest` und ihre Sprites |
| `RebornBuilder.Interiors.cs` | Gildenhallen und Höhle: Einrichtung, Gildenmeister, Gildenmitglieder |
| `RebornBuilder.Menu.cs` | Hauptmenü-Szene |
| `RebornBuilder.Town.cs` | Szene `Base` (Stadt): Mauer, Gebäude, Läden, Bewohner |
| `RebornBuilder.World.cs` | Deko-Objekte |

Hinweise:

- Eine neue Szene gibt nicht benutzte Assets frei. Kacheln nach dem Szenenwechsel neu laden (`NewWorldScene` mit `tileLoader`), sonst entsteht schwarzer Boden. Der Builder bricht ab, wenn Boden oder Mauer leer sind.
- Mauern: Der Builder erzeugt die Mauer-Geometrie vor dem Speichern. `WallColliderGuard` prüft das beim Spielstart.
- Der Builder darf nicht im Play-Modus laufen. Vorher prüfen und nicht selbst beenden.
- `ConfigureTexture` importiert eine Textur nur neu, wenn sich die Einstellungen geändert haben (sonst überschreitet der Lauf das Zeitlimit der CLI).

---

## Play-Tests

Automatische Tests über den **offenen Unity-Editor** (`Tools/PlayTests`). Details: [Tools/PlayTests/README.md](../Tools/PlayTests/README.md).

```
powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1                 # alle Standard-Tests
powershell -ExecutionPolicy Bypass -File Tools\PlayTests\run_all.ps1 -Tests wall,kb  # einzelne Tests
```

Voraussetzungen: Projekt in Unity geöffnet, Play-Modus aus, Unity nicht minimiert (simulierte Eingaben kommen sonst nicht an). Während des Laufs (mehrere Minuten) Unity nicht bedienen.

| Name | Skript | Prüft |
|---|---|---|
| `look` | `looktest.ps1` | Sichtbare Ausrüstung, Schadenszahlen, Manatrank, Layout, Bildanzahl und Fußpunkt aller Animationssets, Treffer- und Sterbe-Animation, Schatten, Kacheln, Deko |
| `magic` | `magictest.ps1` | Mana, vier Zauber, Stufen, Meditieren, Speichern, Magie-Reiter |
| `wall` | `walltest.ps1` | Mauer-Collider, Laufen und Ausweichen gegen alle vier Mauern |
| `m2` | `m2test.ps1` | Truhe, Ausrüstung, Verteidigung, Trank, Attribute, Inventar, Speichern, Menü mit Controller und Tastatur, Spielstand löschen |
| `play` | `playtest.ps1` | Laufen, Kampf gegen Schleime, Beute, Level-Aufstieg |
| `iframe` | `iframetest.ps1` | Schutzzeit nach einem Treffer |
| `kb` | `kbtest.ps1` | Tastatur: D, J, Leertaste |
| `hp0` | `hp0test.ps1` | Mauern aus: kein Schaden, kein Tod |
| `base` | `basetest.ps1` | Start in der Stadt, friedlich, Händler, Schmied, Speichern Version 3, Portal hin und zurück, Tod, alter Spielstand |
| `town` | `towntest.ps1` | Stadt: Gebäude, Wege, Bewohner, Soldaten, Läden, Höhle, Übersichtsbilder `m7a_*` |
| `interior` | `interiortest.ps1` | Gildenhallen hinein und hinaus, Gildenmeister, Höhle → Portal → Kampfraum → Höhle, Gold über zehn Wechsel |
| `guild` | `guildtest.ps1` | Beitritt, Ränge, Quests, Wechsel, Zauber-Quest, Speichern Version 4, Wald mit Wolf und Spinne |
| `boss` | `bosstest.ps1` | Golem: Erwachen, Schlag, Wurzeln, Phasen, Ultimate, Sieg, Wiederkehr nach 10 Minuten |
| `frost` | `frosttest.ps1` | Siegel-Portal, Frostgipfel, Eisdrache (Atem, Eisregen, Flügelschlag, Flugphase, Blizzard), Wiederkehr |
| `menu` | `menutest.ps1` | Hauptmenü, Einstellungen, Sprache, Tasten, Pause-Menü, Wegwerfen, Ortsname |
| `diag` | `inputdiag.ps1` | Nur zur Fehlersuche, **nicht verlässlich**, nicht in der Standardliste |

Der Standardlauf enthält alle Tests außer `diag`.

**Schutz des Spielstands:** `run_all.ps1` sichert `reborn_save.json` mit SHA-256, startet die Tests, räumt auf und spielt den Stand zurück. Die Prüfsumme wird verglichen. Liegt schon eine alte Sicherung (`reborn_save.user_backup.json`), bricht der Lauf ab. Einstellungen und Tastenbelegung des Spielers bleiben unangetastet, weil die Tests nur Sitzungs-Standardwerte nutzen (`UseSessionDefaults`). Diagnosen nur über `run_all.ps1` laufen lassen, nie über eigene Skripte ohne Sicherung.

**Aufbau**

- `lib.ps1`: gemeinsame Funktionen (`Eval`, `StartPlay`, `StopPlay`, `Shot`, `Check`, `CheckRange`, `CheckTrue`, `WaitReady`).
- `cs/`: kleine C#-Schnipsel, die im Editor laufen. `//#HEADER` setzt `cs/_header.cs` ein. Platzhalter wie `%PX%` füllt `Eval 'datei.cs' @{ PX = '1' }`.
- Platzhalter nie `Keys`, `Values` oder `Count` nennen. Das sind PowerShell-Eigenschaften von Tabellen.
- Kurze Animationen (Angriff, Ausweichen) zählt der Test über `OnFrameChanged`.

**Bekannte Grenzen der Tests:** Mausklicks im Menü werden nicht getestet. Touch ist auf einem echten Gerät noch nicht geprüft. Die Bildprüfung kontrolliert nur Namen und Größe; abgeschnittene Figuren erkennt nur die Randprüfung.

---

## Konventionen

- **Domain Reload aus:** Im Play-Modus wird der Code nicht neu geladen, statische Felder behalten ihren Wert. Statische Zustände deshalb beim Start zurücksetzen:

  ```csharp
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics() { /* statische Felder und Ereignisse zurücksetzen */ }
  ```

  Ereignisse abmelden (`SceneManager.sceneLoaded -= …`), sonst hängen alte Abonnenten an der nächsten Sitzung. Test-Geräte (virtuelle Tastatur) entfernt `StartPlay` vor jedem Start.
- **Dateiname = Klassenname:** Unity speichert MonoBehaviour-Skripte nur, wenn Dateiname und Klassenname gleich sind. Deshalb steht `IcePillar` in einer eigenen Datei und nicht zusammen mit `IceShard` oder `DragonAI`.
- **Zeichen in Skripten:** `.ps1`-Dateien sind **reines ASCII ohne BOM**, da Windows PowerShell 5.1 Umlaute sonst falsch liest. Sonderzeichen in Testerwartungen daher als `·` oder ähnlich ausdrücken.
- **Namen für Oberflächen:** Bedienbare Teile behalten ihre Namen, damit die Tests weiter laufen.
- **Deutsch als Schlüssel:** Neue Texte immer mit `Loc.T` oder `Loc.F` ausgeben.
- **Neue Gegner:** Die allgemeine Kreaturen-KI `SlimeAI` nutzt `creatureId` und Werte pro Art. Eine eigene KI-Klasse ist nur nötig, wenn sich das Verhalten ändert.
- **Neue Gegenstände:** Eintrag im Builder (`Item(…)`), Icon in `Art/Blender` (48 px), Lokalisierungseintrag in `Loc_en.json`.
- **Tests vor Commit:** Der relevante Test läuft mit `-Tests …`. Ein vollständiger Lauf vor jedem Meilenstein-Abschluss.

---

## Git-Workflow

- Jeder Meilenstein bekommt einen eigenen Branch: `feature/meilenstein-N` (z. B. `feature/meilenstein-10a`).
- Gearbeitet wird auf dem Feature-Branch, Commits pro Teilschritt mit Meilenstein-Bezug im Text.
- Ist ein Meilenstein fertig und getestet, wird `main` nur per **Fast-Forward** auf den Feature-Branch vorgespult und `main` nach GitHub gepusht. Keine Merge-Commits auf `main`.
- Generierte Ordner (`Library/`, `Temp/`, `Logs/`, `UserSettings/`) stehen in `.gitignore` und gehören nicht ins Repository.
