# Stadt-Test (Meilenstein 7a): Gebaeude, Mauer, Bewohner und Soldaten; Wege zu Tor, Magierturm, Mauer, Laeden und Hoehle
# sind frei (echtes Laufen mit dem Stick); die drei neuen Haendler; Hoehle fuehrt in den Kampfraum; Uebersichtsbilder.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Stadt-Test ---'
StartPlay -Scene 'Base'
'  ' + (Eval 't2_setup.cs')

'T1) Was in der Stadt steht'
Check 'T1 Haeuser, Laeden, Gilden, Mauer, Leute, Soldaten, Hoehle, alle Bilder da' (Eval 'tw_census.cs') '^Haeuser=15 Laeden=4 Gilden=3 Mauerteile=([5-9]\d|\d{3,}) Leute=18 Soldaten=5 Hoehle=Cave ohneBild=0$'

'T2) Bewohner und Soldaten laufen'
Wait 9000
Check 'T2 alle laufenden Figuren haben sich bewegt' (Eval 'tw_people.cs') '^laufend=15 bewegt=15 Soldaten=5$'

'T3) Wege (mit dem Stick laufen)'
[void](Eval 'w_walk.cs' @{ PX = '0'; PY = '-6'; SX = '0'; SY = '-1' }); Wait 2600
CheckRange 'T3 Hauptstrasse bis zum Tor im Sueden' (Eval 'w_pos.cs') 'y' -16.95 -16.7
[void](Eval 'w_walk.cs' @{ PX = '-1.3'; PY = '-1'; SX = '0'; SY = '1' }); Wait 2600
CheckRange 'T3 Hauptstrasse bis zum Magierturm' (Eval 'w_pos.cs') 'y' 9.5 10.9
[void](Eval 'w_walk.cs' @{ PX = '-0.5'; PY = '6.5'; SX = '-1'; SY = '0' }); Wait 5500
CheckRange 'T3 Gildenstrasse bis zur Mauer im Westen' (Eval 'w_pos.cs') 'x' -21.85 -21.65
[void](Eval 'w_walk.cs' @{ PX = '0'; PY = '-10'; SX = '1'; SY = '0' }); Wait 5500
CheckRange 'T3 Ladenstrasse bis zur Mauer im Osten' (Eval 'w_pos.cs') 'x' 21.65 21.85
[void](Eval 'w_pos.cs')

'T4) Haendler in der Ladenstrasse'
[void](Eval 'b_goto.cs' @{ X = '-8.5'; Y = '-10.1' }); Wait 300
Check 'T4 Waffenhaendler in der Naehe' (Eval 'b_trade.cs') '^nah=Waffenh.ndler offen=False'
[void](Eval 'b_key.cs' @{ KEY = 'E' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 200
Check 'T4 E oeffnet den Waffenhaendler' (Eval 'b_trade.cs') ' offen=True Seite=Haendler'
[void](Eval 'b_shop.cs' @{ ACT = 'setgold:200' })
Check 'T4 Eisenschwert kaufen (160 Gold)' (Eval 'b_shop.cs' @{ ACT = 'buy:eisenschwert' }) '^Meldung=Gekauft: Eisenschwert \| Gold=40 Anzahl=1$'
Shot 'm7a_waffenhaendler'
[void](Eval 'b_close.cs')
[void](Eval 'b_goto.cs' @{ X = '-4.5'; Y = '-10.1' }); Wait 300
Check 'T4 Ruestungshaendler in der Naehe' (Eval 'b_trade.cs') '^nah=R.stungsh.ndler offen=False'
[void](Eval 'b_goto.cs' @{ X = '4.5'; Y = '-10.1' }); Wait 300
Check 'T4 Alchemistin in der Naehe' (Eval 'b_trade.cs') '^nah=Alchemistin offen=False'

'T5) Uebersichtsbilder'
foreach ($v in @(@('m7a_marktplatz', '0', '-1'), @('m7a_gilden', '0', '8'), @('m7a_laeden', '0', '-10.6'),
                 @('m7a_hoehle', '-17', '8.6'), @('m7a_mauer_soldaten', '11', '13'), @('m7a_teich', '16', '-10'))) {
    [void](Eval 'lk_zoom.cs' @{ SIZE = '6'; X = $v[1]; Y = $v[2] }); Wait 900
    Shot $v[0]
}
[void](Eval 'lk_zoom.cs' @{ SIZE = '3.5'; X = '0'; Y = '-2' }); Wait 600
Shot 'm7a_start'

'T6) Durch die Hoehle in den Kampfraum (laufen)'
[void](Eval 'w_walk.cs' @{ PX = '-18.5'; PY = '9.3'; SX = '0'; SY = '1' }); Wait 3500; WaitReady
[void](Eval 'w_pos.cs')
Check 'T6 Kampfraum erreicht' (Eval 'b_state.cs') '^Szene=Room_Test .* Reisen=1 reist=False Ankunft=Ankunft$'

StopPlay
TestResult 'Stadt-Test' $f0
