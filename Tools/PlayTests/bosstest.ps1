# Boss-Test (Meilenstein 8): Golem im Kampfraum - schlaeft, wacht in der Naehe auf, Lebensleiste, Schlag trifft,
# Wurzelangriff trifft (und nicht, wenn man ausweicht), Phase 2 bei 50 % mit 3 Woelfen, Phase 3 bei 25 % mit 3 Wurzeln,
# Stufe 2 (8b): Erwachen, Dornenwelle, Ansturm, Ultimate, Wolfsruf; Sieg mit Beute, gespeichert, nach 10 Minuten wieder da; Tod -> Stadt.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Num([string]$text, [string]$key) { if ($text -match "$key=(-?\d+)") { return [int]$Matches[1] }; return -1 }

'--- Boss-Test ---'
StartPlay
'  ' + (Eval 't2_setup.cs')

'S1) Schlafen und Aufwachen'
'  ' + (Eval 'bs_wake.cs' @{ HP = '1000'; X = '0'; Y = '0.5' }); Wait 400
Check 'S1 schlaeft auf Abstand, keine Leiste' (Eval 'bs_state.cs') '^wach=False Zustand=Sleep Phase=1 HP=1200/1200 Leiste=False '
[void](Eval 'b_goto.cs' @{ X = '0'; Y = '2.6' }); Wait 500
Check 'S1 wacht auf, Leiste mit Namen' (Eval 'bs_state.cs') '^wach=True .* Phase=1 HP=1200/1200 Leiste=True Name=Waldw.chter '
Shot 'm8_boss_wach'

'S2) Schlag aus der Naehe'
Wait 2500
$r = Eval 'bs_state.cs'
CheckTrue 'S2 Schlag ausgefuehrt und getroffen' ((Num $r 'Schlaege') -ge 1 -and (Num $r 'Spieler') -le 970) $r

'S3) Wurzelangriff aus der Ferne'
[void](Eval 'b_goto.cs' @{ X = '7'; Y = '-3.5' }); Wait 3000
$r = Eval 'bs_state.cs'
CheckTrue 'S3 Wurzeln ausgeloest und getroffen' ((Num $r 'Wurzeln') -ge 1 -and (Num $r 'Ausgeloest') -ge 1 -and (Num $r 'WurzelTreffer') -ge 1) $r
Shot 'm8_boss_wurzeln'
[void](Eval 'bs_hit.cs' @{ N = '0'; INV = 'true' })   # wie Ausweichen: unverwundbar
$before = Eval 'bs_state.cs'
[void](Eval 'b_goto.cs' @{ X = '-7'; Y = '-3.5' }); Wait 5000
$r = Eval 'bs_state.cs'
CheckTrue 'S3 beim Ausweichen kein Wurzel-Schaden' ((Num $r 'Ausgeloest') -gt (Num $before 'Ausgeloest') -and (Num $r 'WurzelTreffer') -eq (Num $before 'WurzelTreffer')) "vorher: $before | nachher: $r"

'S4) Phase 2 bei 50 %'
[void](Eval 'bs_hit.cs' @{ N = '600'; INV = 'true' }); Wait 300
Check 'S4 Phase 2, 3 Woelfe' (Eval 'bs_state.cs') ' Phase=2 HP=600/1200 Leiste=True Name=Waldw.chter  \(Phase 2\) .* Woelfe=3 '

'S5) Phase 3 (Wut) bei 25 %: Wurzeln an 3 Stellen'
[void](Eval 'bs_hit.cs' @{ N = '300'; INV = 'true' }); Wait 200
$before = Eval 'bs_state.cs'
Check 'S5 Phase 3' $before ' Phase=3 HP=300/1200 .*\(Wut!\) '
[void](Eval 'b_goto.cs' @{ X = '7'; Y = '-3.5' }); Wait 4500
$r = Eval 'bs_state.cs'
CheckTrue 'S5 mindestens 3 Wurzeln auf einmal' ((Num $r 'Ausgeloest') - (Num $before 'Ausgeloest') -ge 3) "vorher: $before | nachher: $r"
Shot 'm8_boss_wut'

'S6) Stufe 1 besiegt -> Uralter Waldwaechter (Stufe 2, Meilenstein 8b)'
[void](Eval 'bs2_do.cs' @{ ACT = 'hit:99999' }); Wait 300
Check 'S6 erwacht: Stufe 2, 1500 Leben, unverwundbar' (Eval 'bs2_state.cs') '^Stufe=2 Zustand=Transform HP=1500/1500 unverwundbar=True Name=Uralter Waldw.chter  \(Stufe 2\) '
[void](Eval 'bs2_do.cs' @{ ACT = 'hit:100' })
Check 'S6 waehrend der Verwandlung kein Schaden' (Eval 'bs2_state.cs') ' HP=1500/1500 '
Wait 3200
[void](Eval 'bs2_do.cs' @{ ACT = 'timers:999:999:999:999' })
Check 'S6 Verwandlung vorbei' (Eval 'bs2_state.cs') '^Stufe=2 Zustand=(?!Transform)\w+ HP=1500/1500 unverwundbar=False '
Shot 'm8b_stufe2'

