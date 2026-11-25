# Menue-Test (Meilenstein 10a): Hauptmenue (Weiter, Neues Spiel mit Nachfrage, Neuigkeiten, Mitwirkende), Einstellungen
# (Sprache, Ton, Bildrate, Oberflaechen-Groesse, Zahlen, Handy, Grafik), Tasten neu belegen, Pause-Menue (Einstellungen,
# Hauptmenue mit Nachfrage), Gegenstaende wegwerfen und wieder aufheben, Ortsname und "Gespeichert".
# Deine eigenen Einstellungen bleiben unberuehrt: die Tests laufen mit Standardwerten nur fuer die Sitzung (M11 prueft das).
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Num([string]$text, [string]$key) { if ($text -match "$key=(-?\d+)") { return [int]$Matches[1] }; return -1 }
function MmDo([string]$act) { return (Eval 'mm_do.cs' @{ ACT = $act }) }
function MmSet([string]$act) { return (Eval 'mm_settings.cs' @{ ACT = $act }) }
function MmReb([string]$act) { return (Eval 'mm_rebind.cs' @{ ACT = $act }) }
function MmPause([string]$act) { return (Eval 'mm_pause.cs' @{ ACT = $act }) }
function MmDrop([string]$act) { return (Eval 'mm_drop.cs' @{ ACT = $act }) }

'--- Menue-Test ---'
'M0) Hauptmenue-Szene starten'
StartPlay -Scene MainMenu
$p0 = Eval 'mm_prefs.cs'
'  Einstellungen vorher: ' + $p0

'M1) Hauptmenue ohne Spielstand'
Check 'M1 nur Neues Spiel, Version, keine Nachfrage' (Eval 'mm_state.cs') '^Szene=MainMenu Weiter=False Neu=True Beenden=True Version=Beta 0\.9\.0 .*Victory Nachfrage=False '
Shot 'm10a_hauptmenue'

'M2) Neuigkeiten und Mitwirkende'
[void](MmDo 'news')
Check 'M2 Neuigkeiten offen' (Eval 'mm_state.cs') 'Text=True Titel=Neuigkeiten'
[void](MmDo 'textback')
Check 'M2 zurueck im Hauptmenue' (Eval 'mm_state.cs') 'Text=False '
[void](MmDo 'credits')
Check 'M2 Mitwirkende offen' (Eval 'mm_state.cs') 'Text=True Titel=Mitwirkende'
[void](MmDo 'textback')
Check 'M2 wieder zu' (Eval 'mm_state.cs') 'Text=False '

'M3) Einstellungen und Sprache'
[void](MmDo 'settings')
Check 'M3 Einstellungen offen' (Eval 'mm_state.cs') 'Einst=True '
$r = MmSet 'state'
Check 'M3 Standardwerte, nur Sitzung' $r '^Sprache=de .* Sitzung=True'
Shot 'm10a_einstellungen'
$r = MmSet 'click:languageButton'
Check 'M3 Englisch' $r '^Sprache=en .*Inventar=Inventory '
CheckTrue 'M3 Uebersetzungstabelle hat ueber 200 Eintraege' ((Num $r 'EN') -gt 200) $r
Shot 'm10a_englisch'
[void](MmDo 'back')   # Hauptmenue-Knoepfe aktualisieren sich beim Wieder-Einblenden
Check 'M3 Menue ist englisch' (Eval 'mm_state.cs') 'NeuLabel=New Game$'
[void](MmDo 'settings')
$r = MmSet 'click:languageButton'
Check 'M3 wieder Deutsch' $r '^Sprache=de .*Inventar=Inventar '
[void](MmDo 'back')
Check 'M3 Menue wieder deutsch' (Eval 'mm_state.cs') 'NeuLabel=Neues Spiel$'
[void](MmDo 'settings')

'M4) Optionen'
Check 'M4 Lautstaerke 100 %' (MmSet 'click:masterButton') ' Master=1 .*Listener=1\.00 '
Check 'M4 Bildrate unbegrenzt' (MmSet 'click:frameRateButton') ' FPS=-1 targetFPS=-1 .*Werte=Deutsch\|100 %\|Unbegrenzt '
Check 'M4 Oberflaeche 115 %, Skalierung passt' (MmSet 'click:uiScaleButton') ' UI=1\.15 .*Scaler=16(69|70) '
Check 'M4 Schadenszahlen aus' (MmSet 'click:damageNumbersButton') ' Zahlen=False '
Check 'M4 Handy-Knoepfe an' (MmSet 'click:mobileButton') ' Handy=1 '
Check 'M4 Grafik niedrig' (MmSet 'click:qualityButton') ' Qualitaet=0 '
[void](MmDo 'back')
Check 'M4 Einstellungen zu' (Eval 'mm_state.cs') 'Einst=False '
# zurueck auf die Sitzungs-Standardwerte (Grafik, Bildrate, Lautstaerke), nichts wird gespeichert
Check 'M4 Standardwerte wieder da' (MmSet 'reset') '^Sprache=de Master=0\.8 .* FPS=60 .* UI=1 Zahlen=True Handy=0 '

