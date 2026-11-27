<#
.SYNOPSIS
    Generates the SQL CLR install script from the template and a built assembly.

.DESCRIPTION
    The script in AhoCorasick.SqlClr/dist installs the SQL CLR functions by embedding the assembly
    as a hex literal, so it has to be regenerated whenever the library changes. SSDT generates it
    from AhoCorasick.SqlClr.sqlproj, but that needs a Visual Studio install and pins the build to one
    Visual Studio version, so CI fills in a template instead: the DDL is fixed, only the two hex
    blobs and the hash of the assembly change.

    The set of functions in the template is checked against the methods in Contains.cs, so that a
    function added or removed there cannot silently leave the template behind.
#>
[CmdletBinding()]
param(
    [string] $Assembly = 'AhoCorasick.SqlClr.Build/bin/Release/net40/AhoCorasick.SqlClr.Build.dll',
    [string] $Source = 'AhoCorasick.SqlClr/Contains.cs',
    [string] $Template = 'AhoCorasick.SqlClr/dist/AhoCorasick.SqlClr_Create.template.sql',
    [string] $OutFile = 'AhoCorasick.SqlClr/dist/AhoCorasick.SqlClr_Create.sql'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$dllPath = (Resolve-Path -LiteralPath $Assembly).Path
$pdbPath = [IO.Path]::ChangeExtension($dllPath, '.pdb')
if (-not (Test-Path -LiteralPath $pdbPath)) { throw "No pdb next to the assembly: $pdbPath" }

$text = [IO.File]::ReadAllText((Resolve-Path -LiteralPath $Template).Path)

# The functions the template declares.
$declared = [regex]::Matches($text, '\[UserDefinedFunctions\]\.\[(?<name>\w+)\]') |
    ForEach-Object { $_.Groups['name'].Value } | Sort-Object -Unique

# The methods carrying [SqlFunction]. The attribute can span lines, so take the name from the first
# method declaration after it.
$implemented = New-Object System.Collections.Generic.List[string]
$pending = $false
foreach ($line in [IO.File]::ReadAllLines((Resolve-Path -LiteralPath $Source).Path)) {
    if ($line -match '\[SqlFunction') { $pending = $true }
    elseif ($pending -and $line -match 'public\s+static\s+[\w<>\[\]\.]+\s+(?<name>\w+)\s*\(') {
        $implemented.Add($Matches['name'])
        $pending = $false
    }
}
$implemented = $implemented | Sort-Object -Unique

$missing = $implemented | Where-Object { $declared -notcontains $_ }
$extra = $declared | Where-Object { $implemented -notcontains $_ }
if ($missing -or $extra) {
    if ($missing) { Write-Host "In Contains.cs but not in the template: $($missing -join ', ')" }
    if ($extra) { Write-Host "In the template but not in Contains.cs: $($extra -join ', ')" }
    throw "$Template is out of date. Rebuild it with SSDT from AhoCorasick.SqlClr.sqlproj."
}
Write-Host "Functions in template and source agree: $($declared.Count)"

function ConvertTo-SqlHex([string] $path) {
    $bytes = [IO.File]::ReadAllBytes($path)
    '0x' + [BitConverter]::ToString($bytes).Replace('-', '')
}

# sp_add_trusted_assembly takes the SHA2_512 of the assembly content.
$sha512 = [BitConverter]::ToString([Security.Cryptography.SHA512]::Create().ComputeHash([IO.File]::ReadAllBytes($dllPath))).Replace('-', '')

$text = $text.Replace('{ASSEMBLY_HEX}', (ConvertTo-SqlHex $dllPath))
$text = $text.Replace('{PDB_HEX}', (ConvertTo-SqlHex $pdbPath))
$text = $text.Replace('{ASSEMBLY_SHA512}', "0x$sha512")

$remaining = [regex]::Matches($text, '\{[A-Z_0-9]+\}') | ForEach-Object { $_.Value } | Sort-Object -Unique
if ($remaining) { throw "Unfilled placeholders in the generated script: $($remaining -join ', ')" }

$outDir = Split-Path -Parent $OutFile
if ($outDir -and -not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

# The file SSDT produces is UTF-8 with a BOM, so keep that.
[IO.File]::WriteAllText($OutFile, $text, (New-Object Text.UTF8Encoding $true))

Write-Host "assembly  $dllPath ($((Get-Item $dllPath).Length) bytes)"
Write-Host "pdb       $pdbPath ($((Get-Item $pdbPath).Length) bytes)"
Write-Host "sha512    $sha512"
Write-Host "generated $OutFile ($((Get-Item $OutFile).Length) bytes)"
