# Sammelt alle deutschen Schluessel (Meilenstein 10a) nach strings_de.txt und meldet die fehlenden englischen Eintraege.
# Aufruf: powershell -ExecutionPolicy Bypass -File Tools\Localization\extract_strings.ps1
# ASCII-only, PowerShell 5.1.
#
# Quellen:
#  (a) erstes String-Literal jedes Loc.T( / Loc.F( Aufrufs in Assets/_Project/Scripts/**/*.cs
#  (b) Builder-Daten in Assets/_Project/Scripts/Editor/*.cs (Item(..), Q(..), Option(..), MenuButton(..), .text =, displayName = ...
#      plus Heuristik: Literale mit Leerzeichen oder Umlauten, die weder Pfad noch Bezeichner sind)
#  (c) RebindingView.Rows, LocationBanner.Names
#  (d) extra_keys.txt (Schluessel, die nur ueber Konstanten laufen und von (a) nicht gefunden werden)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path (Join-Path $here '..\..')).Path
$scripts = Join-Path $root 'Assets\_Project\Scripts'
$editorDir = Join-Path $scripts 'Editor'
$enJson = Join-Path $root 'Assets\_Project\Resources\Localization\Loc_en.json'
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Unescape-CSharp([string]$s, [bool]$verbatim) {
    if ($verbatim) { return $s.Replace('""', '"') }
    $sb = New-Object System.Text.StringBuilder
    $i = 0
    while ($i -lt $s.Length) {
        $c = $s[$i]
        if ($c -eq '\' -and $i + 1 -lt $s.Length) {
            $n = $s[$i + 1]
            $i += 2
            switch ($n) {
                'n' { [void]$sb.Append("`n") }
                'r' { }
                't' { [void]$sb.Append("`t") }
                '"' { [void]$sb.Append('"') }
                "'" { [void]$sb.Append("'") }
                '\' { [void]$sb.Append('\') }
                '0' { }
                'u' {
                    if ($i + 4 -le $s.Length) { [void]$sb.Append([char][Convert]::ToInt32($s.Substring($i, 4), 16)); $i += 4 }
                }
                'x' {
                    $len = 0
                    while ($len -lt 4 -and $i + $len -lt $s.Length -and $s[$i + $len] -match '[0-9a-fA-F]') { $len++ }
                    if ($len -gt 0) { [void]$sb.Append([char][Convert]::ToInt32($s.Substring($i, $len), 16)); $i += $len }
                }
                default { [void]$sb.Append($n) }
            }
        }
        else { [void]$sb.Append($c); $i++ }
    }
    return $sb.ToString()
}

