# Demo-Test (Meilenstein 10c): in der Demo bleibt das Siegel zum Frostgipfel zu, auch mit Schluessel; Hauptmenue zeigt "Demo".
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Dm([string]$act) { return (Eval 'dm_lock.cs' @{ ACT = $act }) }

'--- Demo-Test ---'
'D1) Siegel in der Demo'
StartPlay -Scene Cave
Check 'D1 normal: keine Demo' (Dm 'state') '^Demo=False '
[void](Dm 'on')
[void](Dm 'givekey')
Check 'D1 Demo: Siegel bleibt zu, Schluessel bleibt' (Dm 'unseal') '^Demo=True offen=False Betreten=False Schluessel=1 Flag=False Meldung=Frostgipfel . nur in der Vollversion$'
[void](Dm 'off')
Check 'D1 ohne Demo: Schluessel oeffnet' (Dm 'unseal') '^Demo=False offen=True Betreten=True Schluessel=0 Flag=True '
StopPlay

'D2) Hauptmenue zeigt Demo'
StartPlay -Scene MainMenu
Check 'D2 ohne Demo keine Demo-Kennung' (Eval 'mm_state.cs') 'Version=Beta 0\.9\.0 '
StopPlay
TestResult 'Demo-Test' $f0