# Tastatur-Test: D laeuft nach rechts, J greift an, Leertaste weicht aus (virtuelle Tastatur).
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Tastatur-Test ---'
StartPlay
'0) ' + (Eval 't_kb_setup.cs')

'1) Laufen mit D:'
'  ' + (Eval 't_kb_d.cs')
Wait 600
$r = Eval 't_kb_release.cs'
Check 'D: Blick nach rechts' $r 'facing=Right'
CheckRange 'D: Strecke' $r 'x' 2 5.5
Wait 400

'2) Angriff mit J:'
'  ' + (Eval 't_kb_j.cs')
Wait 50
Check 'J: Angriffs-Animation' (Eval 't_kb_check_attack.cs') 'clip=attack'
[void](Eval 't_kb_release.cs')
Wait 600

'3) Ausweichen mit Leertaste:'
[void](Eval 't_kb_watch.cs')
'  ' + (Eval 't_kb_space.cs')
Wait 400
Check 'Leertaste: Ausweichen (mindestens 1 Ausweich-Bild)' (Eval 't_kb_watch_check.cs') '^Ausweich-Bilder=[1-9]\d*$'
[void](Eval 't_kb_release.cs')

'  ' + (Eval 't_kb_remove.cs')
StopPlay
TestResult 'Tastatur-Test' $f0
