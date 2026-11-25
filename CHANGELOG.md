# Changelog

Alle wichtigen Änderungen an RE:BORN – Echoes of Eternity.

Das Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.1.0/). Versionen folgen grob [Semantic Versioning](https://semver.org/lang/de/); solange das Spiel in der Beta ist, steht die Nummer für den Meilenstein der Entwicklung.

## [Unveröffentlicht]

### Geplant
- Einführung mit Mentor Aldric (Meilenstein 10b)
- Soundeffekte (Meilenstein 10b)
- Browser-Version auf itch.io (Meilenstein 10c)
- Gilden-Fähigkeiten: Wirbelschlag, Kettenblitz, Schattensprung (Meilenstein 9b)
- Handwerk beim Schmied (Meilenstein 9b)
- Wolfswelpe als Begleiter (Meilenstein 9b)
- Musik

## [0.9.0] – Beta

Die erste öffentliche Beta.

### Hinzugefügt
- Hauptmenü mit Weiter, Neues Spiel (mit Nachfrage bei vorhandenem Spielstand), Einstellungen, Neuigkeiten, Mitwirkende und Beenden (nur PC)
- Einstellungen: Sprache, Ton (gesamt und Effekte), Grafik (Vollbild, Qualität, Bildrate), Oberfläche (Größe 90/100/115 %, Schadenszahlen, Handy-Knöpfe)
- Tastenbelegung für Tastatur und Controller, getrennt änderbar, mit Zurücksetzen und Hinweis bei Doppelbelegung
- Englische Oberfläche und englische Gegenstandsnamen und -beschreibungen
- Pause-Menü mit Einstellungen und „Zum Hauptmenü“
- Wegwerfen: einzelne Gegenstände oder alle auf einmal (mit kurzer Aufhebsperre)
- Ortsname beim Szenenwechsel
- Hinweis „Gespeichert“

### Geändert
- Einstellungen sind getrennt vom Spielstand gespeichert (auch im Browser nutzbar)
- Die Play-Tests sichern den Spielstand mit Prüfsumme und lassen die Einstellungen und Tasten des Spielers unangetastet

## [0.8.0] – Frostgipfel und Siegel-Portal

### Hinzugefügt
- Schlüssel-Portal in der Höhle: Der Golem lässt beim ersten Sieg den Frostschlüssel fallen, mit dem sich das versiegelte Portal öffnet
- Frostgipfel mit Eiswölfen und Frostgeistern, die aus der Ferne Eissplitter schießen
- Eisdrache mit Eisatem, Eisregen, Flügelschlag, Flugphase und Blizzard (Schutz hinter Eissäulen)
- Beute des Eisdrachen: Drachenschuppe, Eiskristall, Gold und Erfahrung
- Wiederkehr des Eisdrachen nach 10 Minuten
- Spielstand-Version 7 mit geöffneten Portalen (`worldFlags`)

## [0.7.1] – Golem-Boss mit zwei Stufen

### Hinzugefügt
- Zweite Stufe „Uralter Waldwächter“ (1500 Leben) nach dem Sieg über die erste Stufe
- Dornenwelle, Ansturm und Ultimate „Zorn des Waldes“ in Stufe 2
- Wolfsruf in Stufe 2
- Wiederkehr des Golems nach 10 Minuten (echte Zeit, auch nach einem Neustart)
- Spielstand-Version 6 mit Siegzeitpunkt und Anzahl der Siege je Boss

### Behoben
- Der Name des Bosses war im Bild unsichtbar, weil das Textfeld zu niedrig war

## [0.7.0] – Golem-Boss und schönere Gildenhallen

### Hinzugefügt
- Golem (Waldwächter) im Kampfraum mit Wurzelangriff und Lebensleiste
- Amulett „Herz des Waldes“ als Bossbeute
- Spielstand-Version 5 mit besiegten Bossen
- Große Möbel in allen drei Gildenhallen
- Je zwei Gildenmitglieder, die in den Hallen umherlaufen

## [0.6.0] – Gilden, Quests und Wald

### Hinzugefügt
- Drei Gilden: Krieger, Magier und Assassinen. Beitritt, Wechsel für 200 Gold, Ränge (Anwärter, Geselle)
- Gilden-Quests (je drei pro Gilde) mit Fortschritt im HUD
- Gildenwaffen mit Gildenbonus
- Wald mit Schattenwölfen und Riesenspinnen, erreichbar über ein Portal in der Höhle
- Kampf-Logik für Wolf und Spinne
- Neue Beute: Wolfsfell und Spinnenseide
- Spielstand-Version 4 mit Gilde, Rang und Quest

## [0.5.1] – Begehbare Räume

### Hinzugefügt
- Begehbare Gildenhallen (Krieger, Magier, Assassinen) und Höhle
- Gildenmeister mit Sprechblase
- Türen in die Hallen und in die Höhle
- Dunklere Innenräume mit Fackeln, Kerzen und Kristallen als Lichtquellen

## [0.5.0] – Stadt, Läden und Gildenhallen

### Hinzugefügt
- Stadt mit Mauer, Ecktürmen, geschlossenem Stadttor, Soldaten und Wachen
- Marktplatz mit Brunnen, Schmiede mit Esse, Rathaus und Gasthaus
- Ladenstraße: Waffenhändler, Rüstungshändler, Alchemistin und Gemischtwarenladen
- Wohnviertel mit 15 Häusern, Gärten und Teich
- Gildenviertel im Norden (von außen)
- Bewohner, die zwischen festen Punkten umherlaufen

### Behoben
- Dachflächen der Gebäude waren nach innen geneigt

## [0.4.0] – Basis, Händler, Schmied und Gold

### Hinzugefügt
- Startszene Basis mit Händler (Kaufen und Verkaufen) und Schmied (Verbessern, Stufen +1 bis +5)
- Gold als Währung, Gold-Münzen von Schleimen
- Portal zwischen Basis und Kampfraum
- Tod mit Rückkehr in die Basis (10 % des Goldes gehen verloren)
- Spielstand-Version 3 mit Gold, Schmiede-Stufen und letzter Szene

### Behoben
- Die Basis wurde ohne Boden gespeichert (Szenenwechsel gab Kacheln frei)
- „Spielstand löschen“ startete im Kampfraum statt in der Basis

## [0.3.0] – Comic-Grafik

### Hinzugefügt
- Neue Blender-Grafik im Comic-Stil: Cel-Shading mit dunkler Kontur und modellierten Formen
- Figuren, Gegenstände, Gegner und Zauber in 128 × 128 Pixeln
- Animationen „hurt“ (Treffer) und „death“ (Tod), das letzte Bild bleibt stehen
- Blob-Schatten unter allen Figuren
- Grafik für Schattenwolf, Riesenspinne und Waldwächter
- Bodenkacheln, Bäume, Felsen und Büsche aus Blender

## [0.2.0] – Magie und neues Aussehen

### Hinzugefügt
- Magie mit Wasser und Feuer: Wasserkugel, Wasserdrache, Feuerball, Flammenring; je Stufe 1–10
- Mana, Meditieren und Mana-Training; Manatrank
- Dunkler Fantasy-Stil mit Schriften Cinzel und Alegreya Sans
- Neues HUD mit Porträt, Leisten für Leben, Mana und Erfahrung, Aktionsleiste
- Sichtbare Ausrüstung am Spieler (14 Teile)
- Schadenszahlen und Zauber-Ausrufe
- Spielstand-Version 2 mit Zauber-Stufen und Mana

## [0.1.0] – Prototyp und Charakter

### Hinzugefügt
- Top-Down-Figur mit Angriff und Ausweichen, Waldschleime als Gegner
- Lebenspunkte mit Schutzzeit, Erfahrung und Level
- Schatztruhe, Inventar, Ausrüstung (Waffen, Rüstung, Ringe, Halsketten) und Attribute
- Tränke und Beute
- Speichern des Spielstands (Version 1)
- Automatische Play-Tests über den Unity-Editor

### Behoben
- Mauern hielten den Spieler nicht auf (leerer Collider nach dem Bauen der Szene)
</content>
