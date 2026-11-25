# Gemeinsame Funktionen fuer die Play-Tests von RE:BORN.
# Jedes Testskript laedt diese Datei mit:  . "$PSScriptRoot\lib.ps1"
# Die Tests steuern den offenen Unity-Editor ueber die Unity CLI ("unity command ...").
# Nur ASCII in den Skripten, damit Windows PowerShell 5.1 sie ohne BOM richtig liest.

$CsDir       = Join-Path $PSScriptRoot 'cs'
$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$ShotDir     = Join-Path $ProjectPath 'Screenshots'
$TmpDir      = Join-Path $env:TEMP 'reborn_playtests'
$U           = '--caller', 'plugin', '--skill', 'unity-cli', '--project-path', $ProjectPath, '--no-banner'
$Inv         = [System.Globalization.CultureInfo]::InvariantCulture
$Utf8NoBom   = New-Object System.Text.UTF8Encoding($false)
$HeaderText  = [System.IO.File]::ReadAllText((Join-Path $CsDir '_header.cs'), [System.Text.Encoding]::UTF8)

if ($null -eq $global:RebornChecks) { $global:RebornChecks = 0 }
if ($null -eq $global:RebornFails)  { $global:RebornFails = 0 }
New-Item -ItemType Directory -Force $TmpDir | Out-Null
New-Item -ItemType Directory -Force $ShotDir | Out-Null

# Fuehrt ein C#-Schnipsel aus cs/ im Editor aus und gibt sein Ergebnis (Text) zurueck.
#   //#HEADER  im Schnipsel wird durch cs/_header.cs ersetzt (Spieler, Inventar, Geraete ...).
#   %NAME%     wird durch den Wert aus $vars ersetzt, z. B. Eval 'w_walk.cs' @{ PX = '-7.8' }.
function Eval([string]$file, [hashtable]$vars) {
    $src = [System.IO.File]::ReadAllText((Join-Path $CsDir $file), [System.Text.Encoding]::UTF8)
    if ($src.Contains('//#HEADER')) { $src = $src.Replace('//#HEADER', $HeaderText) }
    # GetEnumerator statt .Keys: bei einem Eintrag namens "Keys" liefert .Keys sonst dessen Wert
    if ($vars) { foreach ($kv in $vars.GetEnumerator()) { $src = $src.Replace("%$($kv.Key)%", [string]$kv.Value) } }
    $tmp = Join-Path $TmpDir $file
    [System.IO.File]::WriteAllText($tmp, $src, $Utf8NoBom)
    $raw = unity command script eval @U --result-only --file $tmp 2>$null | Out-String
    $res = $null
    try { $res = ($raw | ConvertFrom-Json).result } catch {}
    if ($null -ne $res -and "$res" -ne '') { return "$res" }
    $flat = ($raw -replace '\s+', ' ').Trim()
    return 'ROH: ' + $flat.Substring(0, [Math]::Min(500, $flat.Length))
}

function Wait([int]$ms) { Start-Sleep -Milliseconds $ms }

function WaitReady { unity status --project-path $ProjectPath --until-ready --no-banner 2>$null | Out-Null }

# Beendet den Play-Modus (das Spiel speichert dabei) und wartet, bis der Editor wieder bereit ist.
function StopPlay {
    unity command editor.playmode stop @U 2>$null | Out-Null
    Wait 800
    WaitReady
}

