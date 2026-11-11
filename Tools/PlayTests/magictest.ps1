# Magie-Test (Meilenstein 3): Mana, 4 Zauber, Stufen 1-10, Meditieren, Speichern, Menue-Reiter.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
$K = 'UnityEngine.InputSystem.Key.'
function Key([string]$keys, [int]$hold = 150) { [void](Eval 'mg_key.cs' @{ TASTEN = $keys }); Wait $hold; [void](Eval 't2_release.cs') }

'--- Magie-Test ---'
StartPlay
'  ' + (Eval 't2_setup.cs')

'Z1) Start'
Check 'Z1 Mana und Freischaltung' (Eval 'mg_state.cs') '^Mana=25/25 frei=True,False,False,False Stufen=1,1,1,1 ManaStufe=1'

'Z2/Z3) Wasserkugel'
[void](Eval 'mg_slimes.cs' @{ POS = '0,-3' })
[void](Eval 'mg_key.cs' @{ TASTEN = "${K}Digit1" }); Wait 120
$r = Eval 'mg_cast_check.cs'
[void](Eval 't2_release.cs')
Shot 'm3_wasserkugel'
Check 'Z2 Taste 1 zaubert, Mana 20, Ausruf' $r '^Mana=20 Effekte=[01] Zauber=1 LastFail= Ausruf=Wasser-Erschaffungsmagie: Wasserkugel! \| Banner=$'
Wait 700
Check 'Z3 Wasserkugel trifft (30 -> 21)' (Eval 'mg_slimehp.cs') '^Schleime=21 '

'Z4) Zu wenig Mana'
[void](Eval 'mg_setmana.cs' @{ M = '3' })
Key "${K}Digit1"
Check 'Z4 kein Zauber, Mana bleibt 3' (Eval 'mg_cast_check.cs') '^Mana=3 Effekte=0 Zauber=1 LastFail=mana'

'Z5) Abklingzeit'
Check 'Z5 zweimal schnell = ein Zauber' (Eval 'mg_double.cs') '^Zauber=1 erster=True zweiter=False LastFail=abklingzeit$'
Wait 900

'Z6) Stufe 2 durch 10 Treffer'
Check 'Z6 Stufe 2, Schaden 11' (Eval 'mg_hits.cs' @{ SLOT = '0'; N = '10' }) '^Stufe=2 XP=\d+ Schaden=11 frei=False,False,False$'

'Z7) Wasserdrache'
Check 'Z7 Stufe 3 schaltet den Drachen frei' (Eval 'mg_hits.cs' @{ SLOT = '0'; N = '20' }) '^Stufe=3 XP=\d+ Schaden=14 frei=True,False,False$'
[void](Eval 'mg_slimes.cs' @{ POS = '0,-1.5;0,-3;0,-4.3' })
[void](Eval 'mg_setmana.cs' @{ M = '25' })
[void](Eval 'mg_key.cs' @{ TASTEN = "${K}Digit2" }); Wait 250
$r = Eval 'mg_cast_check.cs'
Shot 'm3_wasserdrache'
[void](Eval 't2_release.cs')
Check 'Z7 grosser Ausruf' $r 'Zauber=\d+ LastFail= Ausruf=Wasser-Erschaffungsmagie: Wasserdrache! \| Banner=WASSERDRACHE!$'
Wait 1200
Check 'Z7 Drache trifft alle 3 je einmal (30 -> 9)' (Eval 'mg_slimehp.cs') '^Schleime=9,9,9 '

'Z8) Feuerball'
Check 'Z8 Level 2 schaltet Feuerball frei' (Eval 'mg_levelup.cs') 'Level=2 Feuerball frei=True'
[void](Eval 'mg_slimes.cs' @{ POS = '0,-3;0.6,-3' })
[void](Eval 'mg_setmana.cs' @{ M = '25' })
[void](Eval 'mg_key.cs' @{ TASTEN = "${K}Digit3" }); Wait 380
Shot 'm3_feuerball'
[void](Eval 't2_release.cs')
Wait 600
Check 'Z8 Treffer 13 + Explosion 7 (30 -> 17 und 23)' (Eval 'mg_slimehp.cs') '^Schleime=17,23 '
Wait 400

