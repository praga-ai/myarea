# Mobile App - Phase 1 Deployment Guide

## Overview
This guide walks through deploying the backend infrastructure (Azure SQL + Functions) and configuring the mobile app.

## Prerequisites
- Azure subscription
- Azure CLI installed
- .NET 8.0 SDK
- Git

---

## 1. Infrastructure Deployment

### 1.1 Set up resource group (if not exists)

```bash
# Set variables
$resourceGroup = "rg-mobileapp-cs"
$location = "eastus"  # Change as needed
$environment = "dev"
$sqlAdminLogin = "sqladmin"
$sqlAdminPassword = "P@ssw0rd123!"  # Change to strong password

# Create resource group
az group create --name $resourceGroup --location $location
```

### 1.2 Deploy infrastructure with Bicep

```bash
# Navigate to infra directory
cd E:\repo\app\infra

# Deploy main.bicep
az deployment group create `
  --resource-group $resourceGroup `
  --template-file main.bicep `
  --parameters `
    environment=$environment `
    sqlAdminLogin=$sqlAdminLogin `
    sqlAdminPassword=$sqlAdminPassword `
  --output table
```

**Expected Output:**
- SQL Server: `sql-mobileapp-cs-dev.database.windows.net`
- Function App: `func-mobileapp-cs-dev.azurewebsites.net`
- Key Vault: `kv-mobileapp-cs-dev`

---

## 2. Database Setup

### 2.1 Connect to Azure SQL and run schema creation

```bash
# Get SQL server name from deployment output
$sqlServer = "sql-mobileapp-cs-dev.database.windows.net"
$database = "db-mobileapp"

# Method 1: Using Azure Data Studio or SQL Server Management Studio
# - Connect to: Server=tcp:$sqlServer,1433
# - Database: $database
# - Authentication: SQL Login
# - Run: api/Scripts/01_CreateTables.sql

# Method 2: Using sqlcmd
sqlcmd -S tcp:$sqlServer,1433 -U sqladmin -P "P@ssw0rd123!" -d $database -i "api/Scripts/01_CreateTables.sql"
```

### 2.2 Verify tables created

```sql
-- Check tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Expected tables:
-- - Ward
-- - Part
-- - Area
-- - Street
-- - Survey
```

---

## 3. Azure Functions Configuration

### 3.1 Update local.settings.json for local testing

Copy `api/local.settings.template.json` to `api/local.settings.json` and update:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SqlConnectionString": "Server=tcp:sql-mobileapp-cs-dev.database.windows.net,1433;Database=db-mobileapp;User ID=sqladmin;Password=<YOUR_PASSWORD>;Encrypt=True;"
  }
}
```

### 3.2 Test locally

```bash
cd E:\repo\app\api

# Restore packages
dotnet restore

# Run locally
func start
```

You should see endpoints:
- `GET http://localhost:7071/api/health` ✅
- `GET http://localhost:7071/api/wards` 
- `GET http://localhost:7071/api/parts/{wardId}`
- `GET http://localhost:7071/api/areas/{partId}`
- `GET http://localhost:7071/api/streets/{areaId}`
- `POST http://localhost:7071/api/survey`
- `GET http://localhost:7071/api/surveys`

### 3.3 Publish to Azure

```bash
# Navigate to API folder
cd E:\repo\app\api

# Build release
dotnet build --configuration Release

# Publish to Azure Functions
func azure functionapp publish func-mobileapp-cs-dev --build remote
```

---

## 4. Configure Mobile App

### 4.1 Update API endpoint in mobile app

After Azure Functions deployment, update the API URL in the mobile app:

**File:** `mobile/src/services/ApiClient.ts`

```typescript
const BASE_URL = 'https://func-mobileapp-cs-dev.azurewebsites.net/api';
```

Change to your deployed function app URL.

### 4.2 Rebuild mobile app

```bash
cd E:\repo\app\mobile

# Update dependencies
npm install

# Build new APK
eas build --platform android --wait
```

---

## 5. Verify Everything Works

### 5.1 Test API endpoints

```bash
# Test health endpoint
curl https://func-mobileapp-cs-dev.azurewebsites.net/api/health

# Test wards endpoint
curl https://func-mobileapp-cs-dev.azurewebsites.net/api/wards

# Expected response:
[
  {"wardId": 1, "wardName": "Ward A", "createdDate": "2026-06-08T..."},
  {"wardId": 2, "wardName": "Ward B", "createdDate": "2026-06-08T..."},
  ...
]
```

### 5.2 Test mobile app

1. Install the APK on emulator: `adb install <path-to-apk>`
2. Open the app and test:
   - **Dashboard:** Should load surveys (empty at first)
   - **Survey:** Select Ward → Should load parts, then areas, then streets
   - **Master:** Should show all reference data
3. Submit a survey and verify it appears in Dashboard

---

## 6. Troubleshooting

### Connection String Issues
If you get "SqlConnectionString not configured" error:
- Verify Key Vault access policy allows Function App managed identity
- Run: `az keyvault secret show --vault-name kv-mobileapp-cs-dev --name SqlConnectionString`

### Database Not Reachable
- Check SQL firewall rules allow Azure Services
- Verify admin credentials are correct
- Test connection with: `sqlcmd -S tcp:SERVER.database.windows.net,1433 -U user -P password`

### Functions Not Starting
- Check .NET 8.0 is installed: `dotnet --version`
- Clear local cache: `func settings clear`
- Delete local.settings.json and rebuild

---

## 7. Next Steps (Phase 2)

- Add authentication (Azure AD / Custom Auth)
- Add data validation and error handling
- Create API logging and monitoring
- Set up CI/CD pipeline
- Add more master data tables as needed
