# Aussehen-Test (Meilenstein 4 + 5): sichtbare Ausruestung, Schadenszahlen, Manatrank, Truhe 2, Layout, Bilder,
# Blender-Grafik (A1-A8: Bildanzahl, Treffer/Tod, Schleim laeuft, Schatten, Kacheln, Deko, Schaufenster).
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Aussehen-Test ---'
StartPlay
'  ' + (Eval 't2_setup.cs')

'L1) Ohne Ausruestung'
Check 'L1 alle Ebenen leer, Koerper sichtbar' (Eval 'lk_layers.cs') '^Hose= Schuhe= Brust= Handschuhe= Helm= Nebenhand= Hauptwaffe= \| Koerper=idle_down_\d$'
[void](Eval 'lk_zoom.cs' @{ SIZE = '1.6'; X = '0'; Y = '-1' }); Wait 300
Shot 'm5_figur_ohne'

'L2) An- und Ablegen'
[void](Eval 'lk_equip.cs' @{ IDS = 'lederweste,holzschwert' }); Wait 150
Check 'L2 Weste und Schwert sichtbar' (Eval 'lk_layers.cs') 'Brust=idle_down_\d .*Hauptwaffe=idle_down_\d \| Koerper=idle_down_\d$'
[void](Eval 'lk_unequip_all.cs'); Wait 150
Check 'L2 nach dem Ablegen wieder leer' (Eval 'lk_layers.cs') '^Hose= Schuhe= Brust= Handschuhe= Helm= Nebenhand= Hauptwaffe= '

'L3) Gleiches Bild wie der Koerper'
[void](Eval 'lk_equip.cs' @{ IDS = 'lederhose,lederstiefel,lederweste,lederhandschuhe,lederkappe,holzschild,holzschwert' }); Wait 200
Shot 'm5_figur_leder'
[void](Eval 'lk_watch.cs')
[void](Eval 'lk_walk.cs'); Wait 450
[void](Eval 't2_release.cs'); Wait 300
[void](Eval 'lk_attack.cs'); Wait 120
[void](Eval 't2_release.cs'); Wait 500
[void](Eval 'lk_cast.cs'); Wait 500
Check 'L3 jedes Bild: Ebenen gleich wie der Koerper' (Eval 'lk_watch_check.cs') '^walk=[1-9]\d* attack=[1-9]\d* cast=[1-9]\d* Abweichungen=0$'
'L4) Treffer-Aufblitzen'
Check 'L4 Ebenen blitzen mit' (Eval 'lk_flash.cs') 'rot=True Ebenen=7 gleich=7$'
Wait 400

'L5) Schadenszahl'
Check 'L5 Zahl 7 erscheint' (Eval 'lk_number.cs') '^Zahl=7 sichtbar=[1-9]'

'L6) Manatrank (R)'
[void](Eval 'lk_manapot.cs' @{ MODE = 'leer' }); Wait 300
$r = Eval 'lk_manapot_check.cs'
[void](Eval 't2_release.cs')
Check 'L6 R fuellt 20 Mana auf' $r '^Mana=2\d/\d+ Traenke 2->1$'
Wait 1200
[void](Eval 'lk_manapot.cs' @{ MODE = 'voll' }); Wait 300
$r = Eval 'lk_manapot_check.cs'
[void](Eval 't2_release.cs')
Check 'L6 bei vollem Mana kein Verbrauch' $r 'Traenke 1->1$'

'L7) Truhe 2'
[void](Eval 'lk_unequip_all.cs')
[void](Eval 'lk_zoom.cs' @{ SIZE = '3.5'; X = '0'; Y = '0' })
[void](Eval 'lk_chest2_go.cs'); Wait 700
Check 'L7 Truhe 2 oeffnet (Truhe 1 bleibt zu)' (Eval 'lk_chest2_check.cs') '^Truhe 2 offen=True Truhe 1 offen=False$'
Shot 'm5_truhe2'
for ($k = 0; $k -lt 8; $k++) { if ((Eval 't2_collect.cs') -eq 'keine Beute am Boden') { break }; Wait 450 }
Check 'L7 Inhalt im Inventar' (Eval 't2_inv.cs') '^Inventar \[(?=.*eichenstabx1)(?=.*eisenhelmx1)(?=.*saphirringx1)(?=.*manatrank_kleinx2).*\]$'
Check 'L7 Saphirring: Intelligenz +2, Mana +10' (Eval 'lk_ring.cs') '^angelegt=True Intelligenz 1->3 Mana-Max 25->35$'

