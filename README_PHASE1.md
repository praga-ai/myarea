# Mobile App - Survey Collection Platform

A React Native mobile application for collecting survey data with cascading form dropdowns, backed by Azure Functions and SQL Database.

## 🎯 Project Overview

### Phase 1 Status: ✅ COMPLETE

**Dashboard** | **Survey Form** | **Master Data**
---|---|---
View all surveys | Cascading dropdowns | Reference data
Real-time updates | House number input | Section organization
Pull-to-refresh | Form validation | Data management

## 🚀 Quick Start (5 Steps)

### 1️⃣ Login to Azure
```powershell
az login
```

### 2️⃣ Run Deployment
```powershell
cd E:\repo\app\scripts
.\Setup-Complete.ps1 `
  -ResourceGroup "rg-mobileapp-cs" `
  -Location "eastus" `
  -Environment "dev" `
  -SqlAdminLogin "sqladmin" `
  -SqlAdminPassword "YourStrongPassword123!"
```

### 3️⃣ Verify APIs
```powershell
# Test health
$url = "https://func-mobileapp-cs-dev.azurewebsites.net/api/health"
Invoke-WebRequest -Uri $url | Select-Object -ExpandProperty Content

# Test wards
$url = "https://func-mobileapp-cs-dev.azurewebsites.net/api/wards"
(Invoke-WebRequest -Uri $url).Content | ConvertFrom-Json
```

### 4️⃣ Build Mobile App
```powershell
cd E:\repo\app\mobile
npm install
eas build --platform android --wait
```

### 5️⃣ Install & Test
```powershell
adb install <downloaded-apk>
# Open app and test Dashboard → Survey → Master
```

✅ **Done!** You now have a fully functional survey collection platform.

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [QUICK_START.md](QUICK_START.md) | 5-step deployment guide |
| [DEPLOYMENT.md](DEPLOYMENT.md) | Detailed deployment instructions |
| [PHASE1_SUMMARY.md](PHASE1_SUMMARY.md) | Complete implementation details |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Mobile App (React Native)               │
│  ┌──────────────┬──────────────┬──────────────────────────┐ │
│  │  Dashboard   │    Survey    │    Master Data           │ │
│  │              │              │                          │ │
│  │ View surveys │ Form with    │ View all reference data  │ │
│  │              │ cascading    │                          │ │
│  │              │ dropdowns    │                          │ │
│  └──────────────┴──────────────┴──────────────────────────┘ │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTP REST
        ┌──────────────┴──────────────┐
        │   Azure Functions (C# .NET)  │
        │  ┌─────────────────────────┐ │
        │  │ 6 REST API Endpoints    │ │
        │  └─────────────────────────┘ │
        └──────────────┬───────────────┘
                       │ SQL Queries
        ┌──────────────┴───────────────┐
        │   Azure SQL Database         │
        │  ┌─────────────────────────┐ │
        │  │ 5 Tables with relations │ │
        │  │ Ward → Part → Area →    │ │
        │  │ Street → Survey         │ │
        │  └─────────────────────────┘ │
        └──────────────────────────────┘
```

---

## 📦 What's Included

### Backend (C# .NET 8)
- 6 REST API endpoints
- SQL database service layer
- Azure Functions isolated worker
- Key Vault integration

### Frontend (React Native + TypeScript)
- 3 main screens (Dashboard, Survey, Master Data)
- Bottom tab navigation
- Cascading dropdown logic
- Real-time data loading

### Infrastructure (Bicep)
- Azure SQL Database setup
- Azure Functions configuration
- Azure Key Vault for secrets
- Networking & security rules

### Deployment Scripts
- One-command deployment (`Setup-Complete.ps1`)
- Infrastructure deployment
- Database schema setup

---

## ✅ What's Ready for Testing

- ✅ Backend API (6 endpoints)
- ✅ Mobile App (3 screens)
- ✅ Database (schema + sample data)
- ✅ Deployment automation
- ✅ Documentation

---

## 🚀 Get Started Now

### **Option 1: Full Automated Deployment (Recommended)**
```powershell
.\scripts\Setup-Complete.ps1 `
  -ResourceGroup "rg-mobileapp-cs" `
  -Environment "dev" `
  -SqlAdminLogin "sqladmin" `
  -SqlAdminPassword "YourPassword123!"
```

### **Option 2: Step-by-Step Instructions**
1. Read [QUICK_START.md](QUICK_START.md)
2. Follow 5 simple steps
3. Get app running in 30 minutes

### **Option 3: Detailed Setup**
1. Read [DEPLOYMENT.md](DEPLOYMENT.md)
2. Follow detailed instructions for each component
3. Deploy with full control

---

## 📊 What You Get

### After Deployment:
- ✅ SQL Database with 5 related tables
- ✅ 6 REST API endpoints running
- ✅ Mobile app with 3 functional pages
- ✅ Pre-populated sample data
- ✅ Ready-to-use infrastructure

### Testing Capabilities:
- ✅ Test API endpoints with Postman
- ✅ Test form cascading on mobile
- ✅ Submit surveys and view in dashboard
- ✅ View master data

---

## 🔐 Security Built-In

- ✅ SQL parameterization (SQL injection protection)
- ✅ Secrets in Key Vault (no hardcoded credentials)
- ✅ HTTPS endpoints
- ✅ Database firewall rules
- ✅ Managed Identity authentication

---

## 🎯 Next Steps

1. **Right now:** Run the deployment script
2. **After 20 mins:** Verify everything works
3. **Then:** Test the mobile app
4. **Finally:** Plan Phase 2 features

---

## 📖 Full Documentation Index

| File | Content |
|------|---------|
| [QUICK_START.md](QUICK_START.md) | 5-step quick start guide |
| [DEPLOYMENT.md](DEPLOYMENT.md) | Comprehensive deployment guide |
| [PHASE1_SUMMARY.md](PHASE1_SUMMARY.md) | Technical implementation details |
| [QUICK_START.md](QUICK_START.md) | Troubleshooting guide |

---

## ✨ Highlights

🎉 **Everything Works Out of the Box**
- Pre-configured infrastructure
- Sample data included
- Automated deployment

🚀 **Production Ready**
- Security best practices
- Error handling throughout
- Async operations
- Database optimization

📱 **Mobile First Design**
- Responsive UI
- Smooth animations
- Loading states
- Error messages

---

**Start here:** [QUICK_START.md](QUICK_START.md) 👈

**Questions?** Check [DEPLOYMENT.md](DEPLOYMENT.md) for detailed help.