'M5) Tasten neu belegen'
[void](MmDo 'settings')
$r = MmReb 'state'
Check 'M5 15 Zeilen' $r '^Zeilen=15 '
$origAttack = '<Keyboard>/space'
if ($r -match 'Attack=(\S+)') { $origAttack = $Matches[1] }
$origDodge = '-'
if ($r -match 'Dodge=(\S+)') { $origDodge = $Matches[1] }
Check 'M5 Angriff auf K' (MmReb 'apply:Attack:-:kb:<Keyboard>/k') 'Attack=<Keyboard>/k .*Ergebnis=K$'
Check 'M5 gleiche Taste: Warnung' (MmReb 'apply:Dodge:-:kb:<Keyboard>/k') 'Meldung=Achtung.*Angriff'
Shot 'm10a_tasten'
$r = MmReb 'reset'
Check 'M5 Angriff wieder auf Standard' $r ('Attack=' + [regex]::Escape($origAttack) + ' ')
Check 'M5 Ausweichen wieder auf Standard' $r ('Dodge=' + [regex]::Escape($origDodge) + ' ')
Check 'M5 Meldung Standard-Belegung' $r 'Meldung=Standard-Belegung'
[void](MmDo 'back')

'M6) Neues Spiel ohne Spielstand'
[void](MmDo 'newgame')
Wait 2500; WaitReady
Check 'M6 in der Stadt' (Eval 'b_state.cs') '^Szene=Base '
Check 'M6 Einfuehrung angefordert' (MmDo 'flag') 'Tutorial=True'

'M7) Pause-Menue: Einstellungen und Hauptmenue'
'  ' + (Eval 't2_setup.cs')
Check 'M7 Menue offen' (MmPause 'open') 'Menue=True Wurzel=True '
Check 'M7 Einstellungen ueber dem Menue' (MmPause 'settings') 'Wurzel=False Einst=True '
Shot 'm10a_pause_einstellungen'
Check 'M7 zurueck ins Menue' (MmPause 'back') 'Wurzel=True Einst=False '
Check 'M7 Nachfrage Hauptmenue' (MmPause 'mainmenu') 'Nachfrage=True Meldung=Zum Hauptmen.*'
Shot 'm10a_nachfrage'
Check 'M7 Nein: weiter im Spiel' (MmPause 'no') 'Nachfrage=False .*Szene=Base'
[void](MmPause 'mainmenu')
[void](MmPause 'yes')
Wait 2500; WaitReady
Check 'M7 im Hauptmenue, Spiel wurde gespeichert' (Eval 'mm_state.cs') '^Szene=MainMenu Weiter=True '

'M8) Weiter und Neues Spiel mit Nachfrage'
Check 'M8 Weiter da' (Eval 'mm_state.cs') ' Weiter=True .*Spielstand=True '
Check 'M8 Nachfrage beim Neuen Spiel' ((MmDo 'newgame') + ' ' + (Eval 'mm_state.cs')) 'Nachfrage=True '
Shot 'm10a_neues_spiel'
[void](MmDo 'no')
Check 'M8 Abbrechen: Spielstand bleibt' (Eval 'mm_state.cs') 'Nachfrage=False .*Spielstand=True '
[void](MmDo 'newgame')
[void](MmDo 'yes')
Wait 2500; WaitReady
Check 'M8 in der Stadt' (Eval 'b_state.cs') '^Szene=Base '
Check 'M8 Einfuehrung angefordert' (MmDo 'flag') 'Tutorial=True'
StopPlay

'M9) Gegenstaende wegwerfen'
StartPlay -Scene Room_Test
'  ' + (Eval 't2_setup.cs')
Check 'M9 drei Schleimgel im Beutel' (MmDrop 'give:schleimgel:3') '^Anzahl=3 '
[void](MmDrop 'open')
Check 'M9 Menue offen, Anzahl 3' (MmDrop 'state:schleimgel') '^Anzahl=3 Boden=0 '
Check 'M9 eins weggeworfen' (MmDrop 'drop1:schleimgel') '^Anzahl=2 Boden=1 .*Ergebnis=True$'
[void](MmPause 'close')
[void](MmDrop 'onto:schleimgel')   # direkt darauf stellen: erst nach 2 s wieder aufheben
Wait 800
Check 'M9 nach 1 s noch am Boden (Wartezeit 2 s)' (MmDrop 'state:schleimgel') '^Anzahl=2 Boden=1 '
Wait 2200
[void](MmDrop 'onto:schleimgel')   # kleiner Schritt darauf (ruhende Koerper melden keine Beruehrung)
Wait 400
Check 'M9 nach 3 s wieder aufgehoben' (MmDrop 'state:schleimgel') '^Anzahl=3 Boden=0 '
[void](MmDrop 'open')
Check 'M9 ganzen Stapel weggeworfen' (MmDrop 'dropall:schleimgel') '^Anzahl=0 Boden=3 .*Ergebnis=True$'
Check 'M9 Ausruestung laesst sich nicht wegwerfen' (MmDrop 'equipdrop') 'Ergebnis=False$'
Shot 'm10a_wegwerfen'
[void](MmPause 'close')
StopPlay

'M10) Ortsname und Gespeichert'
StartPlay -Scene Frost
'  ' + (Eval 't2_setup.cs')
Wait 600
Check 'M10 Ortsname Frostgipfel' (Eval 'mm_banner.cs' @{ ACT = 'state' }) 'Ort=Frostgipfel '
Shot 'm10a_ortsname'
[void](Eval 'mm_banner.cs' @{ ACT = 'save' })
Wait 300
Check 'M10 Gespeichert wird gezeigt' (Eval 'mm_banner.cs' @{ ACT = 'state' }) 'Gespeichert=True '

'M11) Deine Einstellungen sind geschuetzt'
$p1 = Eval 'mm_prefs.cs'
CheckTrue 'M11 deine gespeicherten Einstellungen unveraendert' ($p0 -eq $p1) "vorher: $p0 | nachher: $p1"

StopPlay
TestResult 'Menue-Test' $f0
