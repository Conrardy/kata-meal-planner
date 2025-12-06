#!/usr/bin/env pwsh

# Generate AIDD documentation files
# Usage: .\aidd-generate-docs.ps1 [OPTIONS]

param(
    [switch]$Tree,
    [switch]$MemoryBank,
    [switch]$Rules,
    [switch]$All,
    [string]$DocsDir = "",
    [switch]$Help
)

$ErrorActionPreference = "Stop"

$CURRENT_DIR = Get-Location

# Detect if running in dev (cli/assets) or installed (aidd/assets) structure
if (Test-Path "$CURRENT_DIR/cli/assets") {
    # Dev structure
    $CLI = "$CURRENT_DIR/cli/assets/scripts/cli"
    $TEMPLATE_DIR = "$CURRENT_DIR/prompts/templates"
} else {
    # Installed structure
    $CLI = "$CURRENT_DIR/aidd/assets/scripts/cli"
    $TEMPLATE_DIR = "$CURRENT_DIR/aidd/prompts/templates"
}

if ([string]::IsNullOrEmpty($DocsDir)) {
    $DocsDir = "$CURRENT_DIR/docs"
}

# Component flags
$GENERATE_TREE = $false
$GENERATE_MEMORY_BANK = $false
$GENERATE_RULES = $false

function Show-Help {
    Write-Host @"
Usage: .\aidd-generate-docs.ps1 [OPTIONS]

Generate AIDD documentation files with selective component generation

OPTIONS:
  -Tree            Generate project tree section
  -MemoryBank      Generate memory bank section
  -Rules           Generate coding rules section
  -All             Generate all components (default if no flags)
  -DocsDir <path>  Override docs directory (default: ./docs)
  -Help            Show this help message

Examples:
  .\aidd-generate-docs.ps1                      # Generate all components
  .\aidd-generate-docs.ps1 -All                 # Generate all components
  .\aidd-generate-docs.ps1 -Tree                # Only project tree
  .\aidd-generate-docs.ps1 -MemoryBank          # Only memory bank
  .\aidd-generate-docs.ps1 -Rules               # Only rules
  .\aidd-generate-docs.ps1 -Rules -MemoryBank   # Rules and memory bank
"@
}

# Show help and exit if requested
if ($Help) {
    Show-Help
    exit 0
}

# Set flags based on parameters
if ($Tree) { $GENERATE_TREE = $true }
if ($MemoryBank) { $GENERATE_MEMORY_BANK = $true }
if ($Rules) { $GENERATE_RULES = $true }
if ($All) {
    $GENERATE_TREE = $true
    $GENERATE_MEMORY_BANK = $true
    $GENERATE_RULES = $true
}

# If no specific flags are provided, generate all components
if (-not $GENERATE_TREE -and -not $GENERATE_MEMORY_BANK -and -not $GENERATE_RULES) {
    $GENERATE_TREE = $true
    $GENERATE_MEMORY_BANK = $true
    $GENERATE_RULES = $true
}

# Ensure docs directory exists
if (-not (Test-Path $DocsDir)) {
    New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null
}

# Generate components based on flags
if ($GENERATE_TREE) {
    Write-Host "Generating project tree..."
    $parentDir = Split-Path -Parent $CURRENT_DIR
    & pwsh "$CLI/tree.ps1" -ScanDir $parentDir -OutputFile "$DocsDir/tree.txt"
} else {
    Write-Host "Skipping tree generation"
}

if ($GENERATE_RULES) {
    Write-Host "Generating rules..."
    if (Test-Path "$DocsDir/rules") {
        node "$CLI/merge.cjs" `
            --input-dir="$DocsDir/rules" `
            --output-file="$DocsDir/rules.md"
    } else {
        Write-Host "Warning: Rules directory not found at $DocsDir/rules"
    }
} else {
    Write-Host "Skipping rules generation"
}

