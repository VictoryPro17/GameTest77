# Einfuehrungs-Test (Meilenstein 10b): Mentor Aldric, 7 Schritte (Laufen, Angreifen, Ausweichen, Zauber, Menue, Haendler,
# Weg zur Hoehle), Hinweiszeile und Zielpfeil, Uebungs-Erlaubnis in der Stadt, Belohnung (50 Gold + 3 Heiltraenke) nur einmal,
# Spielstand-Schalter "tutorial_done", Ueberspringen, Mentor folgt nach Teleport, Soundeffekte (Sfx, SoundBank, Klick, Stumm).
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails
function Num([string]$text, [string]$key) { if ($text -match "(?:^|\s)$key=(-?\d+)") { return [int]$Matches[1] }; return -999 }
function NumF([string]$text, [string]$key) { if ($text -match "(?:^|\s)$key=(-?[0-9]+(?:\.[0-9]+)?)") { return [double]::Parse($Matches[1], $Inv) }; return -999.0 }
function TuState { return (Eval 'tu_state.cs') }
function TuDo([string]$act) { return (Eval 'tu_do.cs' @{ ACT = $act }) }
function TuSfx([string]$act) { return (Eval 'tu_sfx.cs' @{ ACT = $act }) }

'--- Einfuehrungs-Test ---'
'T1) Stadt ohne Anfrage: keine Einfuehrung'
StartPlay -Scene Base
'  ' + (Eval 't2_setup.cs')
Wait 1000
$r = TuState
Check 'T1 keine Einfuehrung aktiv' $r '^Schritt=-1 Aktiv=False Fertig=False Panel=False Pfeil=False Ziel=- Training=False Frieden=True '
CheckTrue 'T1 Mentor steht in der Stadt' ((NumF $r 'Mentor') -ge 0) $r

'T2) Einfuehrung anfordern, Schritt 0 (Laufen)'
[void](TuDo 'request')
Check 'T2 Schritt 0, Hinweiszeile da' (TuDo 'begin:0') '^Schritt=0 Aktiv=True Fertig=False Panel=True .*Hinweis=Laufe'
Shot 'm10b_tutorial_laufen'
$s0 = TuState
$gold0 = Num $s0 'Gold'
$tr0 = Num $s0 'Traenke'
'  Gold vorher: ' + $gold0 + ' Traenke vorher: ' + $tr0

'T3) Laufen -> Schritt 1 (Angreifen, Uebung erlaubt)'
for ($i = 0; $i -lt 5; $i++) {
    $r = TuDo 'walk'
    Wait 150
    $r = TuState
    if ($r -match '^Schritt=1 ') { break }
}
Check 'T3 Schritt 1, Uebung erlaubt, nicht mehr friedlich' $r '^Schritt=1 Aktiv=True .*Training=True Frieden=False .*Hinweis=Greife an'

'T4) Weiter bis zum Haendler'
# echte Tasten ueber die virtuelle Tastatur: 2x J (Angriff), dann Leertaste (Ausweichen)
'  ' + (Eval 't_kb_setup.cs')
for ($i = 0; $i -lt 2; $i++) { [void](Eval 't_kb_j.cs'); Wait 100; [void](Eval 't_kb_release.cs'); Wait 500 }
Check 'T4 zwei echte Angriffe (J) -> Schritt 2 Ausweichen, Training aus' (TuState) '^Schritt=2 .*Training=False Frieden=True .*Hinweis=Weiche aus'
[void](Eval 't_kb_space.cs'); Wait 150; [void](Eval 't_kb_release.cs'); Wait 300
Check 'T4 echtes Ausweichen (Leertaste) -> Schritt 3 Zauber, Training an' (TuState) '^Schritt=3 .*Training=True Frieden=False .*Hinweis=Wirke einen Zauber'
[void](Eval 't_kb_remove.cs')
$r = TuDo 'cast'
Check 'T4 Zauber in der Stadt gewirkt' $r 'Gezaubert=True'
Wait 300
Check 'T4 Schritt 4 Menue, Training wieder aus' (TuState) '^Schritt=4 .*Training=False Frieden=True .*Hinweis=.*Men'
[void](TuDo 'menuopen')
Wait 300
Check 'T4 Menue offen: noch Schritt 4' (TuState) '^Schritt=4 '
[void](TuDo 'menuclose')
Wait 300
[void](TuDo 'start')
Wait 200
$r = TuState
Check 'T4 Schritt 5 Haendler, Pfeil zeigt auf Haendler' $r '^Schritt=5 .*Pfeil=True Ziel=Npc_haendler '
Shot 'm10b_tutorial_pfeil'

