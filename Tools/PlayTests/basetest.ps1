# Basis-Test (Meilenstein 6): Start in der Basis, friedlich, Mauer, Haendler (Kaufen, Verkaufen, Gold, Inventar voll),
# Schmied (Verbessern mit Gold und Schleimgel), Speichern/Laden, Portal hin und zurueck, Tod -> Basis mit -10 % Gold,
# alter Spielstand (Version 2), Bedienung mit Tastatur (E, Esc) und Controller (Steuerkreuz unten).
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Basis-Test ---'
StartPlay -Scene 'Base'
'  ' + (Eval 't2_setup.cs')

'B1) Start in der Basis'
Check 'B1 Basis, Startpunkt, friedlich, 2 NPCs, Portal, Mauer, Boden' (Eval 'b_start.cs') '^Szene=Base Pos=\(0\.0,-2\.0\) friedlich=True Gold=0 NPCs=2 Portal=Portal_Kampf>Room_Test Mauer=[1-9]\d* Boden=[1-9]\d* Mauerkacheln=[1-9]\d*$'

'B2) Kein Kampf in der Basis'
Check 'B2 kein Zauber' (Eval 'b_cast.cs') '^gezaubert=False Grund=friedlich$'
[void](Eval 'b_key.cs' @{ KEY = 'J' }); Wait 150
Check 'B2 kein Angriff (J)' (Eval 'a_clip.cs') '^clip=idle '
[void](Eval 'b_keyup.cs'); Wait 200

'B3) Mauer'
[void](Eval 'w_walk.cs' @{ PX = '-6.5'; PY = '0'; SX = '-1'; SY = '0' }); Wait 1200
CheckRange 'B3 Mauer links haelt' (Eval 'w_pos.cs') 'x' -6.85 -6.65

