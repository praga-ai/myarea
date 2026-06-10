# Phase 1 Quick Start - 5 Steps to Production

## 🚀 Get Everything Running in 30 Minutes

### Step 1: Login to Azure
```powershell
az login
```

### Step 2: Run Complete Deployment
```powershell
cd E:\repo\app\scripts

# Deploy everything (infrastructure, database, API)
.\Setup-Complete.ps1 `
  -ResourceGroup "rg-mobileapp-cs" `
  -Location "eastus" `
  -Environment "dev" `
  -SqlAdminLogin "sqladmin" `
  -SqlAdminPassword "YourStrongPassword123!"
```

**⏱️ Expect:** 10-15 minutes for Azure to provision resources

### Step 3: Verify Deployment
Once deployment completes, test the API:

```powershell
# Test health endpoint
$url = "https://func-mobileapp-cs-dev.azurewebsites.net/api/health"
Invoke-WebRequest -Uri $url | Select-Object -ExpandProperty Content

# Test wards endpoint
$url = "https://func-mobileapp-cs-dev.azurewebsites.net/api/wards"
Invoke-WebRequest -Uri $url | Select-Object -ExpandProperty Content | ConvertFrom-Json | Format-Table
```

### Step 4: Update Mobile App URL
**File:** `mobile/src/services/ApiClient.ts` (already correct if using default deployment)

```typescript
const BASE_URL = 'https://func-mobileapp-cs-dev.azurewebsites.net/api';
```

### Step 5: Build & Install APK
```powershell
cd E:\repo\app\mobile

# Install dependencies
npm install

# Build APK with EAS
eas build --platform android --wait

# Once built, install on emulator
adb install <path-to-apk>
```

---

## ✅ Verify Everything Works

### In the Mobile App
1. Open **Dashboard** tab
   - Should show: "Total Surveys: 0" (or count from previous submissions)
   
2. Open **Survey** tab
   - Select a Ward from dropdown
   - Select a Part (should auto-populate)
   - Select an Area (should auto-populate)
   - Select a Street (should auto-populate)
   - Enter House Number
   - Click "Submit Survey"
   - Should see success message
   
3. Go back to **Dashboard**
   - Your new survey should appear

4. Open **Master** tab
   - Should show all wards, parts, areas, streets

---

## 🆘 Troubleshooting

### "SqlConnectionString not configured"
**Solution:** The SQL connection string needs to be in Azure Key Vault
```powershell
# Check if secret exists
az keyvault secret show `
  --vault-name kv-mobileapp-cs-dev `
  --name SqlConnectionString
```

### "Cannot connect to database"
**Solution:** Check SQL firewall rules
```powershell
# List firewall rules
az sql server firewall-rule list `
  --resource-group rg-mobileapp-cs `
  --server sql-mobileapp-cs-dev
```

### API returns 500 error
**Solution:** Check Function App logs
```powershell
# Stream live logs
az functionapp log tail `
  --name func-mobileapp-cs-dev `
  --resource-group rg-mobileapp-cs
```

### Mobile app says "Could not reach API"
**Solution:** Verify endpoint URL
- Check function app is running: `az functionapp show --name func-mobileapp-cs-dev --resource-group rg-mobileapp-cs`
- Test URL manually in browser or Postman
- Ensure mobile has internet access

---

## 📊 What Was Deployed

### Backend
- **Azure SQL Database** - Stores Ward, Part, Area, Street, and Survey data
- **Azure Functions** - 6 API endpoints for CRUD operations
- **Azure Key Vault** - Secure storage for database connection string

### Frontend
- **Dashboard** - View all submitted surveys
- **Survey** - Form with cascading dropdowns to submit new survey
- **Master Data** - View all reference data

### Data
- **Sample Data** - Pre-populated with 3 wards, 5 parts, 5 areas, 5 streets

---

## 🎯 Architecture

```
Mobile App (React Native)
    ↓ HTTP Requests
Azure Functions (C# .NET 8)
    ↓ SQL Queries
Azure SQL Database
    ↓ Secure Secrets
Azure Key Vault
```

---

## 📝 Next Phase: Phase 2

- [ ] Authentication (Azure AD or Custom)
- [ ] Input validation and error handling
- [ ] API logging and monitoring
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] More master data tables
- [ ] Edit/Delete survey functionality

---

**Need detailed setup?** Read [`DEPLOYMENT.md`](DEPLOYMENT.md)

**Questions?** Check the Azure docs:
- [Azure Functions](https://docs.microsoft.com/azure/azure-functions)
- [Azure SQL Database](https://docs.microsoft.com/azure/azure-sql/database)
- [Azure Key Vault](https://docs.microsoft.com/azure/key-vault)
