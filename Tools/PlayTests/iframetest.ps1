# Schutzzeit-Test: Der Spieler steht ohne Test-Schutz zwischen allen 5 Schleimen.
# Erwartung: hoechstens 6 Treffer in 3 s (der erste zaehlt bei 0 s), mindestens 0,58 s Abstand, Spieler lebt.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Schutzzeit-Test ---'
StartPlay
'  ' + (Eval 't_iframe.cs')
Wait 3500

# Das Spiel schreibt jeden Treffer mit genauer Zeit in die Konsole. Auswertung: letzter Start + Treffer danach.
$log = unity command console get @U -- --tail 200 2>$null | Out-String
$events = @([regex]::Matches($log, 'IFRAME_(START|HIT) t=([0-9.]+) hp=([0-9]+)') | ForEach-Object {
        [pscustomobject]@{ Kind = $_.Groups[1].Value; T = [double]::Parse($_.Groups[2].Value, $Inv); Hp = [int]$_.Groups[3].Value }
    })
$startIdx = -1
for ($k = 0; $k -lt $events.Count; $k++) { if ($events[$k].Kind -eq 'START') { $startIdx = $k } }
if ($startIdx -lt 0) {
    CheckTrue 'Schutzzeit: Start in der Konsole gefunden' $false 'kein IFRAME_START im Log'
}
else {
    $t0 = $events[$startIdx].T
    $hp0 = $events[$startIdx].Hp
    $hits = @($events | Select-Object -Skip ($startIdx + 1) | Where-Object { $_.Kind -eq 'HIT' } | Sort-Object T -Unique)
    $win = @($hits | Where-Object { $_.T -le $t0 + 3.0 })
    '  Treffer-Zeiten (relativ): ' + (($hits | ForEach-Object { ($_.T - $t0).ToString('F3', $Inv) }) -join ', ')
    $minGap = [double]::MaxValue
    for ($k = 1; $k -lt $hits.Count; $k++) { $g = $hits[$k].T - $hits[$k - 1].T; if ($g -lt $minGap) { $minGap = $g } }
    $hp3 = if ($win.Count -gt 0) { $win[-1].Hp } else { $hp0 }
    CheckTrue 'Schutzzeit: mindestens 1 Treffer gemessen' ($hits.Count -ge 1) "Treffer gesamt: $($hits.Count)"
    CheckTrue 'Schutzzeit: hoechstens 6 Treffer in 3 s' ($win.Count -le 6) "Treffer in 3 s: $($win.Count)"
    CheckTrue 'Schutzzeit: Abstand mindestens 0,58 s' ($minGap -ge 0.58) ('kleinster Abstand: ' + $minGap.ToString('F3', $Inv) + ' s')
    CheckTrue 'Schutzzeit: Spieler lebt' ($hp3 -gt 0) "HP nach 3 s: $hp3 (Verlust $($hp0 - $hp3))"
}

StopPlay
TestResult 'Schutzzeit-Test' $f0
