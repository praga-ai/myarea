#!/usr/bin/env pwsh
# Initialize local SQLite database for development

param(
    [string]$DatabasePath = "$PSScriptRoot/../app.db"
)

Write-Host "🗄️  Initializing SQLite database for local development..."
Write-Host ""

# Check if database already exists
if (Test-Path $DatabasePath) {
    Write-Host "Database already exists at: $DatabasePath"
    Write-Host "To reinitialize, delete the file and run this script again."
    exit 0
}

# Install sqlite package if needed
try {
    # Try to use dotnet-sqlite via PowerShell
    $sqlitePath = "C:\Program Files\SQLite\sqlite3.exe"
    if (-not (Test-Path $sqlitePath)) {
        Write-Host "⚠️  sqlite3.exe not found. Attempting to install..."
        # Try to find it via chocolatey or other means
        $sqlite = Get-Command sqlite3 -ErrorAction SilentlyContinue
        if ($null -eq $sqlite) {
            Write-Host "❌ SQLite CLI not found. Please install SQLite first:"
            Write-Host "   Windows: choco install sqlite"
            Write-Host "   Or download from: https://www.sqlite.org/download.html"
            exit 1
        }
        $sqlitePath = $sqlite.Source
    }

    Write-Host "Using SQLite at: $sqlitePath"
    Write-Host ""

    # Create database from SQL script
    Write-Host "📝 Creating tables and inserting sample data..."
    & $sqlitePath $DatabasePath < "$PSScriptRoot/sqlite-init.sql"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database created successfully at: $DatabasePath"
        Write-Host ""
        Write-Host "📍 Connection string for local.settings.json:"
        Write-Host "   Data Source=$DatabasePath;"
        Write-Host ""
        Write-Host "Next steps:"
        Write-Host "1. Update api/local.settings.json with the SQLite connection string"
        Write-Host "2. Run the API locally: func start"
        Write-Host "3. The mobile app can connect to http://localhost:7071"
    } else {
        Write-Host "❌ Failed to create database"
        exit 1
    }
} catch {
    Write-Host "❌ Error: $_"
    exit 1
}
