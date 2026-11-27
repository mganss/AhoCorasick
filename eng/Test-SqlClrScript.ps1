<#
.SYNOPSIS
    Installs the generated SQL CLR script into a SQL Server instance and calls the functions.

.DESCRIPTION
    Runs the install script the way a user would, then checks a few of the functions against known
    results. The dictionary is "aca" and "ca" over the text "aca", which is the case that used to
    lose the match of "ca", so this also exercises the fix through SQL Server rather than only
    through the unit tests.
#>
[CmdletBinding()]
param(
    [string] $Script = 'AhoCorasick.SqlClr/dist/AhoCorasick.SqlClr_Create.sql',
    [string] $Server = '(localdb)\MSSQLLocalDB',
    [string] $Database = 'AhoCorasickSqlClrTest'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module SqlServer

function Invoke-Master([string] $query) {
    Invoke-Sqlcmd -ServerInstance $Server -TrustServerCertificate -Database master -Query $query -ErrorAction Stop
}

function Invoke-Test([string] $query) {
    Invoke-Sqlcmd -ServerInstance $Server -TrustServerCertificate -Database $Database -Query $query -ErrorAction Stop
}

function Assert-Equal($expected, $actual, [string] $what) {
    if ("$actual" -ne "$expected") { throw "$what : expected [$expected] but got [$actual]" }
    Write-Host "ok  $what = $actual"
}

Write-Host "Enabling CLR integration on $Server"
Invoke-Master "exec sp_configure 'show advanced options', 1; reconfigure;" | Out-Null
Invoke-Master "exec sp_configure 'clr enabled', 1; reconfigure;" | Out-Null
$strict = Invoke-Master "select value_in_use from sys.configurations where name = 'clr strict security'"
if ($strict) { Write-Host "clr strict security = $($strict.value_in_use)" }

Write-Host "Creating $Database"
Invoke-Master "if db_id('$Database') is not null begin alter database [$Database] set single_user with rollback immediate; drop database [$Database]; end; create database [$Database];" | Out-Null

Write-Host "Running $Script"
Invoke-Sqlcmd -ServerInstance $Server -TrustServerCertificate -Database $Database -InputFile (Resolve-Path -LiteralPath $Script).Path -ErrorAction Stop | Out-Null

$words = '<root><row Word="aca"/><row Word="ca"/></root>'

Write-Host 'Calling the functions'
$hit = Invoke-Test "select dbo.ContainsWords('$words', N'aca', 'o') as Hit"
Assert-Equal $true $hit.Hit 'ContainsWords'

$rows = Invoke-Test "select * from dbo.ContainsWordsTable('$words', N'aca', 'o') order by [Index]"
$matches = ($rows | ForEach-Object { "$($_.Index):$($_.Word)" }) -join ', '
Assert-Equal '0:aca, 1:ca' $matches 'ContainsWordsTable'

$miss = Invoke-Test "select dbo.ContainsWords('$words', N'bb', 'o') as Hit"
Assert-Equal $false $miss.Hit 'ContainsWords with no match'

# The static object path from the README, where the trie is built once and reused.
$handle = Invoke-Test "select dbo.CreateAhoCorasick('$words', 'o') as H"
$byObject = Invoke-Test "select dbo.ContainsWordsByObject(N'aca', '$($handle.H)') as Hit"
Assert-Equal $true $byObject.Hit 'ContainsWordsByObject'

$objectRows = Invoke-Test "select * from dbo.ContainsWordsTableByObject(N'aca', '$($handle.H)') order by [Index]"
$objectMatches = ($objectRows | ForEach-Object { "$($_.Index):$($_.Word)" }) -join ', '
Assert-Equal '0:aca, 1:ca' $objectMatches 'ContainsWordsTableByObject'

$listed = Invoke-Test 'select count(*) as N from dbo.ListAhoCorasick()'
Assert-Equal 1 $listed.N 'ListAhoCorasick'

Invoke-Test "select dbo.DeleteAhoCorasick('$($handle.H)') as Deleted" | Out-Null
$listedAfter = Invoke-Test 'select count(*) as N from dbo.ListAhoCorasick()'
Assert-Equal 0 $listedAfter.N 'ListAhoCorasick after DeleteAhoCorasick'

# Case insensitive matching, the ":i" suffix from the README.
$insensitive = Invoke-Test "select dbo.ContainsWords('<root><row Word=""ACA""/></root>', N'aca', 'o:i') as Hit"
Assert-Equal $true $insensitive.Hit 'ContainsWords ordinal case insensitive'

Write-Host 'All SQL CLR checks passed'
