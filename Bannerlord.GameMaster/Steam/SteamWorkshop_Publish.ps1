$workshopPublishExecutable = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.SteamWorkshop.exe" 
$workshopConfig = "WorkshopCreate.xml"


if (-not (Test-Path $workshopPublishExecutable)) 
{
    Write-Host "Unable to find TaleWorlds.MountAndBlade.SteamWorkshop.exe at: $workshopPublishExecutable"
    return
}

if (-not (Test-Path $workshopConfig)) 
{
    Write-Host "Unable to find WorkshopCreate.xml at: $workshopConfig"
    return
}

& $workshopPublishExecutable $workshopConfig
Write-Host "Module published to Steam Workshop"