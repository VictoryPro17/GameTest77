# Eingabe-Nachweis (Test "diag"): Kommen simulierte Tastendruecke und Stick-Bewegungen im Spiel an?
# Start ueber run_all.ps1 -Tests diag (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Eingabe-Nachweis ---'
StartPlay
'  ' + (Eval 't2_setup.cs')
[void](Eval 't_kb_d.cs'); Wait 400
Check 'Taste D kommt an' (Eval 'diag_input.cs') '^D=True .* move=1\.0'
[void](Eval 't2_release.cs'); Wait 300
[void](Eval 't_press.cs'); Wait 400
Check 'Stick kommt an' (Eval 'diag_input.cs') 'stick=1\.0 move=1\.0'
[void](Eval 't2_release.cs')
StopPlay
TestResult 'Eingabe-Nachweis' $f0
