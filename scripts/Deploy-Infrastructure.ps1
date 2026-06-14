#!/usr/bin/env pwsh
<#
.SYNOPSIS
Automated deployment script for Mobile App infrastructure on Azure

.DESCRIPTION
Deploys Azure SQL Database, Key Vault, and Azure Functions using Bicep templates

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
.\Deploy-Infrastructure.ps1 -ResourceGroup "rg-mobileapp-cs" -Environment "dev" -SqlAdminLogin "sqladmin" -SqlAdminPassword "P@ssw0rd123!"
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

Write-Host "=== Mobile App Infrastructure Deployment ===" -ForegroundColor Cyan

# Check prerequisites
Write-Host "`nChecking prerequisites..." -ForegroundColor Yellow
$azCliVersion = az --version 2>$null
if (!$azCliVersion) {
    Write-Host "ERROR: Azure CLI not installed" -ForegroundColor Red
    exit 1
}

# Verify logged in to Azure
$currentAccount = az account show 2>$null
if (!$currentAccount) {
    Write-Host "ERROR: Not logged in to Azure. Run 'az login'" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Azure CLI ready" -ForegroundColor Green

# Create resource group if not exists
Write-Host "`nChecking resource group: $ResourceGroup" -ForegroundColor Yellow
$rg = az group exists --name $ResourceGroup 2>$null
if ($rg -eq "false") {
    Write-Host "Creating resource group..." -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location | Out-Null
    Write-Host "✓ Resource group created" -ForegroundColor Green
}
else {
    Write-Host "✓ Resource group exists" -ForegroundColor Green
}

# Deploy infrastructure
Write-Host "`nDeploying infrastructure with Bicep..." -ForegroundColor Yellow
$infraPath = Join-Path (Split-Path $PSScriptRoot) "infra\main.bicep"

$deployment = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file $infraPath `
    --parameters `
    environment=$Environment `
    sqlAdminLogin=$SqlAdminLogin `
    sqlAdminPassword=$SqlAdminPassword `
    --output json 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Deployment failed" -ForegroundColor Red
    Write-Host $deployment -ForegroundColor Red
    exit 1
}

Write-Host "✓ Infrastructure deployed successfully" -ForegroundColor Green

# Extract outputs
$deploymentObj = $deployment | ConvertFrom-Json
$functionAppUrl = $deploymentObj.properties.outputs.functionAppUrl.value

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Function App URL: $functionAppUrl" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Run database setup: .\Deploy-Database.ps1 -ResourceGroup $ResourceGroup -Environment $Environment"
Write-Host "2. Update mobile app with function URL"
Write-Host "3. Deploy API functions: func azure functionapp publish func-mobileapp-cs-$environment --build remote"
