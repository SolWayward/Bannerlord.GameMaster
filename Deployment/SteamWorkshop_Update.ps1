$workshopPublishExecutable = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.SteamWorkshop.exe" 
$workshopConfig = "WorkshopUpdate.xml"


if (-not (Test-Path $workshopPublishExecutable)) 
{
    Write-Host "Unable to find TaleWorlds.MountAndBlade.SteamWorkshop.exe at: $workshopPublishExecutable"
    return
}

if (-not (Test-Path $workshopConfig)) 
{
    Write-Host "Unable to find WorkshopUpdate.xml at: $workshopConfig"
    return
}

& $workshopPublishExecutable $workshopConfig
Write-Host "Module updated on Steam Workshop"