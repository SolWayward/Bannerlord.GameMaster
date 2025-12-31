# Copy Test Results Script
# Automatically copies test results from game config to project directory with versioning

$ErrorActionPreference = "Stop"

try {
    # Define paths
    $sourceFile = Join-Path $env:USERPROFILE "Documents\Mount and Blade II Bannerlord\Configs\GameMaster\test-results.txt"
    $targetDir = Join-Path $PSScriptRoot "..\Bannerlord.GameMaster\Console\Testing\Results"
    
    # Check if source file exists
    if (-not (Test-Path $sourceFile)) {
        # Silent exit if source doesn't exist (no tests were run)
        exit 0
    }
    
    # Create target directory if it doesn't exist
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    # Get all existing result files
    $existingFiles = Get-ChildItem -Path $targetDir -Filter "Console_Test_Results_*.txt" -ErrorAction SilentlyContinue
    
    # Get source file hash for comparison
    $sourceHash = (Get-FileHash -Path $sourceFile -Algorithm MD5).Hash
    
    # Find most recent result file
    $mostRecentFile = $null
    if ($existingFiles.Count -gt 0) {
        # Sort by filename (which includes date and number) descending
        $mostRecentFile = $existingFiles | Sort-Object Name -Descending | Select-Object -First 1
    }
    
    # Compare with most recent result if exists
    $shouldCopy = $true
    if ($mostRecentFile) {
        $recentHash = (Get-FileHash -Path $mostRecentFile.FullName -Algorithm MD5).Hash
        if ($sourceHash -eq $recentHash) {
            # Content is identical, no need to copy
            $shouldCopy = $false
        }
    }
    
    if ($shouldCopy) {
        # Get today's date in yyyy-MM-dd format
        $today = Get-Date -Format "yyyy-MM-dd"
        
        # Find all files for today
        $todayPattern = "Console_Test_Results_${today}_*.txt"
        $todayFiles = Get-ChildItem -Path $targetDir -Filter $todayPattern -ErrorAction SilentlyContinue
        
        # Calculate next number
        $nextNumber = 1
        if ($todayFiles.Count -gt 0) {
            # Extract numbers from filenames and find max
            $numbers = $todayFiles | ForEach-Object {
                if ($_.Name -match "Console_Test_Results_${today}_(\d+)\.txt") {
                    [int]$matches[1]
                }
            }
            $nextNumber = ($numbers | Measure-Object -Maximum).Maximum + 1
        }
        
        # Format number with 3-digit zero padding
        $paddedNumber = $nextNumber.ToString("000")
        
        # Create target filename
        $targetFilename = "Console_Test_Results_${today}_${paddedNumber}.txt"
        $targetPath = Join-Path $targetDir $targetFilename
        
        # Copy file
        Copy-Item -Path $sourceFile -Destination $targetPath -Force
        
        # Silent success - no output
    }
    
    exit 0
}
catch {
    # Output error to stderr
    Write-Error "Failed to copy test results: $_"
    exit 1
}
