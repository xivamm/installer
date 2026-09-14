# =====================================================================
# TechInstaller - Custom Post-Install Automation Script (custom.ps1)
# =====================================================================
# Dito mo ilalagay ang sarili mong mga PowerShell code o command.
# Awtomatikong magbubukas ang PowerShell bilang Administrator
# at tatakbuhin ang kahit anong code na ilalagay mo dito.
# =====================================================================

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "   TechInstaller Custom PowerShell Script Running    " -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# [Halimbawa 1] I-set ang High Performance Power Plan
# powercfg -setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
# Write-Host "Power plan set to High Performance." -ForegroundColor Green

# [Halimbawa 2] I-run ang iyong paboritong online command
# Write-Host "Running custom online command..." -ForegroundColor Yellow
# irm https://get.activated.win | iex

# [Halimbawa 3] I-uninstall ang built-in bloatware (Xbox, Maps, etc.)
# Get-AppxPackage *bingweather* | Remove-AppxPackage

Write-Host ""
Write-Host "Kasalukuyang handa ang PowerShell console para sa iyong mga commands." -ForegroundColor Green
Write-Host "Pwede mong i-edit ang file na ito sa: scripts\custom.ps1" -ForegroundColor Gray
