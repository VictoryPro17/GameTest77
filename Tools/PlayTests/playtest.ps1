# Spieltest Meilenstein 1: Laufen, Kampf gegen alle 5 Schleime, Beute einsammeln, Level-Aufstieg.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Spieltest: Laufen, Kampf, Beute, Level ---'
StartPlay
'0) ' + (Eval 't_setup.cs')
Wait 800

'1) Bewegung:'
'  ' + (Eval 't_press.cs')
Wait 600
Shot 'test_2_laufen'
$r = Eval 't_release.cs'
Check 'Laufen: Animation und Blick' $r 'clip=walk facing=Right'
CheckRange 'Laufen: Strecke' $r 'x' 3 5.5

'2) Kampf gegen alle 5 Schleime:'
for ($k = 0; $k -lt 18; $k++) {
    $r = Eval 't_fight.cs'
    if ($r -eq 'keine Schleime mehr') { break }
    if ($k -eq 1) { Wait 120; Shot 'test_3_angriff' }
    Wait 550
    $s = Eval 't_status.cs'
    "  $r -> $s"
    Wait 150
}
Wait 500
Check 'Kampf: alle Schleime besiegt, Level 2' (Eval 't_status.cs') '^Lv 2 XP 50/200 HP 60/60 \| Schleime: \| Beute am Boden: \d+ \| Inventar: '
Shot 'test_4_beute'

'3) Beute einsammeln:'
for ($k = 0; $k -lt 15; $k++) {
    $r = Eval 't_collect.cs'
    if ($r -eq 'keine Beute am Boden') { break }
    Wait 500
}
Check 'Beute: alles eingesammelt' (Eval 't_status.cs') 'Beute am Boden: 0 \| Inventar: .*Schleimgel x\d+'
Shot 'test_5_ende'

StopPlay
TestResult 'Spieltest' $f0