# Zerlegt eine Zeile in String-Literale (ohne Kommentar-Rest). Liefert Objekte mit Value, Prefix, Interpolated.
function Get-Literals([string]$line) {
    $result = New-Object System.Collections.ArrayList
    $i = 0
    $n = $line.Length
    while ($i -lt $n) {
        $c = $line[$i]
        if ($c -eq '/' -and $i + 1 -lt $n -and $line[$i + 1] -eq '/') { break }
        $interp = $false
        $verb = $false
        $start = -1
        if ($c -eq '"') { $start = $i }
        elseif (($c -eq '@' -or $c -eq '$') -and $i + 1 -lt $n) {
            $j = $i
            while ($j -lt $n -and ($line[$j] -eq '@' -or $line[$j] -eq '$')) {
                if ($line[$j] -eq '@') { $verb = $true } else { $interp = $true }
                $j++
            }
            if ($j -lt $n -and $line[$j] -eq '"') { $start = $j } else { $i = $j; continue }
        }
        elseif ($c -eq "'") {
            # Zeichenliteral ueberspringen
            $j = $i + 1
            while ($j -lt $n -and $line[$j] -ne "'") { if ($line[$j] -eq '\') { $j++ }; $j++ }
            $i = $j + 1
            continue
        }
        if ($start -lt 0) { $i++; continue }
        $j = $start + 1
        $body = New-Object System.Text.StringBuilder
        $closed = $false
        while ($j -lt $n) {
            $d = $line[$j]
            if ($verb) {
                if ($d -eq '"') {
                    if ($j + 1 -lt $n -and $line[$j + 1] -eq '"') { [void]$body.Append('""'); $j += 2; continue }
                    $closed = $true; break
                }
            }
            else {
                if ($d -eq '\' -and $j + 1 -lt $n) { [void]$body.Append($d); [void]$body.Append($line[$j + 1]); $j += 2; continue }
                if ($d -eq '"') { $closed = $true; break }
            }
            [void]$body.Append($d); $j++
        }
        if (-not $closed) { break }
        $prefixStart = $i
        $obj = New-Object PSObject -Property @{
            Value = (Unescape-CSharp $body.ToString() $verb)
            Prefix = $line.Substring(0, $prefixStart)
            Interpolated = $interp
        }
        [void]$result.Add($obj)
        $i = $j + 1
    }
    return ,$result
}

function Test-HasLetter([string]$s) { return $s -match '[^\W\d_]' }
function Test-NonAscii([string]$s) { return $s -match '[^\x00-\x7F]' }

$keys = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
$counts = @{ a = 0; b = 0; c = 0; d = 0 }

function Add-Key([string]$value, [string]$source) {
    if ([string]::IsNullOrEmpty($value)) { return }
    if (-not (Test-HasLetter $value)) { return }
    if ($keys.Add($value)) { $counts[$source]++ }
}

$excludeLine = 'Debug\.|MenuItem|Exception|new GameObject\(|\.name\s*=|Find\(|Path\.|AssetDatabase|EditorUtility|FindAction|Header\(|CreateAsset|LoadAsset|Resources\.Load|Directory\.|File\.|SetTrigger|StartsWith|EndsWith|Contains\(|Tooltip\(|using |\[Test|Assert|Warn|Error'
$ctxItemName = '\bItem\(\s*(?:"(?:[^"\\]|\\.)*"\s*,\s*){2}$'
$ctxItemDesc = '\bItem\(\s*(?:"(?:[^"\\]|\\.)*"\s*,\s*){3}$'
$ctxSimple = '(?:\bQ\(|\bOption\(|\.text\s*=\s*|\b(?:displayName|greeting|stage2Name|guildName|spellName|description|itemName)\s*=\s*|\bSpell\(\s*"[^"]*"\s*,\s*|\bAddNpc\(\s*[^,]+,\s*"[^"]*"\s*,\s*[^,]+,\s*|\bGuildMaster\(\s*[^,]+,\s*"[^"]*"\s*,\s*|\bTownTrader\(\s*[^,]+,\s*"[^"]*"\s*,\s*)$'
$ctxMenuButton = 'MenuButton\(\s*"[^"]*"\s*,\s*[^,]+,\s*new Vector2\([^)]*\)\s*,\s*new Vector2\([^)]*\)\s*,\s*$'

$allFiles = Get-ChildItem -Path $scripts -Recurse -Filter *.cs
foreach ($f in $allFiles) {
    $isEditor = $f.FullName.StartsWith($editorDir, [StringComparison]::OrdinalIgnoreCase)
    $lines = [System.IO.File]::ReadAllLines($f.FullName, [System.Text.Encoding]::UTF8)
    foreach ($line in $lines) {
        if ($line.TrimStart().StartsWith('//')) { continue }
        if ($line.IndexOf('"') -lt 0) { continue }
        $lits = Get-Literals $line
        foreach ($l in $lits) {
            # (a) Loc.T( / Loc.F(
            if ($l.Prefix -match 'Loc\.[TF]\(\s*$') { Add-Key $l.Value 'a'; continue }
            if (-not $isEditor) { continue }
            if ($l.Interpolated) { continue }
            $v = $l.Value
            if ($v.Contains('{')) { continue }
            # (b) Builder-Daten
            if ($l.Prefix -match $ctxItemName -or $l.Prefix -match $ctxItemDesc -or $l.Prefix -match $ctxSimple -or $l.Prefix -match $ctxMenuButton) {
                if ($v -cmatch '^[a-z0-9_]+$') { continue }
                Add-Key $v 'b'
                continue
            }
            if ($line -match $excludeLine) { continue }
            if ($v.Contains('/') -or $v -match '\.(png|asset|unity|json|prefab|cs|mat|txt|anim|controller)$') { continue }
            if ($v -cmatch '^[a-z0-9_]+$') { continue }
            if ($v -match '^[<\[]' -or $v -match '^\s|[\s,)]$|^Meilenstein') { continue }
            if ($v.Contains(' ') -or (Test-NonAscii $v)) { Add-Key $v 'b' }
        }
    }
}

# (c) RebindingView.Rows und LocationBanner.Names
$str = '"((?:[^"\\\r\n]|\\.)*)"'
$rebind = Join-Path $scripts 'UI\RebindingView.cs'
if (Test-Path $rebind) {
    $t = [System.IO.File]::ReadAllText($rebind, [System.Text.Encoding]::UTF8)
    foreach ($m in [regex]::Matches($t, '\(\s*"[A-Za-z0-9]+"\s*,\s*(?:null|"[a-z]+")\s*,\s*' + $str + '\s*\)')) {
        Add-Key (Unescape-CSharp $m.Groups[1].Value $false) 'c'
    }
}
$banner = Join-Path $scripts 'UI\LocationBanner.cs'
if (Test-Path $banner) {
    $t = [System.IO.File]::ReadAllText($banner, [System.Text.Encoding]::UTF8)
    foreach ($m in [regex]::Matches($t, '\{\s*"[A-Za-z_]+"\s*,\s*' + $str + '\s*\}')) {
        Add-Key (Unescape-CSharp $m.Groups[1].Value $false) 'c'
    }
}

# (d) zusaetzliche Schluessel
$extra = Join-Path $here 'extra_keys.txt'
if (Test-Path $extra) {
    foreach ($line in [System.IO.File]::ReadAllLines($extra, [System.Text.Encoding]::UTF8)) {
        if ($line.Trim().Length -eq 0 -or $line.StartsWith('#')) { continue }
        Add-Key ($line.Replace('\n', "`n")) 'd'
    }
}

function Format-Line([string]$s) { return $s.Replace("`r`n", '\n').Replace("`n", '\n') }

$sorted = New-Object 'System.Collections.Generic.List[string]'
foreach ($k in $keys) { $sorted.Add($k) }
$sorted.Sort([StringComparer]::Ordinal)
$outLines = New-Object 'System.Collections.Generic.List[string]'
foreach ($k in $sorted) { $outLines.Add((Format-Line $k)) }
[System.IO.File]::WriteAllText((Join-Path $here 'strings_de.txt'), (($outLines -join "`n") + "`n"), $utf8)

# Vorhandene englische Eintraege
$have = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
$jsonFound = $false
if (Test-Path $enJson) {
    $jsonFound = $true
    $text = [System.IO.File]::ReadAllText($enJson, [System.Text.Encoding]::UTF8)
    $table = $text | ConvertFrom-Json
    if ($table.entries) {
        foreach ($e in $table.entries) {
            if ($e.de -and $e.en) { [void]$have.Add([string]$e.de) }
        }
    }
}
$missing = New-Object 'System.Collections.Generic.List[string]'
foreach ($k in $sorted) { if (-not $have.Contains($k)) { $missing.Add((Format-Line $k)) } }
[System.IO.File]::WriteAllText((Join-Path $here 'missing_en.txt'), (($missing -join "`n") + "`n"), $utf8)

Write-Output ('Schluessel gesamt: ' + $sorted.Count)
Write-Output ('  (a) Loc.T/Loc.F: ' + $counts['a'] + ', (b) Builder: ' + $counts['b'] + ', (c) Rows/Names: ' + $counts['c'] + ', (d) extra: ' + $counts['d'])
if ($jsonFound) {
    Write-Output ('Mit englischem Eintrag: ' + ($sorted.Count - $missing.Count) + ' von ' + $sorted.Count + ' (Loc_en.json, ' + $have.Count + ' Eintraege)')
}
else {
    Write-Output 'Loc_en.json nicht gefunden: alle Schluessel gelten als fehlend.'
}
Write-Output ('Fehlend: ' + $missing.Count + ' -> Tools\Localization\missing_en.txt')