if ($GENERATE_MEMORY_BANK) {
    Write-Host "Generating AGENTS.md with memory bank..."
    
    if (-not (Test-Path $DocsDir)) {
        New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null
    }

    $AGENTS_TEMPLATE = "$TEMPLATE_DIR/memory-bank/AGENTS.md"
    $AGENTS_HEADERS = "$DocsDir/AGENTS_HEADERS.md"

    # Remove existing AGENTS.md if it exists
    if (Test-Path "$DocsDir/AGENTS.md") {
        Remove-Item "$DocsDir/AGENTS.md" -Force -Verbose
    }

    # Create AGENTS_HEADERS from template if it doesn't exist
    if (-not (Test-Path $AGENTS_HEADERS)) {
        if (-not (Test-Path $AGENTS_TEMPLATE)) {
            Write-Host "❌ Missing AGENTS template at $AGENTS_TEMPLATE" -ForegroundColor Red
            exit 1
        }

        Copy-Item $AGENTS_TEMPLATE $AGENTS_HEADERS
        Write-Host "✅ Created AGENTS headers from template at $AGENTS_HEADERS" -ForegroundColor Green
    }

    # Copy headers to AGENTS.md
    Copy-Item $AGENTS_HEADERS "$DocsDir/AGENTS.md"

    # Merge memory-bank entries if they exist
    if (Test-Path "$DocsDir/memory-bank") {
        $memoryBankFiles = Get-ChildItem -Path "$DocsDir/memory-bank" -File -Recurse -Include "*.md","*.mdc","*.mmd","*.txt" -ErrorAction SilentlyContinue
        if ($memoryBankFiles) {
            # Merge all memory-bank entries except the top-level AGENTS.md
            # Use temp file for cross-platform compatibility
            $tempFile = [System.IO.Path]::GetTempFileName()
            try {
                node "$CLI/merge.cjs" --input-dir="$DocsDir/memory-bank" --ignore="AGENTS.md" --output-file="$tempFile"
                if (Test-Path $tempFile) {
                    $mergedContent = Get-Content -Path $tempFile -Raw
                    if ($mergedContent) {
                        Add-Content -Path "$DocsDir/AGENTS.md" -Value $mergedContent
                    }
                }
            } finally {
                # Clean up temp file
                if (Test-Path $tempFile) {
                    Remove-Item $tempFile -Force
                }
            }
        }
    }

    # Handle AGENTS.md symlink/file in current directory
    $agentsInRoot = "$CURRENT_DIR/AGENTS.md"
    $agentsTarget = "docs/AGENTS.md"
    
    if (Test-Path $agentsInRoot) {
        $item = Get-Item $agentsInRoot
        if ($item.LinkType -eq "SymbolicLink") {
            Write-Host "✅ AGENTS.md already symlinked, leaving as-is" -ForegroundColor Green
        } else {
            # Backup existing file
            $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
            $backupPath = "$CURRENT_DIR/AGENTS.md.backup-$timestamp"
            Move-Item $agentsInRoot $backupPath
            Write-Host "✅ Backed up to: $backupPath" -ForegroundColor Green
            
            # Create symlink (requires admin privileges on Windows)
            try {
                New-Item -ItemType SymbolicLink -Path $agentsInRoot -Target $agentsTarget -Force | Out-Null
                Write-Host "✅ Created symlink: AGENTS.md -> docs/AGENTS.md" -ForegroundColor Green
            } catch {
                Write-Host "⚠️  Could not create symlink (may require admin privileges). Using copy instead." -ForegroundColor Yellow
                Copy-Item "$DocsDir/AGENTS.md" $agentsInRoot
            }
        }
    } else {
        # Create new symlink
        try {
            New-Item -ItemType SymbolicLink -Path $agentsInRoot -Target $agentsTarget -Force | Out-Null
            Write-Host "✅ Created symlink: AGENTS.md -> docs/AGENTS.md" -ForegroundColor Green
        } catch {
            Write-Host "⚠️  Could not create symlink (may require admin privileges). Using copy instead." -ForegroundColor Yellow
            Copy-Item "$DocsDir/AGENTS.md" $agentsInRoot
        }
    }

    # Process subdirectories in memory-bank
    if (Test-Path "$DocsDir/memory-bank") {
        Get-ChildItem -Path "$DocsDir/memory-bank" -Directory | ForEach-Object {
            $subdirName = $_.Name
            Write-Host "Processing memory-bank subdirectory: $subdirName"
            node "$CLI/merge.cjs" --input-dir="$DocsDir/memory-bank/$subdirName" --output-file="$DocsDir/$subdirName.md"
        }
    }
} else {
    Write-Host "Skipping memory bank generation"
}

Write-Host "`n✅ Documentation generation complete!" -ForegroundColor Green
