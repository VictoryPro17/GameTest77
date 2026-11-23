# Boss-Test (Meilenstein 8): Golem im Kampfraum - schlaeft, wacht in der Naehe auf, Lebensleiste, Schlag trifft,
# Wurzelangriff trifft (und nicht, wenn man ausweicht), Phase 2 bei 50 % mit 3 Woelfen, Phase 3 bei 25 % mit 3 Wurzeln,
# Sieg mit Beute (Herz des Waldes, Gold, EP), gespeichert, erscheint nicht wieder; Tod gegen den Boss -> Stadt.
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

'S6) Sieg'
Check 'S6 Beute, Gold, EP, gespeichert' (Eval 'bs_kill.cs') '^tot=True Beute=.*herz_des_waldesx1.* Gold=400 EP\+=True gespeichert=waldwaechter Version=5 '
Wait 3500
Check 'S6 Boss weg, Leiste aus' (Eval 'bs_state.cs') '^kein Boss Leiste=False '
StopPlay

'S7) Besiegt bleibt besiegt'
StartPlay -KeepSave
'  ' + (Eval 't2_setup.cs')
Check 'S7 kein Boss nach Neustart' (Eval 'bs_state.cs') '^kein Boss '
StopPlay

'S8) Tod gegen den Boss -> Stadt'
StartPlay
'  ' + (Eval 't2_setup.cs')
'  ' + (Eval 'bs_wake.cs' @{ HP = '50'; X = '0'; Y = '2.7' }); Wait 8000; WaitReady
Check 'S8 nach dem Tod in der Stadt' (Eval 'b_state.cs') '^Szene=Base .* Ankunft=Start$'

StopPlay
TestResult 'Boss-Test' $f0
