# RE:BORN - alle Blender-Grafiken neu rendern (Meilenstein 5)
# Reihenfolge: Figur -> Ausruestung (braucht Reborn_Characters.blend der Figur) -> Gegner -> Boss
#              -> Gegenstaende -> Bodenkacheln (braucht Reborn_World.blend der Gegenstaende) -> Zauber
# Aufruf:  powershell -File Art/Blender/Scripts/build_all.ps1 [-Blender <pfad>] [-Only player,boss,...]
# Danach in Unity: Tools > RE:BORN > Build (bzw. RebornBuilder per CLI), damit Importeinstellungen stimmen.
param(
    [string]$Blender = "C:\Program Files\Blender Foundation\Blender 5.2\blender.exe",
    [string[]]$Only = @()
)
$ErrorActionPreference = "Stop"
$Only = @($Only | ForEach-Object { $_ -split ',' } | Where-Object { $_ })
$scripts = $PSScriptRoot
$project = (Resolve-Path (Join-Path $scripts "..\..\..")).Path
$art = Join-Path $project "Art\Blender"

$steps = @(
    @{ Name = "player";    Script = "build_player.py";    Blend = $null },
    @{ Name = "equipment"; Script = "build_equipment.py"; Blend = "Reborn_Characters.blend" },
    @{ Name = "enemies";   Script = "build_enemies.py";   Blend = $null },
    @{ Name = "boss";      Script = "build_boss.py";      Blend = $null },
    @{ Name = "props";     Script = "build_props.py";     Blend = $null },
    @{ Name = "tiles";     Script = "build_tiles.py";     Blend = "Reborn_World.blend" },
    @{ Name = "spells";    Script = "build_spells.py";    Blend = $null }
)

$failed = @()
foreach ($s in $steps) {
    if ($Only.Count -gt 0 -and $Only -notcontains $s.Name) { continue }
    $start = Get-Date
    Write-Host "== $($s.Name) ($($s.Script))"
    $bargs = @("-b")
    if ($s.Blend) { $bargs += (Join-Path $art $s.Blend) } else { $bargs += "--factory-startup" }
    $bargs += @("--python", (Join-Path $scripts $s.Script), "--", $project)
    $out = & $Blender @bargs 2>&1 | ForEach-Object { "$_" }
    $result = $out | Where-Object { $_ -match '^RESULT' } | Select-Object -Last 1
    # Nur echte Abbrueche zaehlen ("Error:"-Zeilen kommen auch bei erfolgreich wiederholtem Speichern)
    $errors = $out | Where-Object { $_ -match 'Traceback' }
    $secs = [int]((Get-Date) - $start).TotalSeconds
    if ($LASTEXITCODE -ne 0 -or -not $result -or $errors) {
        Write-Host "   FEHLER nach $secs s" -ForegroundColor Red
        $out | Select-Object -Last 15 | ForEach-Object { Write-Host "   $_" }
        $failed += $s.Name
    } else {
        Write-Host "   $result ($secs s)" -ForegroundColor Green
    }
}
if ($failed.Count -gt 0) { Write-Host "Fehlgeschlagen: $($failed -join ', ')" -ForegroundColor Red; exit 1 }
Write-Host "Alle Grafiken fertig." -ForegroundColor Green