# Startet den Play-Modus. Ohne -KeepSave wird vorher der Spielstand geloescht (frischer Start).
# -Scene: welche Szene gespielt wird (Meilenstein 6: Standard Room_Test, der Basis-Test nimmt Base).
# Sorgt dafuer, dass das Spiel auch ohne Fokus weiterlaeuft, und prueft, dass wirklich Frames laufen.
function StartPlay([switch]$KeepSave, [string]$Scene = 'Room_Test') {
    StopPlay
    [void](Eval 't_openscene.cs' @{ SCENE = $Scene })
    # Das Projekt laedt beim Play-Start den Code nicht neu: alte Test-Geraete nehmen dann keine Eingaben mehr an.
    # Deshalb vor jedem Start entfernen; die Setup-Schnipsel legen sie frisch an.
    [void](Eval 't2_cleanup.cs')
    if (-not $KeepSave) { [void](Eval 't2_delsave.cs') }
    unity command editor set_auto_tick @U 2>$null | Out-Null
    unity command editor.playmode set_run_in_background @U 2>$null | Out-Null
    unity command editor.playmode start @U 2>$null | Out-Null
    Wait 1200
    WaitReady
    $f1 = Eval 't_frame.cs'
    Wait 400
    $f2 = Eval 't_frame.cs'
    if (-not ($f1 -match '^\d+$' -and $f2 -match '^\d+$' -and [int]$f2 -gt [int]$f1)) {
        throw "Der Play-Modus laeuft nicht (Frames: '$f1' -> '$f2'). Ist der Bildschirm gesperrt?"
    }
    # Meilenstein 10a: Standard-Einstellungen nur fuer diese Sitzung (deine Sprache und Tasten bleiben unberuehrt)
    [void](Eval 't_settings_default.cs')
}

# Screenshot der Game-Ansicht nach Screenshots/<name>.png (Seitenverhaeltnis wie die Game-Ansicht).
function Shot([string]$name) {
    if (-not $global:RebornShotSize) {
        $w = 1280; $h = 720
        $gv = Eval 't_gameview.cs'
        if ($gv -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -gt 0 -and [int]$Matches[2] -gt 0) {
            $h = [int][Math]::Round(1280.0 * [int]$Matches[2] / [int]$Matches[1] / 2) * 2
            if ($h -lt 360) { $h = 360 }
            if ($h -gt 1280) { $h = 1280 }
        }
        $global:RebornShotSize = @($w, $h)
    }
    $out = (Join-Path $ShotDir "$name.png") -replace '\\', '/'
    unity command capture game_view @U -- --source camera --width $global:RebornShotSize[0] --height $global:RebornShotSize[1] --output-path $out 2>$null | Out-Null
    "  Screenshot: $name.png"
}

# Pruefung: Text muss zum Muster (Regex, Gross-/Kleinschreibung zaehlt) passen.
function Check([string]$label, [string]$text, [string]$pattern) {
    $global:RebornChecks++
    if ($text -cmatch $pattern) { "  OK      $label | $text" }
    else { $global:RebornFails++; "  FEHLER  $label | $text | erwartet: $pattern" }
}

# Pruefung: Zahl hinter "<key>=" im Text muss zwischen $min und $max liegen (Dezimalpunkt).
function CheckRange([string]$label, [string]$text, [string]$key, [double]$min, [double]$max) {
    $global:RebornChecks++
    $v = $null
    if ($text -cmatch "(?:^|\s)$key=(-?[0-9]+(?:\.[0-9]+)?)") { $v = [double]::Parse($Matches[1], $Inv) }
    if ($null -ne $v -and $v -ge $min -and $v -le $max) { "  OK      $label | $text | erlaubt: $key von $min bis $max" }
    else { $global:RebornFails++; "  FEHLER  $label | $text | erwartet: $key von $min bis $max" }
}

# Pruefung: Bedingung muss wahr sein.
function CheckTrue([string]$label, [bool]$ok, [string]$text) {
    $global:RebornChecks++
    if ($ok) { "  OK      $label | $text" }
    else { $global:RebornFails++; "  FEHLER  $label | $text" }
}

# Die Tests loeschen Spielstaende. Deshalb laufen sie nur ueber run_all.ps1, das den Spielstand sichert.
function RequireSession {
    if (-not $global:RebornTestSession) { throw 'Bitte ueber run_all.ps1 starten. Das schuetzt deinen Spielstand.' }
}

# Ergebniszeile fuer ein Testskript ($failsBefore = Fehlerzaehler am Anfang des Skripts).
function TestResult([string]$name, [int]$failsBefore) {
    $n = $global:RebornFails - $failsBefore
    if ($n -eq 0) { "ERGEBNIS ${name}: BESTANDEN" } else { "ERGEBNIS ${name}: NICHT BESTANDEN ($n Fehler)" }
}