'S7) Dornenwelle'
foreach ($a in 'hp:1000', 'inv:false', 'place:2:0', 'timers:999:999:0:999') { [void](Eval 'bs2_do.cs' @{ ACT = $a }) }
Wait 1800
$r = Eval 'bs2_state.cs'
CheckTrue 'S7 Dornenwelle trifft' ((Num $r 'Wellen') -ge 1 -and (Num $r 'WellenTreffer') -ge 1) $r
foreach ($a in 'inv:true', 'place:2:0', 'timers:999:999:0:999') { [void](Eval 'bs2_do.cs' @{ ACT = $a }) }
Wait 1800
$r2 = Eval 'bs2_state.cs'
CheckTrue 'S7 beim Ausweichen kein Dornen-Schaden' ((Num $r2 'Wellen') -gt (Num $r 'Wellen') -and (Num $r2 'WellenTreffer') -eq (Num $r 'WellenTreffer')) "vorher: $r | nachher: $r2"

'S8) Ansturm'
foreach ($a in 'inv:false', 'hp:1000', 'placec:4', 'timers:999:999:999:0:999') { [void](Eval 'bs2_do.cs' @{ ACT = $a }) }   # Wurzeln aus, Richtung Raummitte
Wait 2400
$r = Eval 'bs2_state.cs'
CheckTrue 'S8 Ansturm ausgefuehrt und getroffen' ((Num $r 'Ansturm') -ge 1 -and (Num $r 'AnsturmTreffer') -ge 1 -and (Num $r 'Spieler') -le 970) $r

'S9) Ultimate "Zorn des Waldes"'
foreach ($a in 'hp:1000', 'place:3:0', 'timers:0:999:999:999') { [void](Eval 'bs2_do.cs' @{ ACT = $a }) }
Wait 700
Check 'S9 laedt, Warnung sichtbar' (Eval 'bs2_state.cs') ' laedt=True Warnung=True '
Shot 'm8b_ultimate'
Wait 4000
$r = Eval 'bs2_state.cs'
CheckTrue 'S9 im Kreis: 50 % Schaden' ((Num $r 'Ultimate') -eq 1 -and (Num $r 'Spieler') -le 500 -and (Num $r 'Spieler') -ge 300) $r
foreach ($a in 'hp:1000', 'place:7.5:0', 'timers:0:999:999:999') { [void](Eval 'bs2_do.cs' @{ ACT = $a }) }
Wait 4800
$r = Eval 'bs2_state.cs'
CheckTrue 'S9 ausserhalb: kein Ultimate-Schaden' ((Num $r 'Ultimate') -eq 2 -and (Num $r 'Spieler') -eq 1000) $r

'S10) Wolfsruf: verwundete Woelfe'
[void](Eval 'bs2_do.cs' @{ ACT = 'timers:999:0:999:999' }); Wait 1200
Check 'S10 2 Woelfe mit halbem Leben' (Eval 'bs2_state.cs') ' WolfRufe=1 Woelfe=2 WolfHP=20 '

'S11) Sieg ueber Stufe 2'
Check 'S11 Beute, Gold, EP, gespeichert' (Eval 'bs_kill.cs') '^tot=True Beute=.*herz_des_waldesx1.* Gold=400 EP\+=True gespeichert=waldwaechter Version=6 '
Wait 3500
Check 'S11 Boss weg, Leiste aus' (Eval 'bs_state.cs') '^kein Boss Leiste=False '
StopPlay

'S12) Nach Neustart noch besiegt, nach 10 Minuten wieder da'
StartPlay -KeepSave
Check 'S12 kein Boss direkt nach dem Sieg' (Eval 'bs2_state.cs') '^kein Boss '
[void](Eval 'bs2_do.cs' @{ ACT = 'clock:11' }); Wait 2500; WaitReady
Check 'S12 nach 11 Minuten wieder da (kein Erst-Sieg)' (Eval 'bs2_state.cs') '^Stufe=1 Zustand=Sleep HP=1200/1200 .* Erst=False '
'  ' + (Eval 't2_setup.cs')
'  ' + (Eval 'bs_wake.cs' @{ HP = '5000'; X = '0'; Y = '2.6' }); Wait 600
[void](Eval 'bs2_do.cs' @{ ACT = 'hit:99999' }); Wait 3400
Check 'S12 zweiter Sieg: Beute ohne Amulett' (Eval 'bs_kill.cs') '^tot=True Beute=heiltrankx2,manatrankx1 Gold=300 EP\+=True '
StopPlay
'S13) Tod gegen den Boss -> Stadt'
StartPlay
'  ' + (Eval 't2_setup.cs')
'  ' + (Eval 'bs_wake.cs' @{ HP = '50'; X = '0'; Y = '2.7' }); Wait 8000; WaitReady
Check 'S13 nach dem Tod in der Stadt' (Eval 'b_state.cs') '^Szene=Base .* Ankunft=Start$'

StopPlay
TestResult 'Boss-Test' $f0
