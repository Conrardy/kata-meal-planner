#!/usr/bin/env pwsh

param(
    [string]$ConfigFile = "symlinks.yml"
)

$ErrorActionPreference = 'Stop'

function Write-Info($msg) { Write-Host $msg -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host $msg -ForegroundColor Green }
function Write-Warn($msg) { Write-Host $msg -ForegroundColor Yellow }
function Write-Err($msg)  { Write-Host $msg -ForegroundColor Red }

# Repository root (script location)

# Force RepoRoot to workspace root, not just script location
$RepoRoot = (Resolve-Path "$PSScriptRoot")
if ($RepoRoot -notlike "*kata-meal-planner*") {
    $RepoRoot = (Resolve-Path ".")
    if ($RepoRoot -notlike "*kata-meal-planner*") {
        throw "Could not determine workspace root. Please run from kata-meal-planner directory."
    }
}

# Validate config
$cfgPath = Join-Path $RepoRoot $ConfigFile
if (-not (Test-Path $cfgPath)) {
    Write-Err "Error: Config file '$ConfigFile' not found at '$RepoRoot'"
    exit 1
}

# Simple YAML parser for expected format
function Parse-SymlinkConfig([string]$Path) {
    $inSection = $false
    $entryStarted = $false
    $current = @{ source = ''; target = ''; description = '' }
    $entries = @()

    Get-Content -LiteralPath $Path | ForEach-Object {
        $line = $_
        if ($line -match '^[\s]*#') { return }
        if ($line -replace '\s','' -eq '') { return }

        if ($line -match '^symlinks:') { $inSection = $true; return }
        if (-not $inSection) { return }

        # New entry start inline: - source: ...
        $m = [regex]::Match($line, '^[\s]*-[\s]*source:[\s]*(.+)$')
        if ($m.Success) {
            if ($entryStarted -and $current.source -and $current.target) {
                $entries += [PSCustomObject]$current
            }
            $current = @{ source = $m.Groups[1].Value.Trim(); target = ''; description = '' }
            $entryStarted = $true
            return
        }
        # New entry dash only
        if ($line -match '^[\s]*-\s*$') {
            if ($entryStarted -and $current.source -and $current.target) {
                $entries += [PSCustomObject]$current
            }
            $current = @{ source = ''; target = ''; description = '' }
            $entryStarted = $true
            return
        }
        # source:
        $m = [regex]::Match($line, '^[\s]+source:[\s]*(.+)$')
        if ($m.Success) { $current.source = $m.Groups[1].Value.Trim(); return }
        # target:
        $m = [regex]::Match($line, '^[\s]+target:[\s]*(.+)$')
        if ($m.Success) { $current.target = $m.Groups[1].Value.Trim(); return }
        # description:
        $m = [regex]::Match($line, '^[\s]+description:[\s]*(.+)$')
        if ($m.Success) { $current.description = $m.Groups[1].Value.Trim(); return }
    }

    if ($entryStarted -and $current.source -and $current.target) {
        $entries += [PSCustomObject]$current
    }

    return $entries
}

function Is-Symlink([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $false }
    try {
        $item = Get-Item -LiteralPath $Path -Force
        return ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0
    } catch { return $false }
}

function Get-LinkTarget([string]$Path) {
    try {
        $item = Get-Item -LiteralPath $Path -Force
        if ($item -and ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            return $item.Target
        }
    } catch {}
    return $null
}

function Ensure-Dir([string]$DirPath) {
    if (-not (Test-Path -LiteralPath $DirPath)) {
        New-Item -ItemType Directory -Path $DirPath -Force | Out-Null
    }
}

function Create-Link([string]$SourceAbs, [string]$TargetRel) {
    # Remove existing if present
    if (Test-Path -LiteralPath $SourceAbs -PathType Any) {
        if (Is-Symlink $SourceAbs) {
            Remove-Item -LiteralPath $SourceAbs -Force -Recurse
        } else {
            # Skip replacing real files
            return $false
        }
    }
    New-Item -ItemType SymbolicLink -Path $SourceAbs -Target $TargetRel | Out-Null
    return $true
}

