#!/usr/bin/env pwsh
<#
.SYNOPSIS
Complete setup automation for Mobile App deployment

.DESCRIPTION
Runs all deployment steps in sequence: Infrastructure, Database, and API Publishing

.PARAMETER ResourceGroup
The Azure resource group name

.PARAMETER Location
Azure region (default: eastus)

.PARAMETER Environment
Environment name (dev, staging, prod)

.PARAMETER SqlAdminLogin
SQL Server administrator login

.PARAMETER SqlAdminPassword
SQL Server administrator password

.EXAMPLE
.\Setup-Complete.ps1 -ResourceGroup "rg-mobileapp-cs" -Environment "dev" -SqlAdminLogin "sqladmin" -SqlAdminPassword "ComplexP@ss123!"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $true)]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$SqlAdminLogin,

    [Parameter(Mandatory = $true)]
    [string]$SqlAdminPassword
)

$ErrorActionPreference = "Stop"
$startTime = Get-Date

Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║     Mobile App - Complete Deployment & Setup                  ║
║     Environment: $Environment (RG: $ResourceGroup)      ║
╚════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

# Step 1: Infrastructure
Write-Host "`n[STEP 1/3] Deploying Infrastructure..." -ForegroundColor Magenta
$infraScript = Join-Path $PSScriptRoot "Deploy-Infrastructure.ps1"
try {
    & $infraScript `
        -ResourceGroup $ResourceGroup `
        -Location $Location `
        -Environment $Environment `
        -SqlAdminLogin $SqlAdminLogin `
        -SqlAdminPassword $SqlAdminPassword
}
catch {
    Write-Host "ERROR: Infrastructure deployment failed" -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit 1
}

# Step 2: Database
Write-Host "`n[STEP 2/3] Deploying Database Schema..." -ForegroundColor Magenta
$dbScript = Join-Path $PSScriptRoot "Deploy-Database.ps1"
try {
    & $dbScript `
        -ResourceGroup $ResourceGroup `
        -Environment $Environment `
        -SqlAdminLogin $SqlAdminLogin `
        -SqlAdminPassword $SqlAdminPassword
}
catch {
    Write-Host "ERROR: Database deployment failed" -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit 1
}

# Step 3: API Publishing
Write-Host "`n[STEP 3/3] Publishing Azure Functions..." -ForegroundColor Magenta
$apiPath = Join-Path (Split-Path $PSScriptRoot) "api"
$functionAppName = "func-mobileapp-cs-$Environment"

Write-Host "`nBuilding API..." -ForegroundColor Yellow
Push-Location $apiPath
try {
    # Build
    dotnet build --configuration Release | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Build successful" -ForegroundColor Green

    # Publish to Azure
    Write-Host "`nPublishing to Azure Functions ($functionAppName)..." -ForegroundColor Yellow
    func azure functionapp publish $functionAppName --build remote

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Publishing failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Publishing successful" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: API deployment failed" -ForegroundColor Red
    Write-Host $_.Exception.Message
    exit 1
}
finally {
    Pop-Location
}

# Summary
$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host @"

╔════════════════════════════════════════════════════════════════╗
║     ✓ DEPLOYMENT COMPLETE                                     ║
╚════════════════════════════════════════════════════════════════╝

Duration: $([Math]::Round($duration, 2)) seconds

Resources Created:
  • SQL Server: sql-mobileapp-cs-$Environment.database.windows.net
  • Database: db-mobileapp (with tables: Ward, Part, Area, Street, Survey)
  • Function App: https://$functionAppName.azurewebsites.net/api
  • Key Vault: kv-mobileapp-cs-$Environment

API Endpoints Ready:
  ✓ GET  /api/health
  ✓ GET  /api/wards
  ✓ GET  /api/parts/{wardId}
  ✓ GET  /api/areas/{partId}
  ✓ GET  /api/streets/{areaId}
  ✓ POST /api/survey
  ✓ GET  /api/surveys

Next Steps:
  1. Update mobile app URL: 'https://$functionAppName.azurewebsites.net/api'
  2. Rebuild mobile app: npm install && eas build --platform android --wait
  3. Test on emulator or device

"@ -ForegroundColor Green
