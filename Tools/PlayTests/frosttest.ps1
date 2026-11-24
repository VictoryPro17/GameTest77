# Frost-Test (Meilenstein 9a): Siegel-Portal in der Hoehle (ohne Schluessel zu, mit Schluessel offen, gespeichert),
# Frostgipfel (Eiswoelfe, Frostgeister mit Fernkampf, Eissaeulen), Eisdrache: schlaeft, wacht auf, Atem, Eisregen,
# Fluegelschlag, Flug bei 50 % mit Geistern, Blizzard (Schaden, Schutz hinter der Saeule); Sieg mit Beute, gespeichert,
# nach 10 Minuten wieder da; Tod in der Frostwelt -> Stadt.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Num([string]$text, [string]$key) { if ($text -match "$key=(-?\d+)") { return [int]$Matches[1] }; return -1 }
function FrDo([string]$act) { [void](Eval 'fr_do.cs' @{ ACT = $act }) }

'--- Frost-Test ---'
'F1) Siegel-Portal in der Hoehle'
StartPlay -Scene Cave
'  ' + (Eval 't2_setup.cs')
Check 'F1 anfangs versiegelt' (Eval 'fr_portal.cs' @{ ACT = 'state' }) 'offen=False .*versiegelt=True blau=False Flag=False'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Frost' }); Wait 1500
Check 'F1 versiegeltes Portal fuehrt nirgendwohin' (Eval 'b_state.cs') '^Szene=Cave '

'F2) Ohne Schluessel bleibt das Siegel zu'
Check 'F2 Meldung Versiegelt' (Eval 'fr_portal.cs' @{ ACT = 'unseal' }) 'offen=False .*Meldung=Versiegelt.*'

'F3) Mit Schluessel oeffnet sich das Siegel'
[void](Eval 'fr_portal.cs' @{ ACT = 'givekey' })
Check 'F3 offen, Schluessel verbraucht, blaues Portal' (Eval 'fr_portal.cs' @{ ACT = 'unseal' }) 'offen=True Schluessel=0 .*versiegelt=False blau=True Flag=True'
Check 'F3 gespeichert (Version 7, Schalter)' (Eval 'fr_savefile.cs') '^Version=7 Flags=frost_siegel'
Shot 'm9a_siegel_offen'
StopPlay

'F4) Nach Neustart noch offen, Portal fuehrt zum Frostgipfel'
StartPlay -KeepSave -Scene Cave
'  ' + (Eval 't2_setup.cs')
Check 'F4 Siegel bleibt offen' (Eval 'fr_portal.cs' @{ ACT = 'state' }) 'offen=True '
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Frost' }); Wait 2500; WaitReady
Check 'F4 Ankunft im Frostgipfel' (Eval 'b_state.cs') '^Szene=Frost .* Ankunft=Ankunft$'

'F5) Frostgipfel: Bewohner'
$r = Eval 'fr_census.cs'
Check 'F5 Gegner, Saeulen, Drache' $r 'Szene=Frost Eiswoelfe=4 Frostgeister=3 Fernkampf=3 Saeulen=3 Drache=True friedlich=False'
CheckTrue 'F5 Boden vorhanden' ((Num $r 'Boden') -gt 0) $r
Shot 'm9a_frostgipfel'

'F6) Frostgeist schiesst Eissplitter'
[void](Eval 'fr_spirit.cs'); Wait 1500
$r = Eval 'fr_spirit_check.cs'
CheckTrue 'F6 Schuesse, Splitter, Treffer, Spieler verwundet' ((Num $r 'Schuesse') -ge 1 -and (Num $r 'Splitter') -ge 1 -and (Num $r 'SplitterTreffer') -ge 1 -and (Num $r 'Spieler') -lt 1000) $r
Shot 'm9a_frostgeist'

'F7) Eisdrache schlaeft und erwacht'
'  ' + (Eval 't2_setup.cs')
FrDo 'hp:5000'
FrDo 'timers:999:999:999:999'
Check 'F7 schlaeft auf Abstand, keine Leiste' (Eval 'fr_state.cs') '^wach=False Zustand=Sleep HP=3000/3000 .* Leiste=False '
FrDo 'place:-3:0'; Wait 600
$r = Eval 'fr_state.cs'
Check 'F7 wacht auf, Leiste mit Namen' $r '^wach=True .* Leiste=True .* Name=Eisdrache'
FrDo 'timers:999:999:999:999'   # Wake setzt den Blizzard-Zaehler
Shot 'm9a_eisdrache'

'F8) Frostatem'
foreach ($a in 'inv:false', 'hp:1000', 'dragonto:9:0.5', 'place:-2.5:0', 'timers:999:0:999:999') { FrDo $a }
Wait 2000
$r = Eval 'fr_state.cs'
CheckTrue 'F8 Atem ausgefuehrt und getroffen' ((Num $r 'Atem') -ge 1 -and (Num $r 'AtemTreffer') -ge 1 -and (Num $r 'Spieler') -le 970) $r
$before = $r
foreach ($a in 'inv:true', 'place:-2.5:0', 'timers:999:0:999:999') { FrDo $a }
Wait 2000
$r = Eval 'fr_state.cs'
CheckTrue 'F8 beim Ausweichen kein Atem-Schaden' ((Num $r 'Atem') -gt (Num $before 'Atem') -and (Num $r 'AtemTreffer') -eq (Num $before 'AtemTreffer')) "vorher: $before | nachher: $r"

