# Raetsel aus Review 3: Im Mauer-Test ohne Mauern stand der Spieler bei (0, 0) mit 0 HP.
# Dieser Test spielt die alte Reihenfolge mit abgeschalteten Mauern nach und protokolliert,
# woher Schaden, Tod und Positionsspruenge kommen. Start ueber run_all.ps1 -Tests hp0.
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- 0-HP-Test (Mauern nur zur Laufzeit aus) ---'
StartPlay
'  ' + (Eval 't2_setup.cs')
'  ' + (Eval 'z_setup.cs')
$walls = @(
    @{ px = '-7.8'; py = '2';    sx = '-1'; sy = '0' },
    @{ px = '7.8';  py = '2';    sx = '1';  sy = '0' },
    @{ px = '2';    py = '-3.8'; sx = '0';  sy = '-1' },
    @{ px = '2';    py = '3.6';  sx = '0';  sy = '1' }
)
foreach ($w in $walls) {
    [void](Eval 'w_walk.cs' @{ PX = $w.px; PY = $w.py; SX = $w.sx; SY = $w.sy })
    Wait 1200
    '  nach Lauf: ' + (Eval 'w_pos.cs') + ' | ' + (Eval 'z_hp.cs')
    Wait 200
}
[void](Eval 'w_dodge.cs'); Wait 600
'  nach Ausweichen: ' + (Eval 'w_pos.cs') + ' | ' + (Eval 'z_hp.cs')

$log = unity command console get @U -- --tail 300 2>$null | Out-String
$lines = @([regex]::Matches($log, 'ZHP_[A-Z]+ [^"]{0,600}') | ForEach-Object { $_.Value -replace '\\n', ' ' })
'  Protokoll (' + $lines.Count + ' Eintraege):'
$lines | ForEach-Object { '    ' + $_ }
$died = @($lines | Where-Object { $_ -like 'ZHP_DIED*' }).Count
$dmg = @($lines | Where-Object { $_ -like 'ZHP_DMG*' }).Count
CheckTrue 'Spieler ohne Mauern: kein Schaden, kein Tod' ($died -eq 0 -and $dmg -eq 0) "Schaden=$dmg Tod=$died"

StopPlay
TestResult '0-HP-Test' $f0
