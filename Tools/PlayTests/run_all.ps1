# Startet die Play-Tests von RE:BORN und schuetzt dabei deinen Spielstand.
#
#   powershell -File Tools\PlayTests\run_all.ps1                 alle Tests
#   powershell -File Tools\PlayTests\run_all.ps1 -Tests wall,kb  nur einzelne Tests
#
# Tests: wall (Mauern), m2 (Inventar, Ausruestung, Menue, Speichern), play (Laufen, Kampf, Beute),
#        iframe (Schutzzeit nach Treffer), kb (Tastatur)
#
# Ablauf: Spielstand sichern -> Tests -> aufraeumen -> Spielstand zurueckspielen und Pruefsumme vergleichen.
# Das Zurueckspielen passiert auch dann, wenn ein Test abbricht.
param([string[]]$Tests = @('look', 'magic', 'wall', 'm2', 'play', 'iframe', 'kb', 'hp0'))
# "-Tests a,b" kommt bei "powershell -File" als ein Text an: hier wieder aufteilen
$Tests = @($Tests | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })

. "$PSScriptRoot\lib.ps1"
$global:RebornChecks = 0
$global:RebornFails = 0
$global:RebornShotSize = $null
$global:RebornTestSession = $false
$log = Join-Path $TmpDir 'last_run.log'
$scripts = @{ wall = 'walltest.ps1'; m2 = 'm2test.ps1'; play = 'playtest.ps1'; iframe = 'iframetest.ps1'; kb = 'kbtest.ps1'; hp0 = 'hp0test.ps1'; magic = 'magictest.ps1'; look = 'looktest.ps1'; diag = 'inputdiag.ps1' }

& {
    "=== RE:BORN Play-Tests: $($Tests -join ', ') ==="
    foreach ($t in $Tests) { if (-not $scripts.ContainsKey($t)) { throw "Unbekannter Test: $t" } }

    # 1. Editor erreichbar und nicht im Play-Modus? (Ein laufendes Spiel gehoert dir, das stoppen wir nicht.)
    $statusRaw = unity command editor status @U --format json 2>$null | Out-String
    $playMode = $null
    try { $playMode = ($statusRaw | ConvertFrom-Json).data.result.playMode } catch {}
    if (-not $playMode) { throw 'Unity-Editor nicht erreichbar. Ist das Projekt in Unity geoeffnet?' }
    if ($playMode -ne 'stopped') { throw 'Unity ist gerade im Play-Modus. Bitte erst den Play-Modus beenden, dann die Tests neu starten.' }

    # 1b. Game-Ansicht sichtbar? Sonst gehen simulierte Eingaben verloren und die Tests melden falsche Fehler.
    $gv = Eval 't_gameview_visible.cs'
    # Nur Hinweis: Die Pruefung hat sich als unzuverlaessig erwiesen (meldet manchmal 'unsichtbar', obwohl der Reiter vorne ist)
    if ($gv -ne 'sichtbar') { "Hinweis: Game-Ansicht laut Unity '$gv'. Falls Eingabe-Tests scheitern: Unity-Fenster wiederherstellen und Reiter 'Game' zeigen." }

    # 2. Spielstand sichern
    $savePath = Eval 't2_savepath.cs'
    if ($savePath -notmatch 'reborn_save\.json$') { throw "Der Spielstand-Pfad konnte nicht abgefragt werden: $savePath" }
    $backup = Join-Path (Split-Path $savePath) 'reborn_save.user_backup.json'
    if (Test-Path $backup) { throw "Alte Sicherung gefunden: $backup. Bitte erst pruefen (zurueckkopieren oder loeschen), dann neu starten." }
    $hadSave = Test-Path $savePath
    $userHash = $null
    if ($hadSave) {
        Copy-Item $savePath $backup -Force
        $userHash = (Get-FileHash $savePath -Algorithm SHA256).Hash
        if ((Get-FileHash $backup -Algorithm SHA256).Hash -ne $userHash) { throw 'Die Sicherung des Spielstands ist fehlgeschlagen.' }
        "Spielstand gesichert (SHA-256 $($userHash.Substring(0, 12))...)"
    }
    else { 'Kein Spielstand vorhanden, nichts zu sichern.' }

    # Alte Meldungen (z. B. vom eigenen Spielen) sollen keinen Test beeinflussen
    unity command console clear @U 2>$null | Out-Null
    $global:RebornTestSession = $true
    $total = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        foreach ($t in $Tests) {
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            ''
            & (Join-Path $PSScriptRoot $scripts[$t])
            "  (Dauer: $([int]$sw.Elapsed.TotalSeconds) s)"
        }
    }
    finally {
        $global:RebornTestSession = $false
        ''
        '=== Aufraeumen ==='
        StopPlay
        'Test-Geraete: ' + (Eval 't2_cleanup.cs')
        'Eingabe-Einstellungen: ' + (Eval 't_reset.cs')
        unity command editor.playmode set_run_in_background @U -- --enabled false 2>$null | Out-Null
        unity command editor set_auto_tick @U -- --enabled false 2>$null | Out-Null
        if ($hadSave) {
            Copy-Item $backup $savePath -Force
            if ((Get-FileHash $savePath -Algorithm SHA256).Hash -eq $userHash) {
                Remove-Item $backup -Force
                'Dein Spielstand ist zurueck (Pruefsumme gleich).'
            }
            else {
                $global:RebornFails++
                "FEHLER: Die Pruefsumme des Spielstands stimmt nicht! Die Sicherung bleibt liegen: $backup"
            }
        }
        elseif (Test-Path $savePath) {
            Remove-Item $savePath -Force
            'Vorher gab es keinen Spielstand. Der Test-Spielstand ist entfernt.'
        }
        ''
        "=== GESAMT: $($global:RebornChecks) Pruefungen, $($global:RebornFails) Fehler, $([int]$total.Elapsed.TotalSeconds) s -> " + $(if ($global:RebornFails -eq 0) { 'BESTANDEN' } else { 'NICHT BESTANDEN' }) + ' ==='
    }
} 2>&1 | Tee-Object -FilePath $log
