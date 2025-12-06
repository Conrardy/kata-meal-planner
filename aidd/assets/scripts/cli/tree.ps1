# tree-win.ps1
# Generate directory tree structure with configurable options
# Usage: .\tree-win.ps1 [OPTIONS]

param(
    [string]$ScanDir = ".",
    [string]$OutputFile = "",
    [string]$Exclude = "node_modules|coverage|dist|build|archives|documentations|public|llm.txt|.git|.repomix|aidd",
    [switch]$Help
)

$DEFAULT_EXCLUDE = "node_modules|coverage|dist|build|archives|documentations|public|llm.txt|.git|.repomix|aidd"

function Show-Help {
    Write-Host @"
Usage: .\tree-win.ps1 [OPTIONS]

Generate a directory tree structure and save it to a file.

OPTIONS:
  -ScanDir <path>        Directory to scan (default: current directory)
  -OutputFile <path>     Output file path (default: docs/tree.txt)
  -Exclude <pattern>     Exclude patterns separated by | (default: $DEFAULT_EXCLUDE)
  -Help                  Show this help message

EXAMPLES:
  .\tree-win.ps1 -ScanDir "C:\path\to\project" -OutputFile "project-tree.txt"
  .\tree-win.ps1 -ScanDir "." -Exclude "node_modules|dist|build"
  .\tree-win.ps1 -OutputFile "custom-tree.txt"

"@
}

function Get-DirectoryTree {
    param(
        [string]$Path,
        [string[]]$ExcludePatterns,
        [string]$Prefix = "",
        [bool]$IsLast = $true
    )
    
    $items = Get-ChildItem -Path $Path -Force -ErrorAction SilentlyContinue | Where-Object {
        $item = $_
        $shouldInclude = $true
        
        foreach ($pattern in $ExcludePatterns) {
            if ($item.Name -match $pattern) {
                $shouldInclude = $false
                break
            }
        }
        
        $shouldInclude
    } | Sort-Object { $_.PSIsContainer }, Name
    
    $output = @()
    $count = $items.Count
    
    for ($i = 0; $i -lt $count; $i++) {
        $item = $items[$i]
        $isLast = ($i -eq ($count - 1))
        
        $connector = if ($isLast) { "└── " } else { "├── " }
        $output += "$Prefix$connector$($item.Name)"
        
        if ($item.PSIsContainer) {
            $newPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix│   " }
            $output += Get-DirectoryTree -Path $item.FullName -ExcludePatterns $ExcludePatterns -Prefix $newPrefix -IsLast $isLast
        }
    }
    
    return $output
}

function Generate-Tree {
    # Set default output file if not provided
    if ([string]::IsNullOrEmpty($OutputFile)) {
        $OutputFile = "docs\tree.txt"
    }
    
    # Create output directory if it doesn't exist
    $outputDir = Split-Path -Parent $OutputFile
    if (![string]::IsNullOrEmpty($outputDir) -and !(Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
    
    # Convert scan directory to absolute path
    $scanPath = Resolve-Path $ScanDir -ErrorAction SilentlyContinue
    if (!$scanPath) {
        Write-Error "Directory not found: $ScanDir"
        exit 1
    }
    
    Write-Host "Generating tree for: $scanPath"
    Write-Host "Output file: $OutputFile"
    Write-Host "Exclude patterns: $Exclude"
    
    # Parse exclude patterns
    $excludePatterns = $Exclude -split '\|'
    
    # Generate tree
    $tree = @()
    $tree += Split-Path -Leaf $scanPath
    $tree += Get-DirectoryTree -Path $scanPath -ExcludePatterns $excludePatterns
    
    # Write to file
    $tree | Out-File -FilePath $OutputFile -Encoding UTF8
    
    Write-Host "Tree generated successfully: $OutputFile"
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

Generate-Tree