'Z9) Flammenring'
Check 'Z9 Feuerball Stufe 3 schaltet den Ring frei' (Eval 'mg_hits.cs' @{ SLOT = '2'; N = '30' }) '^Stufe=3 XP=\d+ .* frei=True,True,True$'
[void](Eval 'mg_slimes.cs' @{ POS = '1.2,0;-1.2,0;0,1.2;0,-1.2' })
[void](Eval 'mg_setmana.cs' @{ M = '25' })
[void](Eval 'mg_key.cs' @{ TASTEN = "${K}Digit4" }); Wait 200
Shot 'm3_flammenring'
[void](Eval 't2_release.cs')
Wait 500
$r = Eval 'mg_slimehp.cs'
Check 'Z9 Ring trifft alle 4 (30 -> 14)' $r '^Schleime=14,14,14,14 '
$dists = @(); if ($r -match 'Abstand=([0-9.,]+)$') { $dists = $Matches[1].Split(',') | ForEach-Object { [double]::Parse($_, $Inv) } }
CheckTrue 'Z9 Ring stoesst weg (Abstand > 1,4)' ($dists.Count -eq 4 -and ($dists | Where-Object { $_ -le 1.4 }).Count -eq 0) "Abstaende: $($dists -join ', ')"
[void](Eval 'mg_slimes.cs' @{ POS = '' })

'Z10) Meditieren'
[void](Eval 'mg_setmana.cs' @{ M = '0' })
$before = Eval 'mg_pos.cs'
[void](Eval 'mg_key.cs' @{ TASTEN = "${K}M, ${K}D" }); Wait 900
$during = Eval 'mg_pos.cs'
Shot 'm3_meditieren'
Wait 1100
$after = Eval 'mg_pos.cs'
[void](Eval 't2_release.cs')
"  vorher:  $before"
"  waehrend: $during"
"  nachher: $after"
Check 'Z10 meditiert' $during 'meditiert=True'
CheckRange 'Z10 mindestens 9 Mana mehr' $after 'Mana' 9 25
$xpB = [int]([regex]::Match($before, 'ManaXP=(\d+)').Groups[1].Value); $xpA = [int]([regex]::Match($after, 'ManaXP=(\d+)').Groups[1].Value)
CheckTrue 'Z10 Mana-Erfahrung steigt' ($xpA -gt $xpB) "ManaXP $xpB -> $xpA"
CheckTrue 'Z10 Spieler bleibt stehen (trotz D)' ($before -match 'x=0\.000' -and $after -match 'x=0\.000') "vorher: $before | nachher: $after"

'Z11) Mana-Stufe'
Check 'Z11 50 Erfahrung = Stufe 2, Maximum 33' (Eval 'mg_manalevel.cs') '^ManaStufe=2 Max 25->33$'

'Z12) Hoechste Stufe'
Check 'Z12 Stufe bleibt bei 10' (Eval 'mg_cap.cs') '^Stufe=10 XP=0 ManaStufe=10 ManaXP=0 Max=97 Schaden=29$'

'Z13) Speichern'
Check 'Z13 gespeichert' (Eval 'mg_save.cs') 'gespeichert=True'
$s1 = Eval 'mg_summary.cs'
StopPlay
StartPlay -KeepSave
'  ' + (Eval 't2_setup.cs')
$s2 = Eval 'mg_summary.cs'
CheckTrue 'Z13 nach Neustart gleich' ($s1 -eq $s2 -and $s1 -notmatch '^ROH') "vorher: $s1 | nachher: $s2"
StopPlay
[void](Eval 'mg_writev1.cs')
unity command console clear @U 2>$null | Out-Null  # nur Fehler aus diesem Ladevorgang zaehlen
StartPlay -KeepSave
'  ' + (Eval 't2_setup.cs')
Check 'Z13 alter Spielstand (Version 1) laedt' (Eval 'mg_summary.cs') '^Lv3 Zauber\[1:0 1:0 1:0 1:0\] ManaStufe=1 ManaXP=0 Mana=30/30$'
$err = unity command console get @U -- --tail 50 --level error 2>$null | Out-String
CheckTrue 'Z13 keine Fehler beim Laden' ($err -notmatch 'Spielstand konnte nicht|NullReference|Exception') 'Konsole ohne Ladefehler'

'Z14) Menue-Reiter Magie'
[void](Eval 't2_pad_start.cs'); Wait 300; [void](Eval 't2_release.cs'); Wait 300
[void](Eval 'mg_menu.cs')
[void](Eval 't2_pad_south.cs'); Wait 300; [void](Eval 't2_release.cs'); Wait 300
Check 'Z14 Magie-Reiter zeigt Werte' (Eval 'mg_menu_check.cs') '^Seite=2 \| Wasserkugel  Stufe 1/10 .* \| Wasser \u00B7 Mana 5 \u00B7 Schaden 10 \u00B7 Abklingzeit 0[.,]5 s \u00B7 Erfahrung 0/10 \| Gesperrt: Ab Feuerball Stufe 3$'
Shot 'm3_magie_reiter'
[void](Eval 't2_pad_start.cs'); Wait 300; [void](Eval 't2_release.cs'); Wait 300
Shot 'm3_hud_mana'

StopPlay
TestResult 'Magie-Test' $f0