'T5) Haendler, Hoehle, Belohnung'
[void](TuDo 'trade')
Wait 300
Check 'T5 Schritt 6, Pfeil zur Hoehle' (TuState) '^Schritt=6 .*Ziel=Tuer_Hoehle '
[void](TuDo 'tradeclose')
Wait 200
[void](TuDo 'tocave')
Wait 400
$r = TuState
Check 'T5 fertig, nicht mehr aktiv' $r '^Schritt=-1 Aktiv=False Fertig=True Panel=False Pfeil=False Ziel=- Training=False Frieden=True '
CheckTrue 'T5 50 Gold Belohnung' ((Num $r 'Gold') -eq ($gold0 + 50)) $r
CheckTrue 'T5 3 Heiltraenke Belohnung' ((Num $r 'Traenke') -eq ($tr0 + 3)) $r
Check 'T5 einmal belohnt, nicht uebersprungen' $r 'Belohnt=1 Ueberspr=False '
Shot 'm10b_tutorial_fertig'

'T6) Zweiter Start bleibt aus'
$r = TuDo 'begin:0'
Check 'T6 bleibt inaktiv' $r '^Schritt=-1 Aktiv=False Fertig=True Panel=False '
Check 'T6 keine zweite Belohnung' $r 'Belohnt=1 '
CheckTrue 'T6 Gold unveraendert' ((Num $r 'Gold') -eq ($gold0 + 50)) $r

'T7) Schalter im Spielstand'
Check 'T7 tutorial_done gespeichert' (Eval 'tu_savefile.cs') 'Flag=True'

'T8) Ueberspringen'
StopPlay
StartPlay -Scene Base
'  ' + (Eval 't2_setup.cs')
Wait 1000
[void](TuDo 'request')
[void](TuDo 'resetflag')
$s0 = TuState
$gold1 = Num $s0 'Gold'
Check 'T8 Schalter weg: Einfuehrung nicht fertig' $s0 'Fertig=False '
Check 'T8 Schritt 2 aktiv' (TuDo 'begin:2') '^Schritt=2 Aktiv=True '
$r = TuDo 'clickskip'
Check 'T8 uebersprungen, nicht mehr aktiv' $r '^Schritt=-1 Aktiv=False Fertig=True Panel=False Pfeil=False Ziel=- Training=False Frieden=True '
Check 'T8 Ueberspr True, einmal belohnt' $r 'Belohnt=1 Ueberspr=True '
CheckTrue 'T8 Belohnung auch beim Ueberspringen' ((Num $r 'Gold') -eq ($gold1 + 50)) $r

'T9) Mentor folgt nach Teleport'
[void](TuDo 'resetflag')
Check 'T9 Einfuehrung laeuft wieder' (TuDo 'begin:0') '^Schritt=0 Aktiv=True '
$r = TuDo 'far'
CheckTrue 'T9 Spieler weit weg vom Mentor' ((NumF $r 'Mentor') -ge 10) $r
Wait 300
CheckRange 'T9 Mentor ist nachgerueckt' (TuState) 'Mentor' 0 3
[void](TuDo 'skip')
Wait 300

'T10) Soundeffekte'
$a = TuSfx 'state'
'  ' + $a
Check 'T10 Klangbank hat Klaenge, 10 Quellen' $a 'Quellen=10 '
$g0 = Num $a 'Gespielt'
$m0 = Num $a 'Stumm'
$k0 = Num $a 'Klicks'
$b = TuSfx 'play:Hit'
CheckTrue 'T10 Treffer-Ton gespielt (+1)' ((Num $b 'Gespielt') -eq ($g0 + 1)) $b
Check 'T10 Letzter Ton Hit, Lautstaerke 0.80' $b 'Letzt=Hit Lautst=0\.80 '
CheckTrue 'T10 Klangbank mindestens 1' ((Num $b 'Bank') -ge 1) $b
[void](TuSfx 'mute')
$c = TuSfx 'play:Coin'
CheckTrue 'T10 stumm: Stumm +1' ((Num $c 'Stumm') -eq ($m0 + 1)) $c
CheckTrue 'T10 stumm: nichts gespielt' ((Num $c 'Gespielt') -eq ($g0 + 1)) $c
[void](TuSfx 'unmute')
Wait 100
$d = TuSfx 'click'
CheckTrue 'T10 Klick-Ton (+1)' ((Num $d 'Klicks') -eq ($k0 + 1)) $d
Check 'T10 10 Quellen, Gesamt-Lautstaerke' $d 'Quellen=10 Listener=0\.80 '

StopPlay
TestResult 'Einfuehrungs-Test' $f0