Write-Info "=== Symlink Management (PowerShell) ==="
Write-Info ("Repo: {0}" -f $RepoRoot)
Write-Info ("Config: {0}" -f $cfgPath)
Write-Host

$entries = Parse-SymlinkConfig -Path $cfgPath
if (-not $entries -or $entries.Count -eq 0) {
    Write-Err "No symlink entries found in configuration."
    exit 1
}

$total = 0; $created = 0; $existing = 0; $errors = 0; $skipped = 0; $updated = 0

foreach ($e in $entries) {
    $total++
    $sourceRel = ($e.source -replace '"','').Trim()
    $targetRel = ($e.target -replace '"','').Trim()
    $desc = $e.description

    $sourceAbs = Join-Path $RepoRoot $sourceRel
    try {
        $targetAbs = (Resolve-Path -Path (Join-Path $RepoRoot $targetRel) -ErrorAction Stop).Path
    } catch {
        $targetAbs = $null
    }
    $sourceDir = Split-Path -Parent $sourceAbs

    Write-Info ([string]::Format('[{0}] {1}', $total, $sourceRel))
    if ($desc) { Write-Warn ([string]::Format('    -> {0}', $desc)) }

    Ensure-Dir $sourceDir

    if (-not $targetAbs -or -not (Test-Path -LiteralPath $targetAbs)) {
        $targetDisplay = if ($targetAbs) { $targetAbs } else { $targetRel }
        Write-Warn ([string]::Format('    [!] Target missing: {0}', $targetDisplay))
        $errors++
        Write-Host
        continue
    }

    if (Is-Symlink $sourceAbs) {
        $currentTarget = Get-LinkTarget $sourceAbs
        Write-Ok ([string]::Format('    [OK] Symlink exists: {0} -> {1}', $sourceRel, $currentTarget))
        $match = $false
        if ($currentTarget) {
            # Accept relative or absolute match
            $fullCurrent = $null
            try { $fullCurrent = [System.IO.Path]::GetFullPath((Join-Path $sourceDir $currentTarget)) } catch {}
            $fullTarget  = [System.IO.Path]::GetFullPath($targetAbs)
            if ($currentTarget -eq $targetRel -or ($fullCurrent -and $fullCurrent -eq $fullTarget)) { $match = $true }
        }
        if ($match) {
            Write-Ok '    [OK] Target matches configuration'
            $existing++
        } else {
            Write-Info '    [*] Updating symlink to match config'
            if (Create-Link -SourceAbs $sourceAbs -TargetRel $targetRel) {
                $updated++
                Write-Ok '    [OK] Symlink updated'
            } else {
                $errors++
                Write-Err '    [ERR] Failed to update symlink'
            }
        }
    } elseif (Test-Path -LiteralPath $sourceAbs) {
        Write-Warn '    [!] File exists but is not a symlink; skipping'
        $skipped++
    } else {
        Write-Info '    [*] Creating symlink...'
        if (Create-Link -SourceAbs $sourceAbs -TargetRel $targetRel) {
            $created++
            Write-Ok ([string]::Format('    [OK] Created: {0} -> {1}', $sourceRel, $targetRel))
        } else {
            $errors++
            Write-Err '    [ERR] Failed to create symlink'
        }
    }
    Write-Host
}

Write-Info "=== Summary ==="
Write-Host ("Total:   {0}" -f $total)
Write-Host ("Created: {0}" -f $created)
Write-Host ("Updated: {0}" -f $updated)
Write-Host ("Existing:{0}" -f $existing)
Write-Host ("Skipped: {0}" -f $skipped)
Write-Host ("Errors:  {0}" -f $errors)

if ($errors -gt 0) { exit 1 } else { exit 0 }
