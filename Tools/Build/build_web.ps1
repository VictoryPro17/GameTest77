# RE:BORN - Web-Demo bauen (Meilenstein 10c)
# Baut die Demo (Stadt bis Golem-Boss, Frostgipfel gesperrt) als WebGL nach Builds/WebDemo und packt Builds/RebornDemo.zip.
# Voraussetzung: Unity-Editor mit dem Projekt ist offen und NICHT im Play-Modus.
# Danach schaltet der Editor wieder auf die vorherige Plattform (meist Windows) zurueck.
# Aufruf:  powershell -File Tools/Build/build_web.ps1
$ErrorActionPreference = "Stop"
$project = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$U = @('--caller', 'plugin', '--skill', 'unity-cli', '--project-path', $project, '--no-banner')
$out = Join-Path $project "Builds\WebDemo"
$zip = Join-Path $project "Builds\RebornDemo.zip"

$ErrorActionPreference = "Continue"
$mode = (unity command editor status @U --format json | ConvertFrom-Json).data.result.playMode
if ($mode -ne 'stopped') { Write-Host "Der Editor ist im Play-Modus ($mode). Bitte erst stoppen." -ForegroundColor Red; exit 1 }

$tmp = Join-Path $env:TEMP 'reborn_webbuild.cs'
[IO.File]::WriteAllText($tmp, 'return Reborn.EditorTools.WebBuild.BuildDemo(@"' + $out + '");')
Write-Host "== WebGL-Build (kann 15-30 Minuten dauern) ..."
$start = Get-Date
$r = unity command script eval @U --result-only --timeout 3600 --file $tmp 2>&1 | Out-String
Write-Host $r
[IO.File]::WriteAllText($tmp, 'return Reborn.EditorTools.WebBuild.SwitchBack();')
Write-Host "== Zurueck auf die vorherige Plattform ..."
unity command script eval @U --result-only --timeout 3600 --file $tmp 2>&1 | Out-String | Write-Host
$ErrorActionPreference = "Stop"

if ($r -notmatch 'WebGL Succeeded') { Write-Host "Build fehlgeschlagen." -ForegroundColor Red; exit 1 }
if (Test-Path $zip) { Remove-Item $zip }
Compress-Archive -Path (Join-Path $out '*') -DestinationPath $zip
$size = (Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum / 1MB
Write-Host ("Fertig: {0} ({1:N1} MB), ZIP: {2}, Dauer {3:N0} min" -f $out, $size, $zip, ((Get-Date) - $start).TotalMinutes) -ForegroundColor Green
