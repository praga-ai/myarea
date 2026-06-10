#!/usr/bin/env pwsh
<#
.SYNOPSIS
Deploy database schema to Azure SQL Database

.DESCRIPTION
Runs SQL migration scripts to create tables and seed data

.PARAMETER ResourceGroup
The Azure resource group name

.PARAMETER Environment
Environment name (dev, staging, prod)

.PARAMETER SqlAdminLogin
SQL Server administrator login

.PARAMETER SqlAdminPassword
SQL Server administrator password

.EXAMPLE
.\Deploy-Database.ps1 -ResourceGroup "rg-mobileapp-cs" -Environment "dev" -SqlAdminLogin "sqladmin" -SqlAdminPassword "P@ssw0rd123!"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true)]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$SqlAdminLogin,

    [Parameter(Mandatory = $true)]
    [string]$SqlAdminPassword
)

$ErrorActionPreference = "Stop"

Write-Host "=== Mobile App Database Deployment ===" -ForegroundColor Cyan

# Get SQL server details from Azure
Write-Host "`nRetrieving SQL server details..." -ForegroundColor Yellow
$sqlServerName = "sql-mobileapp-cs-$Environment"
$sqlServer = az sql server list --resource-group $ResourceGroup --query "[?name=='$sqlServerName']" --output json | ConvertFrom-Json

if (!$sqlServer -or $sqlServer.Count -eq 0) {
    Write-Host "ERROR: SQL Server not found. Run infrastructure deployment first." -ForegroundColor Red
    exit 1
}

$fullyQualifiedDomainName = $sqlServer[0].fullyQualifiedDomainName
Write-Host "✓ Found SQL Server: $fullyQualifiedDomainName" -ForegroundColor Green

# Build connection string
$connectionString = "Server=tcp:$fullyQualifiedDomainName,1433;Database=db-mobileapp;User ID=$SqlAdminLogin;Password=$SqlAdminPassword;Encrypt=True;Connection Timeout=30;"

# Test connection
Write-Host "`nTesting SQL connection..." -ForegroundColor Yellow
try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connectionString
    $sqlConnection.Open()
    $sqlConnection.Close()
    Write-Host "✓ Connection successful" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to connect to SQL Server" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Run schema creation script
Write-Host "`nDeploying database schema..." -ForegroundColor Yellow
$scriptPath = Join-Path (Split-Path $PSScriptRoot) "api\Scripts\01_CreateTables.sql"

if (!(Test-Path $scriptPath)) {
    Write-Host "ERROR: SQL script not found at $scriptPath" -ForegroundColor Red
    exit 1
}

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connectionString
    $sqlConnection.Open()

    $sqlCommand = $sqlConnection.CreateCommand()
    $scriptContent = Get-Content $scriptPath -Raw
    $sqlCommand.CommandText = $scriptContent
    $sqlCommand.CommandTimeout = 300

    $sqlCommand.ExecuteNonQuery() | Out-Null
    $sqlConnection.Close()

    Write-Host "✓ Database schema created successfully" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to execute database schema" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Verify tables
Write-Host "`nVerifying tables..." -ForegroundColor Yellow
try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connectionString
    $sqlConnection.Open()

    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
    $reader = $sqlCommand.ExecuteReader()

    $tables = @()
    while ($reader.Read()) {
        $tables += $reader["TABLE_NAME"]
    }
    $reader.Close()
    $sqlConnection.Close()

    if ($tables.Count -gt 0) {
        Write-Host "✓ Tables created:" -ForegroundColor Green
        $tables | ForEach-Object { Write-Host "  - $_" }
    }
    else {
        Write-Host "WARNING: No tables found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "WARNING: Could not verify tables" -ForegroundColor Yellow
    Write-Host $_.Exception.Message
}

Write-Host "`n=== Database Deployment Complete ===" -ForegroundColor Green
Write-Host "SQL Server: $fullyQualifiedDomainName" -ForegroundColor Cyan
Write-Host "Database: db-mobileapp" -ForegroundColor Cyan
Write-Host "`nNext: Publish Azure Functions" -ForegroundColor Yellow