'B4) Haendler ansprechen (E)'
[void](Eval 'b_goto.cs' @{ X = '-0.5'; Y = '-2.4' }); Wait 300
Check 'B4 Hinweis in der Naehe' (Eval 'b_trade.cs') '^nah=H.ndler offen=False Seite=- Zeit=1 Menue=False Hinweis=E: Mit H.ndler sprechen$'
[void](Eval 'b_key.cs' @{ KEY = 'E' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 200
Check 'B4 E oeffnet den Haendler, Spiel pausiert' (Eval 'b_trade.cs') ' offen=True Seite=Haendler Zeit=0 Menue=False'
Shot 'm6_haendler'

'B5-B7) Kaufen und Verkaufen'
Check 'B5 zu wenig Gold' (Eval 'b_shop.cs' @{ ACT = 'buy:heiltrank_klein' }) '^Meldung=Nicht genug Gold \| Gold=0 Anzahl=0$'
[void](Eval 'b_shop.cs' @{ ACT = 'gold:100' })
Check 'B6 Kaufen: -20 Gold, Trank im Inventar' (Eval 'b_shop.cs' @{ ACT = 'buy:heiltrank_klein' }) '^Meldung=Gekauft: Kleiner Heiltrank \| Gold=80 Anzahl=1$'
[void](Eval 'b_shop.cs' @{ ACT = 'fill' })
Check 'B6 Inventar voll: kein Kauf, kein Gold weg' (Eval 'b_shop.cs' @{ ACT = 'buy:heiltrank_klein' }) '^Meldung=Inventar voll \| Gold=80 Anzahl=0$'
[void](Eval 'b_shop.cs' @{ ACT = 'clear' })
[void](Eval 'b_shop.cs' @{ ACT = 'buy:heiltrank_klein' })
Check 'B7 Verkaufen: +5 Gold' (Eval 'b_shop.cs' @{ ACT = 'sell:heiltrank_klein' }) '^Meldung=Verkauft: Kleiner Heiltrank \(\+5 Gold\) \| Gold=65 Anzahl=0$'

'B8) Esc schliesst, Menue bleibt zu'
[void](Eval 'b_key.cs' @{ KEY = 'Escape' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 300
Check 'B8 geschlossen, Spiel laeuft' (Eval 'b_trade.cs') ' offen=False Seite=- Zeit=1 Menue=False'

'B9) Schmied (Controller: Steuerkreuz unten)'
[void](Eval 'b_goto.cs' @{ X = '3.3'; Y = '-2.2' }); Wait 300
Check 'B9 Schmied in der Naehe' (Eval 'b_trade.cs') '^nah=Schmied offen=False'
[void](Eval 'b_pad.cs' @{ BTN = 'DpadDown' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 200
Check 'B9 Steuerkreuz unten oeffnet den Schmied' (Eval 'b_trade.cs') ' offen=True Seite=Schmied Zeit=0'
[void](Eval 'b_close.cs')
[void](Eval 'b_smith.cs' @{ ACT = 'equip:holzschwert' })
Check 'B9 Liste mit dem Holzschwert' (Eval 'b_smith.cs' @{ ACT = 'open' }) '^Meldung=Liste=1 \| Stufe=0 '
[void](Eval 'b_shop.cs' @{ ACT = 'setgold:20' })   # Stufe +1 kostet 30 Gold
$r = Eval 'b_smith.cs' @{ ACT = 'up:holzschwert' }
Check 'B9 zu wenig Gold' $r '^Meldung=Nicht genug Gold \| Stufe=0 Angriff=\d+ Gold=20 '
$a0 = 0; if ($r -match 'Angriff=(\d+)') { $a0 = [int]$Matches[1] }
[void](Eval 'b_shop.cs' @{ ACT = 'gold:500' })
Check 'B9 Stufe 1: -30 Gold, Angriff +1' (Eval 'b_smith.cs' @{ ACT = 'up:holzschwert' }) "^Meldung=Holzschwert \+1! \| Stufe=1 Angriff=$($a0 + 1) Gold=490 Gel=0 Name=Holzschwert \+1$"
Check 'B9 Stufe 2: -60 Gold' (Eval 'b_smith.cs' @{ ACT = 'up:holzschwert' }) "^Meldung=Holzschwert \+2! \| Stufe=2 Angriff=$($a0 + 2) Gold=430 "
Check 'B9 Stufe 3 braucht Schleimgel' (Eval 'b_smith.cs' @{ ACT = 'up:holzschwert' }) '^Meldung=Nicht genug Schleimgel \| Stufe=2 .* Gold=430 Gel=0 '
[void](Eval 'b_smith.cs' @{ ACT = 'gel:6' })
Check 'B9 Stufe 3: -90 Gold, -6 Schleimgel' (Eval 'b_smith.cs' @{ ACT = 'up:holzschwert' }) "^Meldung=Holzschwert \+3! \| Stufe=3 Angriff=$($a0 + 3) Gold=340 Gel=0 Name=Holzschwert \+3$"
Shot 'm6_schmied'

'B10) Speichern und Laden'
Check 'B10 Datei: Version 3, Gold, Stufen' (Eval 'b_savefile.cs') '^Version=3 Gold=340 Stufen=holzschwert:3 Szene=Base '
Check 'B10 Laden stellt Gold und Stufe her' (Eval 'b_reload.cs') "^geladen=True Gold=340 Stufe=3 Angriff=$($a0 + 3)$"
[void](Eval 'b_close.cs')

'B11) Portal in den Kampfraum'
[void](Eval 'b_goto.cs' @{ X = '4.5'; Y = '0.9' }); Wait 300
Shot 'm6_portal'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Kampf' }); Wait 2500; WaitReady
Check 'B11 Kampfraum, Ankunftspunkt, Gold und Stufe mitgenommen' (Eval 'b_state.cs') "^Szene=Room_Test Pos=\(.*\) Gold=340 Stufe=3 Angriff=$($a0 + 3) .* Reisen=1 reist=False Ankunft=Ankunft$"
'  ' + (Eval 't2_setup.cs')

'B12) Rueckkehr-Portal'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Basis' }); Wait 2500; WaitReady
Check 'B12 zurueck in der Basis am Portal' (Eval 'b_state.cs') '^Szene=Base Pos=\(4\.6,0\.9\) Gold=340 Stufe=3 .* Reisen=2 reist=False Ankunft=Portal$'
'  ' + (Eval 't2_setup.cs')
[void](Eval 'lk_zoom.cs' @{ SIZE = '3.5'; X = '0'; Y = '-1' }); Wait 400
Shot 'm6_basis'

'B13) Tod'
Check 'B13 Tod: -10 % Gold' (Eval 'b_die.cs') '^tot=True Gold 340->306 Verlust=34$'
Wait 600
Shot 'm6_besiegt'
Wait 3200; WaitReady
Check 'B13 zurueck in der Basis, Leben und Mana voll' (Eval 'b_state.cs') '^Szene=Base Pos=\(0\.0,-2\.0\) Gold=306 Stufe=3 Angriff=\d+ HP=(\d+)/\1 Mana=(\d+)/\2 Reisen=3 reist=False Ankunft=Start$'
StopPlay

'B14) Alter Spielstand (Version 2)'
[void](Eval 'b_writev2.cs')
unity command console clear @U 2>$null | Out-Null  # nur Fehler aus diesem Ladevorgang zaehlen
StartPlay -KeepSave -Scene 'Base'
'  ' + (Eval 't2_setup.cs')
Check 'B14 Version 2 laedt (Gold 0, keine Stufen)' (Eval 'b_summary.cs') '^Lv2 Gold=0 Stufen=0 Heiltraenke=3$'
Check 'B14 Speichern in der Basis behaelt Truhen des Kampfraums' (Eval 'b_savefile.cs') '^Version=3 Gold=0 Stufen= Szene=Base Truhen=room_test_truhe_1$'
$err = unity command console get @U -- --tail 50 --level error 2>$null | Out-String
CheckTrue 'B14 keine Fehler beim Laden' ($err -notmatch 'Spielstand konnte nicht|NullReference|Exception') 'Konsole ohne Ladefehler'

StopPlay
TestResult 'Basis-Test' $f0