'F9) Eisregen'
$before = Eval 'fr_state.cs'
foreach ($a in 'inv:false', 'hp:1000', 'place:-3.5:0', 'timers:999:999:0:999') { FrDo $a }
Wait 2200
$r = Eval 'fr_state.cs'
CheckTrue 'F9 Eiszapfen fallen und treffen' ((Num $r 'Regen') -ge 1 -and (Num $r 'Zapfen') - (Num $before 'Zapfen') -ge 5 -and (Num $r 'ZapfenTreffer') -ge 1) "vorher: $before | nachher: $r"

'F10) Fluegelschlag'
foreach ($a in 'inv:false', 'hp:1000', 'place:-1.6:0', 'timers:999:999:999:0') { FrDo $a }
Wait 1500
$r = Eval 'fr_state.cs'
CheckTrue 'F10 Fluegelschlag trifft' ((Num $r 'Fluegel') -ge 1 -and (Num $r 'FluegelTreffer') -ge 1) $r

'F11) Flug bei 50 %'
foreach ($a in 'inv:true', 'timers:999:999:999:999', 'hit:1500') { FrDo $a }
Wait 300
Check 'F11 in der Luft, unverwundbar, 2 Geister' (Eval 'fr_state.cs') ' unverwundbar=True Flug=True .* Geister=2 '
Check 'F11 Leben halbiert' (Eval 'fr_state.cs') ' HP=1500/3000 '
FrDo 'hit:100'
Check 'F11 im Flug kein Schaden' (Eval 'fr_state.cs') ' HP=1500/3000 '
Wait 5500
Check 'F11 Flug vorbei' (Eval 'fr_state.cs') ' unverwundbar=False Flug=False Geflogen=True '
Shot 'm9a_flug'

'F12) Blizzard'
'  ' + (Eval 't2_setup.cs')   # Geister aus, Drache bleibt
foreach ($a in 'inv:false', 'hp:1000', 'dragonto:9.5:0.5', 'place:-3:0', 'timers:0:999:999:999') { FrDo $a }
Wait 700
Check 'F12 laedt, Warnung sichtbar' (Eval 'fr_state.cs') ' laedt=True Warnung=True '
Shot 'm9a_blizzard'
Wait 5200
$r = Eval 'fr_state.cs'
CheckTrue 'F12 ungeschuetzt: 60 % Schaden' ((Num $r 'Blizzard') -eq 1 -and (Num $r 'Spieler') -ge 380 -and (Num $r 'Spieler') -le 420) $r
foreach ($a in 'hp:1000', 'dragonto:9.5:0.5', 'pillar:0', 'timers:0:999:999:999') { FrDo $a }
Wait 5800
$r = Eval 'fr_state.cs'
CheckTrue 'F12 hinter der Saeule: geschuetzt' ((Num $r 'Blizzard') -eq 2 -and (Num $r 'Geschuetzt') -ge 1 -and (Num $r 'Spieler') -eq 1000) $r

'F13) Sieg ueber den Drachen'
Check 'F13 Beute, Gold, EP, gespeichert' (Eval 'fr_kill.cs') '^tot=True Beute=drachenschuppex3,eiskristallx2 Gold=600 EP\+=True gespeichert=.*eisdrache.* Version=7 '
Wait 3500
Check 'F13 Drache weg, Leiste aus' (Eval 'fr_state.cs') '^kein Drache Leiste=False'
StopPlay

'F14) Nach Neustart noch besiegt, nach 10 Minuten wieder da'
StartPlay -KeepSave -Scene Frost
Check 'F14 kein Drache direkt nach dem Sieg' (Eval 'fr_state.cs') '^kein Drache '
FrDo 'clock:11'; Wait 2500; WaitReady
Check 'F14 nach 11 Minuten wieder da (kein Erst-Sieg)' (Eval 'fr_state.cs') '^wach=False Zustand=Sleep HP=3000/3000 .* Erst=False'
'  ' + (Eval 't2_setup.cs')
FrDo 'hp:5000'
FrDo 'wake'; Wait 600
Check 'F14 zweiter Sieg: kleinere Beute' (Eval 'fr_kill.cs') '^tot=True Beute=(eiskristallx[12],heiltrankx1) Gold=(2[5-9]\d|300) '
StopPlay

'F15) Tod in der Frostwelt -> Stadt'
StartPlay -Scene Frost
'  ' + (Eval 't2_setup.cs')
foreach ($a in 'inv:false', 'hp:30', 'wake', 'timers:999:0:999:999', 'place:-2.5:0') { FrDo $a }
Wait 8000; WaitReady
Check 'F15 nach dem Tod in der Stadt' (Eval 'b_state.cs') '^Szene=Base .* Ankunft=Start$'

StopPlay
TestResult 'Frost-Test' $f0