'L8) Layout'
[void](Eval 'lk_menu_open.cs' @{ PAGE = '0' }); Wait 300
foreach ($page in 0, 1, 2) {
    Check "L8 Reiter $page ohne Ueberlappung, passt in 16:9 und 4:3" (Eval 'lk_layout.cs' @{ PAGE = "$page" }) "^Seite=$page Ueberlappungen=0 Expand=True 16:9=True 4:3=True$"
}

'L9) Bilder'
[void](Eval 'lk_equip.cs' @{ IDS = 'eichenstab,eisenhelm' })
foreach ($page in @(@('0', 'm5_menue_inventar'), @('1', 'm5_menue_charakter'), @('2', 'm5_menue_magie'))) {
    [void](Eval 'lk_menu_open.cs' @{ PAGE = $page[0] }); Wait 300
    Shot $page[1]
}
[void](Eval 'lk_menu_close.cs'); Wait 300
Shot 'm5_hud'
[void](Eval 'lk_unequip_all.cs')
[void](Eval 'lk_equip.cs' @{ IDS = 'eisenhose,eisenstiefel,eisenbrust,eisenhandschuhe,eisenhelm,eisenschwert' })
[void](Eval 'lk_zoom.cs' @{ SIZE = '1.6'; X = '0'; Y = '-1' }); Wait 300
Shot 'm5_figur_eisen'
[void](Eval 'lk_unequip_all.cs')
[void](Eval 'lk_equip.cs' @{ IDS = 'lederhose,lederweste,eichenstab' }); Wait 300
Shot 'm5_figur_stab'

'A) Blender-Grafik (Meilenstein 5)'
[void](Eval 'lk_unequip_all.cs')
Check 'A1-A3 Animationssets: Bildanzahl, 128/256 px, Fusspunkt' (Eval 'a_assets.cs') '^Sets ok=19/19 Ausruestung=14 Bilder=\d+ Fehler=0$'
Wait 700
Check 'A4 Treffer spielt hurt' (Eval 'a_hurt.cs') '^clip=hurt bild=hurt_\w+_\d HP=\d+$'
Wait 600
Check 'A4 danach wieder idle' (Eval 'a_clip.cs') '^clip=idle bild=idle_\w+_\d$'
[void](Eval 'a_slime_go.cs'); Wait 400
Check 'A5 verfolgender Schleim laeuft' (Eval 'a_slime_check.cs') '^clip=walk bild=walk_\w+_\d$'
Check 'A6 Schatten unter Spieler, Schleimen, Schaufenster' (Eval 'a_shadow.cs') '^Spieler=1 Schleime=5 Schaufenster=3 Fehler=0$'
Check 'A7 Kacheln, Deko, Schaufenster' (Eval 'a_scene.cs') '^Boden=Ground_Grass_0,Ground_Grass_1,Ground_Grass_2,Ground_Grass_3,Ground_Path 64px=True Deko=\d+ ohneBild=0 Schaufenster=Wolf:idle,Boss:idle,Spider:idle$'
[void](Eval 'lk_zoom.cs' @{ SIZE = '3.5'; X = '0'; Y = '0.5' }); Wait 400
Shot 'm5_raum'
[void](Eval 'lk_zoom.cs' @{ SIZE = '2'; X = '1.5'; Y = '2.4' }); Wait 400
Shot 'm5_schaufenster'
[void](Eval 'lk_equip.cs' @{ IDS = 'eisenhose,eisenstiefel,eisenbrust,eisenhandschuhe,eisenhelm,eisenschwert' }); Wait 200
[void](Eval 'lk_zoom.cs' @{ SIZE = '1.6'; X = '0'; Y = '-1' }); Wait 300
Check 'A8 Tod startet death' (Eval 'a_death.cs') '^tot=True clip=death$'
Wait 900
Check 'A8 letztes Sterbe-Bild bleibt stehen' (Eval 'a_clip.cs') '^clip=death bild=death_\w+_3$'
Shot 'm5_figur_tod'

StopPlay
TestResult 'Aussehen-Test' $f0
