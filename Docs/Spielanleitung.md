# RE:BORN – Spielanleitung

Willkommen in **RE:BORN – Echoes of Eternity**, Beta 0.9.0. Diese Anleitung erklärt den Einstieg, die Steuerung, Kampf, Magie, Handel, Gilden und die Orte der Welt. Die Bedienung gilt für Tastatur, Controller und Handy; Abweichungen stehen jeweils dabei.

---

## Inhalt

1. [Einstieg](#einstieg)
2. [Menüs](#menüs)
3. [Steuerung](#steuerung)
4. [Kampf](#kampf)
5. [Magie](#magie)
6. [Inventar und Ausrüstung](#inventar-und-ausrüstung)
7. [Stadt: Händler, Schmied und Läden](#stadt-händler-schmied-und-läden)
8. [Gilden und Quests](#gilden-und-quests)
9. [Orte](#orte)
10. [Bosse](#bosse)
11. [Einstellungen](#einstellungen)

---

## Einstieg

- Das **Hauptmenü** hat: **Weiter** (nur mit Spielstand), **Neues Spiel**, **Einstellungen**, **Neuigkeiten**, **Mitwirkende** und **Beenden** (nur am PC).
- **Neues Spiel** fragt vorher nach, wenn schon ein Spielstand existiert.
- Das Spiel startet immer in der **Stadt**. Dort beginnt man mit Startwerten.
- Gespeichert wird automatisch, etwa beim Aufsteigen, beim Schließen von Menüs und vor jedem Ortswechsel.

> **Hinweis:** Eine Einführung mit Mentor Aldric ist für die nächste Version geplant (siehe Roadmap in der README).

## Menüs

| Taste (Tastatur) | Wirkung |
|---|---|
| Tab, I oder Esc | Menü öffnen oder schließen |
| Pfeiltasten | Auswahl bewegen |
| Enter | Anlegen, Benutzen oder Ablegen |

Im Menü gibt es folgende Reiter:

- **Charakter:** Level, Attribute, Lebens-, Mana- und Angriffswerte. Hier liegt auch **Spielstand löschen**.
- **Inventar:** alle Gegenstände. Sortieren und Filtern sind möglich. Ein Klick (oder Enter) öffnet das Detail-Fenster mit **Anlegen**, **Benutzen**, **Ablegen** und **Wegwerfen**.
- **Magie:** deine Zauber mit Stufe und Erfahrung sowie Mana-Stufe.
- **Einstellungen** und **Zum Hauptmenü** (im Pause-Menü).

## Steuerung

| Aktion | Tastatur | Controller | Handy |
|---|---|---|---|
| Laufen | WASD / Pfeiltasten | linker Stick | Joystick links |
| Angriff | J oder linke Maustaste | West-Taste (X / □) | Knopf „Angriff“ |
| Ausweichen | Leertaste oder Linke Umschalt | Süd-Taste (A / ✕) | Knopf „Ausw.“ |
| Zauber 1–4 | 1, 2, 3, 4 | RB, RT, LB, LT | Knöpfe Z1–Z4 |
| Meditieren (halten) | M | Ost-Taste (B / ○) | Knopf „Medit.“ |
| Heiltrank | Q | Nord-Taste (Y / △) | Knopf „Trank“ |
| Manatrank | R | Steuerkreuz oben | HUD-Knopf |
| Sprechen | E oder Enter | Steuerkreuz unten | Knopf „Sprechen“ (nur nahe am NPC) |
| Menü | Tab, I, Esc | Start | Knopf „Menü“ oben rechts |

Alle Tasten lassen sich unter **Einstellungen → Tasten** neu belegen.

## Kampf

- Mit **Angriff** schlägst du den nächsten Gegner im Trefferkreis vor dir.
- Mit **Ausweichen** rollst du kurz und schnell zur Seite. Die Abklingzeit sinkt mit dem Attribut **Geschick**.
- Nach einem Treffer hast du eine kurze **Schutzzeit**, damit du nicht sofort erneut getroffen wirst.
- Erlittener Schaden sinkt mit deiner **Verteidigung**. Ausrüstung erhöht sie.
- Wenn du stirbst, erscheint der Text „Du wurdest besiegt“. Danach kehrst du in die Stadt zurück, mit vollem Leben und Mana. Du verlierst **10 % deines Goldes**; Gegenstände und Erfahrung bleiben.

**Gegner**

| Gegner | Wo | Eigenschaften |
|---|---|---|
| Schleim | Kampfraum, Wald | Langsam, Berührung kostet Leben; lässt Schleimgel und Gold fallen |
| Schattenwolf | Wald | Schnell, 40 Leben, 8 Schaden; Wolfsfell |
| Riesenspinne | Wald | Langsam, 60 Leben, 12 Schaden; Spinnenseide |
| Eiswolf | Frostgipfel | Schnell, 60 Leben, 12 Schaden |
| Frostgeist | Frostgipfel | Schwebt und schießt Eissplitter aus der Ferne (10 Schaden) |

## Magie

Du hast vier Zauberplätze (Tasten 1–4). Jeder Zauber hat eine Stufe von 1 bis 10, die durch Treffer steigt. Höhere Stufen machen mehr Schaden und größere Effekte; ab Stufe 10 erscheint ein goldener Rand.

| Zauber | Mana | Wirkung | Abklingzeit | Freischalten |
|---|---|---|---|---|
| Wasserkugel | 5 | Schaden 8 | 0,5 s | Von Anfang an |
| Wasserdrache | 25 | Schaden 20, durchdringt Gegner | 4 s | Wasserkugel Stufe 3 |
| Feuerball | 8 | Schaden 12 + Explosion (6) | 0,8 s | Spieler-Level 2 |
| Flammenring | 20 | Schaden 15 + Rückstoß | 3 s | Feuerball Stufe 3 |

- **Mana** füllt sich von selbst. Es steigt mit dem Attribut **Intelligenz**.
- **Meditieren** (M halten) füllt Mana schneller, du stehst dabei still.
- **Mana-Training:** Zaubern und Meditieren bringen Erfahrung für die Mana-Stufe. Sie erhöht das maximale Mana.
- Ein **Manatrank** stellt sofort Mana wieder her.

## Inventar und Ausrüstung

- **Benutzen:** Tränke heilen oder geben Mana. Die Schnelltasten Q (Heiltrank) und R (Manatrank) brauchen kein Menü.
- **Anlegen:** Waffen, Rüstung und Schmuck verbessern deine Werte.
- **Wegwerfen:** Du wirfst einen Gegenstand weg, einzeln oder alle auf einmal. Ausgerüstete Gegenstände musst du vorher ablegen. Weggeworfene Gegenstände lassen sich 2 Sekunden lang nicht wieder aufheben.
- **Löschen:** Beim Löschen eines Spielstands gibt es eine Rückfrage.

Gegenstände gibt es in den Seltenheiten **Gewöhnlich**, **Selten** und **Episch**. Beispiele:

| Gegenstand | Platz | Wirkung |
|---|---|---|
| Holzschwert / Eisenschwert | Waffe | Angriff +3 / +6 |
| Eichenstab | Waffe | Angriff +1, Intelligenz +3 |
| Holzschild | Nebenhand | Verteidigung +2 |
| Lederweste | Brust | Verteidigung +2, Leben +5 |
| Eisenhelm | Helm | Verteidigung +3 |
| Saphirring | Ring | Intelligenz +2, Glück +1 |
| Herz des Waldes | Halskette | Leben +25, Vitalität +3 (Bossbeute) |

## Stadt: Händler, Schmied und Läden

Die Stadt hat einen Marktplatz mit Händlern und dem Schmied, Läden in der Ladenstraße und Wohnviertel mit Bewohnern. Du sprichst mit **E** oder **Enter** (Steuerkreuz unten bei Controller).

**Händler (Marktplatz)**
- Reiter **Kaufen** und **Verkaufen**.
- Kaufpreis = 4 × Verkaufswert. Der Händler kauft alles, was du nicht trägst.
- Waren: Heiltränke, Manatränke, Eisen-Set.

**Schmied (Amboss)**
- Verbessert Waffen, Rüstung und Schild auf Stufe +1 bis +5. Schmuck wird nicht verbessert.
- Waffen: +1 Angriff pro Stufe. Rüstung und Schild: +1 Verteidigung pro Stufe.
- Kosten: Verkaufswert × Stufe × 3 Gold; ab Stufe 3 zusätzlich Stufe × 2 Schleimgel. Das klappt immer.

**Läden (Ladenstraße)**
- **Waffenhändler:** Holzschwert, Eisenschwert, Eichenstab, Holzschild
- **Rüstungshändler:** Leder- und Eisen-Set
- **Alchemistin:** Tränke
- **Gemischtwarenladen**

Läden und Wohnhäuser kann man nicht betreten; man bedient sie von außen.

**Gold** bekommst du von Schleimen (2–4 Gold), Wölfen und Spinnen, Quests und Bossen. Es wird auf dem Boden eingesammelt, wenn du es berührst.

## Gilden und Quests

Es gibt drei Gilden. Jede hat eine Halle in der Stadt (Nordviertel), die du betreten kannst. Der Gildenmeister darin öffnet das Gilden-Fenster.

| Gilde | Bonus bei Beitritt | Gildenwaffe |
|---|---|---|
| Krieger | Stärke +2 (Rang 2: +4 auf das Gilden-Attribut) | Gildenschwert (Angriff +5, Stärke +1) |
| Magier | Intelligenz +2 | Gildenstab (Angriff +2, Intelligenz +4) |
| Assassinen | Geschick +2 | Schattendolche (Angriff +4, Geschick +2) |

- Du kannst **einer** Gilde angehören. Ein Wechsel kostet **200 Gold**; Rang und Quests der alten Gilde gehen verloren, die Waffe bleibt.
- Beitritt bringt **Rang 1 „Anwärter“**. Nach **drei Quests** wirst du **Geselle** (Rang 2) und erhältst mehr Bonus.
- Du hast immer nur eine aktive Quest. **Annehmen** startet sie, **Abgeben** belohnt dich, sobald sie erfüllt ist.
- Belohnungen: 50, 80 und 120 Gold sowie 60, 100 und 150 Erfahrung.

**Quests je Gilde**

- **Krieger:** 5 Schleime besiegen → 3 Wölfe besiegen → 4 Wolfsfelle bringen
- **Magier:** 5 Gegner mit Zaubern besiegen → 3 Spinnenseiden bringen → 3 Spinnen besiegen
- **Assassine:** 3 Wölfe besiegen → 3 Spinnen besiegen → 6 Schleimgel und 2 Spinnenseiden bringen

Der Quest-Fortschritt steht oben rechts im HUD.

## Orte

| Ort | Was dort ist |
|---|---|
| **Stadt** | Marktplatz, Händler, Schmied, Läden, Gildenhallen (Nordviertel), Wohnviertel, Teich, Stadttor (geschlossen). Soldaten stehen auf der Mauer, tun aber nichts. |
| **Höhle** | Nordwesten der Stadt. Im Eingang liegt das Portal zum Kampfraum, das Portal zum Wald und das Siegel-Portal zum Frostgipfel. |
| **Kampfraum** | Übungsort für Schleime. Oben in der Mitte steht der Waldwächter. |
| **Wald** | Schattenwölfe, Riesenspinnen und Schleime. Grüner Ausgang zurück in die Höhle. |
| **Frostgipfel** | Schnee, Eis und gefrorene Bäume. Eiswölfe, Frostgeister, hinten die Arena des Eisdrachen. |
| **Gildenhallen** | Krieger-, Magier- und Assassinenhalle mit Gildenmeister und Mitgliedern. Der Ausgang ist die Fußmatte vor der Tür. |

**Portale** führen zwischen den Orten. Du läufst in das Portal, das Spiel speichert und blendet kurz ab.

**Siegel-Portal (Frostgipfel):** Es ist grau und versiegelt. Mit dem **Frostschlüssel** (Beute des Waldwächters) öffnest du es mit **E**. Der Schlüssel wird verbraucht, und das Portal bleibt offen. Ohne Schlüssel zeigt es „Versiegelt – ein Schlüssel fehlt“.

## Bosse

### Waldwächter (Golem) – Kampfraum

Der Waldwächter wacht auf, wenn du ihm nahe kommst. Er hat zwei Stufen.

**Stufe 1 (1200 Leben)**
- **Schlag** aus der Nähe: Er holt kurz aus. Weiche aus oder geh zur Seite.
- **Wurzeln:** Eine rote Warnfläche erscheint am Boden und nach 0,8 Sekunden schießen Wurzeln hoch. Verlasse die Fläche oder weiche aus.
- Bei 50 % Leben brüllt er und ruft drei Wölfe.

**Stufe 2 – „Uralter Waldwächter“ (1500 Leben)**
- Nach 0 Leben verwandelt er sich (kurz unverwundbar). Die Wölfe verschwinden.
- **Dornenwelle:** Ein Ring wächst von ihm aus. Gehe weg oder weiche im richtigen Moment aus.
- **Ansturm:** Eine Ziellinie blinkt eine Sekunde lang, dann rennt er geradeaus. Gehe seitlich aus der Linie.
- **Zorn des Waldes:** Ein großer roter Kreis lädt sich vier Sekunden lang auf. Wer darin steht, verliert die Hälfte seines maximalen Lebens. Lauf heraus oder weiche im Moment der Entladung aus.
- Alle 15 Sekunden ruft er zwei verwundete Wölfe.

**Belohnung:** 400 Gold, 600 Erfahrung und beim ersten Sieg das Amulett **Herz des Waldes** sowie den **Frostschlüssel**. Nach 10 Minuten kehrt er zurück (auch nach einem Neustart des Spiels). Danach gibt es 300 Gold und Tränke.

**Tipps**
- Halte Abstand, bis du die Angriffe erkennst. Die Warnungen sind rot.
- Der Kampfraum ist eng. Plane deinen Rückweg, bevor der Ansturm kommt.

### Eisdrache – Frostgipfel

Der Eisdrache hat 3000 Leben und wacht auf, wenn du ihm nahe kommst. Sein Kampf hat drei Teile.

- **Eisatem:** Ein Kegel vor dem Drachen, 1 Sekunde Vorwarnung. Seitlich ausweichen.
- **Eisregen:** Fünf Warnkreise am Boden, aus denen Eiszapfen fallen. Die Kreise vorher verlassen.
- **Flügelschlag:** Nah am Drachen wirft er dich zurück. Abstand halten.
- **Flugphase (ab 50 % Leben):** Der Drache fliegt fünf Sekunden lang und ist unverwundbar. In dieser Zeit ruft er zwei Frostgeister.
- **Blizzard:** Nach etwa 25 Sekunden und danach alle 35 Sekunden lädt der Drache fünf Sekunden lang auf. Dann trifft der Schneesturm die ganze Arena und fügt 60 % deines Lebens als Schaden zu. **Schutz:** Stell dich hinter eine der drei **Eissäulen**. Die Deckung gilt nur für die Linie zum Drachen.

**Belohnung:** Beim ersten Sieg 3 Drachenschuppen, 2 Eiskristalle, 600 Gold und 1000 Erfahrung. Später gibt es Gold und Eiskristalle. Nach 10 Minuten kehrt er zurück.

**Tipps**
- Der Blizzard ist der gefährlichste Angriff. Sobald der Drache lädt, such eine Eissäule.
- Frostgeister schießen aus der Ferne. Ihre Eissplitter fliegen geradeaus; wechsle die Richtung, statt stehen zu bleiben.

## Einstellungen

Erreichbar im Hauptmenü und im Pause-Menü.

| Reiter | Einstellungen |
|---|---|
| **Sprache** | Deutsch oder English. Beim ersten Start wird die Systemsprache genutzt. |
| **Ton** | Gesamtlautstärke, Lautstärke der Effekte |
| **Grafik** | Vollbild, Qualität (niedrig, mittel, hoch), Bildrate (30, 60 oder unbegrenzt) |
| **Oberfläche** | Größe (90 %, 100 %, 115 %), Schadenszahlen an oder aus, Handy-Knöpfe (automatisch, an, aus) |
| **Tasten** | Jede Aktion neu belegen. Tastatur und Controller werden getrennt eingestellt. Bei Doppelbelegung erscheint ein Hinweis. „Zurücksetzen“ stellt die Standardbelegung wieder her. |

Deine Einstellungen bleiben beim Neustart erhalten und sind getrennt vom Spielstand gespeichert.
