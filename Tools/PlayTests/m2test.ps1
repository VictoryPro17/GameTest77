# Meilenstein-2-Test: Truhe, Ausruestung, Verteidigung, Trank, Attribute, Inventar, Speichern, Menue, Loeschen.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Press([string]$file) { [void](Eval $file); Wait 300; [void](Eval 't2_release.cs'); Wait 300 }

'--- Meilenstein-2-Test ---'
StartPlay
'  ' + (Eval 't2_setup.cs')

'G1) Truhe'
[void](Eval 't2_g1_go.cs'); Wait 700
Check 'G1 Truhe oeffnet sich' (Eval 't2_g1_check.cs') 'Truhe offen=True'
Shot 'm2_truhe_offen'
for ($k = 0; $k -lt 8; $k++) { if ((Eval 't2_collect.cs') -eq 'keine Beute am Boden') { break }; Wait 450 }
Check 'G1 Beute im Inventar' (Eval 't2_inv.cs') '^Inventar \[(?=.*heiltrank_kleinx2)(?=.*holzschildx1)(?=.*holzschwertx1)(?=.*lederwestex1).*\]$'

'G2) Ausruestung'
Check 'G2 Schwert und Weste' (Eval 't2_g2.cs') 'Schwert angelegt=True Angriff 10->13 \| Weste angelegt=True Leben 50->55 Verteidigung 0->2 \| Hauptwaffe=holzschwert Brust=lederweste Schwert noch im Inventar=False'

'G3) Verteidigung'
[void](Eval 't2_g3_go.cs'); Wait 1500
Check 'G3 Schaden bei Verteidigung 7, Spieler im Raum' (Eval 't2_g3_check.cs') 'Schaden pro Schleimtreffer=4 .*imRaum=True'

'G4) Trank'
[void](Eval 't2_g4a.cs'); Wait 300
Check 'G4 Q heilt 20' (Eval 't2_g4b.cs') '^HP 40 Traenke 2->1$'
Wait 1200
[void](Eval 't2_g4c.cs'); Wait 300
Check 'G4 kein Trank bei vollem Leben' (Eval 't2_g4d.cs') 'Traenke 1->1$'

'G5) Attribute'
Check 'G5 Level und Vitalitaet' (Eval 't2_g5.cs') 'Level 1->2 freie Punkte 3 \| Vitalitaet\+1=True max\. Leben 65->70 Punkte jetzt 2'
Wait 300
Shot 'm2_hud'

'G6) Inventar'
Check 'G6 Stapel 99' (Eval 't2_g6a.cs') 'Rest=0 Platz0=99 Platz1=31 Platz2leer=True'
[void](Eval 't2_g6b.cs'); Wait 800
Check 'G6 volles Inventar' (Eval 't2_g6c.cs') 'Kupferring liegt noch=True im Inventar=0 Eisenschwerter=24'
Check 'G6 Sortieren' (Eval 't2_g6d.cs') 'Sortiert: Kupferring, Holzschwert, Lederhose, Heiltrank, Kleiner Heiltrank, Schleimgel, Platz6leer=True'

'G7) Speichern'
Check 'G7 gespeichert' (Eval 't2_g7a.cs') 'gespeichert=True'
$before = Eval 't2_summary.cs'
StopPlay
StartPlay -KeepSave
'  ' + (Eval 't2_setup.cs')
$after = Eval 't2_summary.cs'
CheckTrue 'G7 nach Neustart gleich' ($before -eq $after -and $before -notmatch '^ROH') "vorher: $before | nachher: $after"

'G8) Menue'
'  ' + (Eval 't2_g8_prep.cs')
Press 't2_key_tab.cs'
Check 'G8.1 Tab oeffnet' (Eval 't2_menu.cs') 'Menue offen=True timeScale=0 Auswahl=Slot_00'
Press 't2_key_tab.cs'
Check 'G8.2 Tab schliesst' (Eval 't2_menu.cs') 'Menue offen=False timeScale=1'
Press 't2_pad_start.cs'
Check 'G8.3 Start oeffnet' (Eval 't2_menu.cs') 'Menue offen=True timeScale=0 Auswahl=Slot_00'
Press 't2_pad_right.cs'
Check 'G8.4 Steuerkreuz rechts' (Eval 't2_menu.cs') 'Auswahl=Slot_01$'
Press 't2_pad_south.cs'
$r = Eval 't2_g8_check.cs'
Check 'G8.5 Sued-Taste legt an' $r 'Hauptwaffe=eisenschwert Angriff=16 Platz1=leer'
if ($r -notmatch 'Hauptwaffe=eisenschwert') { '  ' + (Eval 't2_g8_diag.cs') }
[void](Eval 't2_sel_sort.cs')
Press 't2_pad_south.cs'
Check 'G8.6 Sortieren per Sued-Taste' (Eval 't2_g8_sorted.cs') 'Sortiert: kupferring,lederkappe,heiltrank_klein,schleimgel,-'
Check 'G8.7 Filter' (Eval 't2_g8_filter.cs') 'Filter1=2 Filter2=1 Filter3=1'
[void](Eval 't2_sel_slot.cs' @{ N = '1' })
Check 'G8.8 Knopf im Detail' (Eval 't2_sel_action.cs') 'Knopf=Anlegen'
Press 't2_pad_south.cs'
Check 'G8.8 Anlegen ueber Knopf, Auswahl zurueck' (Eval 't2_g8_equipcheck.cs') 'Helm=lederkappe Auswahl=Slot_01 aktiv=True'
[void](Eval 't2_sel_slot.cs' @{ N = '3' })
Press 't2_key_tab.cs'
Press 't2_key_tab.cs'
Check 'G8.9 nur ein Auswahlrahmen' (Eval 't2_g8_frames.cs') '^Rahmen=1 \(Slot_00\)$'
Check 'G8.10 Layout' (Eval 't2_g8_layout.cs') 'Ueberlappungen=0 DetailImPanel=True .*passt=True'
Check 'G8.11 Vergleich' (Eval 't2_g8_compare.cs') 'Angriff \+3 .*\(-3\).*Vergleich mit: Eisenschwert'
Wait 300
Shot 'm2_menue_inventar'
Check 'G8.12 Charakter-Reiter' (Eval 't2_g8_char.cs') 'Seite=1 frei=3 Auswahl=Plus_0'
Press 't2_pad_south.cs'
Check 'G8.12 Staerke +1' (Eval 't2_g8_charcheck.cs') 'Staerke=2 Angriff=17 frei=2'
Shot 'm2_menue_charakter'

'G9) Spielstand loeschen ueber das Menue'
[void](Eval 't2_sel_delete.cs')
Press 't2_pad_south.cs'
Check 'G9 Rueckfrage' (Eval 't2_g9_confirm.cs') 'Rueckfrage offen=True Auswahl=No'
[void](Eval 't2_sel_yes.cs')
Press 't2_pad_south.cs'
Wait 1500
WaitReady
Check 'G9 geloescht und frei' (Eval 't2_g9_state.cs') 'Datei vorhanden=False Menue offen=False timeScale=1 MoveAktiv=True'
Check 'G9 Startwerte' (Eval 't2_summary.cs') '^Lv1 XP0 Attr\[1,1,1,1,1\] frei0 HP50/50 \| Inv \| Ausr \| Truhe offen=False$'

'G10) Bilder'
'  ' + (Eval 't2_setup.cs')
[void](Eval 't2_chestshot.cs'); Wait 500
Shot 'm2_truhe'

StopPlay
TestResult 'Meilenstein-2-Test' $f0
