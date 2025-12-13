# PowerShell script to disable Bannerlord safe mode prompt before debugging
# This modifies the engine_config.txt file to set safely_exited = 1

$configPath = "$env:USERPROFILE\Documents\Mount and Blade II Bannerlord\Configs\engine_config.txt"

if (Test-Path $configPath) {
    Write-Host "Found engine_config.txt at: $configPath"
    
    # Read the file content
    $content = Get-Content $configPath -Raw
    
    # Replace safely_exited = 0 with safely_exited = 1
    if ($content -match 'safely_exited\s*=\s*0') {
        $content = $content -replace 'safely_exited\s*=\s*0', 'safely_exited = 1'
        
        # Write the modified content back
        Set-Content -Path $configPath -Value $content -NoNewline
        Write-Host "Successfully set safely_exited = 1"
    }
    elseif ($content -match 'safely_exited\s*=\s*1') {
        Write-Host "safely_exited is already set to 1"
    }
    else {
        Write-Warning "Could not find safely_exited setting in engine_config.txt"
    }
}
else {
    Write-Warning "engine_config.txt not found at: $configPath"
    Write-Warning "The file may not exist yet if you haven't launched Bannerlord"
}