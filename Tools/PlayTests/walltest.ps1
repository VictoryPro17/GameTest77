# Mauer-Test: Halten die Mauern den Spieler im Raum?
#   W1 Collider hat Formen, und an den vier Mauern gibt es einen Treffer
#   W2 gegen jede der vier Mauern laufen
#   W3 in die linke Mauer ausweichen
#   W4 ein Schleim drueckt den Spieler gegen die linke Mauer
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Mauer-Test ---'
StartPlay
'  ' + (Eval 't2_setup.cs')

Check 'W1 Mauer-Collider' (Eval 'w_collider.cs') '^Formen=[1-9]\d* Mauertreffer=4/4 Raummitte frei=True$'

# Der Spieler-Collider ist ein Kreis mit Radius 0,2, der 0,12 ueber den Fuessen sitzt.
# Deshalb bleibt er links/rechts bei x = +-8,8 stehen, unten bei y = -4,92 und oben bei y = 4,68.
$walls = @(
    @{ n = 'links';  px = '-7.8'; py = '2';    sx = '-1'; sy = '0';  key = 'x'; min = -8.9; max = -8.7 },
    @{ n = 'rechts'; px = '7.8';  py = '2';    sx = '1';  sy = '0';  key = 'x'; min = 8.7;  max = 8.9 },
    @{ n = 'unten';  px = '2';    py = '-3.8'; sx = '0';  sy = '-1'; key = 'y'; min = -5.0; max = -4.85 },
    @{ n = 'oben';   px = '2';    py = '3.6';  sx = '0';  sy = '1';  key = 'y'; min = 4.6;  max = 4.75 }
)
foreach ($w in $walls) {
    [void](Eval 'w_walk.cs' @{ PX = $w.px; PY = $w.py; SX = $w.sx; SY = $w.sy })
    Wait 1200
    CheckRange "W2 gegen die Mauer $($w.n) laufen" (Eval 'w_pos.cs') $w.key $w.min $w.max
    Wait 200
}

[void](Eval 'w_dodge.cs')
Wait 600
CheckRange 'W3 in die linke Mauer ausweichen' (Eval 'w_pos.cs') 'x' -8.9 -8.7
Wait 700

'  ' + (Eval 'w_slime.cs')
Wait 2500
$r = Eval 'w_slime_check.cs'
CheckRange 'W4 Schleim drueckt gegen die linke Mauer' $r 'x' -8.9 8.9
Check 'W4 Schleim hat wirklich getroffen' $r 'Treffer=[2-9]'

StopPlay
TestResult 'Mauer-Test' $f0
