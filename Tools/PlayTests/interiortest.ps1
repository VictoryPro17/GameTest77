# Raum-Test (Meilenstein 7b): in jede Gildenhalle hinein und wieder hinaus, Waende, Gildenmeister mit Sprechblase,
# Hoehle -> Portal -> Kampfraum -> zurueck in die Hoehle -> Stadt; Gold bleibt ueber alle Wechsel erhalten.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Raum-Test ---'
StartPlay -Scene 'Base'
'  ' + (Eval 't2_setup.cs')
[void](Eval 'b_shop.cs' @{ ACT = 'setgold:77' })
$n = 0

$rooms = @(
    @{ door = 'Tuer_Gilde_Krieger';   scene = 'Guild_Warrior';  light = '0.60'; master = 'Kriegerin Brunhild';    spawn = 'Tuer_Krieger';   pos = '-9\.5,8\.6'; shot = 'm7b_krieger' },
    @{ door = 'Tuer_Gilde_Magier';    scene = 'Guild_Mage';     light = '0.60'; master = 'Erzmagier Aldric';      spawn = 'Tuer_Magier';    pos = '-0\.5,9\.9'; shot = 'm7b_magier' },
    @{ door = 'Tuer_Gilde_Assassine'; scene = 'Guild_Assassin'; light = '0.45'; master = 'Schattenmeisterin Nyx'; spawn = 'Tuer_Assassine'; pos = '9\.5,8\.7';  shot = 'm7b_assassine' }
)
foreach ($r in $rooms) {
    "I) $($r.scene)"
    [void](Eval 'b_door_go.cs' @{ NAME = $r.door }); Wait 2500; WaitReady; $n++
    Check "I $($r.scene): drinnen am Eingang" (Eval 'b_state.cs') "^Szene=$($r.scene) Pos=\(0\.0,-3\.6\) Gold=77 .* Reisen=$n reist=False Ankunft=Eingang$"
    '  ' + (Eval 't2_setup.cs')
    Check "I $($r.scene): Licht, friedlich, Meister, Kacheln, Bilder" (Eval 'i_room.cs') "^Szene=$($r.scene) Licht=$($r.light) friedlich=True Meister=$($r.master) Boden=[1-9]\d* Mauerkacheln=[1-9]\d* Lichter=[1-9]\d* ohneBild=0 Mitglieder=2$"
    [void](Eval 'w_walk.cs' @{ PX = '0'; PY = '-1'; SX = '1'; SY = '0' }); Wait 1500
    CheckRange "I $($r.scene): rechte Wand haelt" (Eval 'w_pos.cs') 'x' 6.65 6.85
    [void](Eval 'b_goto.cs' @{ X = '0'; Y = '1.6' }); Wait 300
    [void](Eval 'b_key.cs' @{ KEY = 'E' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 200
    Check "I $($r.scene): Gildenmeister oeffnet das Gilden-Fenster" (Eval 'b_trade.cs') ' offen=True Seite=Gilde '   # Meilenstein 7c
    [void](Eval 'b_close.cs')
    [void](Eval 'lk_zoom.cs' @{ SIZE = '4.5'; X = '0'; Y = '0.8' }); Wait 500
    Shot $r.shot
    [void](Eval 'b_door_go.cs' @{ NAME = 'Tuer_Ausgang' }); Wait 2500; WaitReady; $n++
    Check "I $($r.scene): zurueck vor der Tuer" (Eval 'b_state.cs') "^Szene=Base Pos=\($($r.pos)\) Gold=77 .* Reisen=$n reist=False Ankunft=$($r.spawn)$"
    '  ' + (Eval 't2_setup.cs')
}

'H) Hoehle'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Tuer_Hoehle' }); Wait 2500; WaitReady; $n++
Check 'H in der Hoehle am Eingang' (Eval 'b_state.cs') "^Szene=Cave Pos=\(0\.0,-3\.6\) Gold=77 .* Reisen=$n reist=False Ankunft=Eingang$"
'  ' + (Eval 't2_setup.cs')
Check 'H Licht, friedlich, Kacheln, Kristall-Lichter' (Eval 'i_room.cs') '^Szene=Cave Licht=0\.40 friedlich=True Meister=- Boden=[1-9]\d* Mauerkacheln=[1-9]\d* Lichter=[1-9]\d* ohneBild=0 Mitglieder=0$'
[void](Eval 'lk_zoom.cs' @{ SIZE = '4.5'; X = '0'; Y = '0' }); Wait 500
Shot 'm7b_hoehle'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Kampf' }); Wait 2500; WaitReady; $n++
Check 'H Portal fuehrt in den Kampfraum' (Eval 'b_state.cs') "^Szene=Room_Test .* Gold=77 .* Reisen=$n reist=False Ankunft=Ankunft$"
'  ' + (Eval 't2_setup.cs')
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Basis' }); Wait 2500; WaitReady; $n++
Check 'H Rueckkehr-Portal fuehrt in die Hoehle' (Eval 'b_state.cs') "^Szene=Cave Pos=\(0\.0,1\.1\) Gold=77 .* Reisen=$n reist=False Ankunft=Portal$"
'  ' + (Eval 't2_setup.cs')
[void](Eval 'b_door_go.cs' @{ NAME = 'Tuer_Ausgang' }); Wait 2500; WaitReady; $n++
Check 'H Hoehlenausgang fuehrt vor die Felsen' (Eval 'b_state.cs') "^Szene=Base Pos=\(-18\.5,9\.3\) Gold=77 .* Reisen=$n reist=False Ankunft=Portal$"
'  ' + (Eval 't2_setup.cs')
[void](Eval 'lk_zoom.cs' @{ SIZE = '5'; X = '0'; Y = '8' }); Wait 600
Shot 'm7b_gildentueren'

StopPlay
TestResult 'Raum-Test' $f0
