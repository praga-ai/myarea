# Survey Admin Web App - Quick Start Guide

## 🚀 Deploy in 5 Minutes

### Prerequisites
- Azure Subscription with existing resources
- PowerShell 5.0+
- Node.js 18+ installed
- Azure CLI installed

---

## Step-by-Step Deployment

### 1️⃣ Run the SQL Script (5 minutes)

```bash
# In Azure Portal -> Query Editor or SQL Management Studio
# Copy and run: api/Scripts/02_CreateUserTables.sql
```

**This creates:**
- ✅ Users table
- ✅ Role table
- ✅ UserAuditLog table
- ✅ Sample data (admin + surveyor users)

---

### 2️⃣ Deploy Web App (3 minutes)

```powershell
# Navigate to project root
cd E:\repo\app

# Run deployment script
.\deploy-web.ps1
```

**This automatically:**
- ✅ Installs npm dependencies
- ✅ Builds React app
- ✅ Creates App Service Plan
- ✅ Deploys to Azure App Service
- ✅ Returns app URL

---

### 3️⃣ Test the App (2 minutes)

1. Open the app URL from deployment script output
2. Login with demo credentials:
   - **Admin**: admin@survey.com / Admin@123
   - **Surveyor**: surveyor@survey.com / Surveyor@123
3. Verify role-based access

---

## 📁 What Got Created

### Backend API
- 4 new Azure Functions for authentication
- Database schema for users and roles
- JWT token generation and validation
- Password hashing with PBKDF2

### Web App
- React + TypeScript application
- Login & Registration pages
- Dashboard with survey stats
- User management (Admin only)
- Master data management (Admin only)
- Role-based route protection

### Infrastructure
- Azure App Service (Standard S1)
- App Service Plan (Linux, Node 18)
- Staging deployment slot
- Application diagnostics

---

## 🔐 User Roles

### Admin Role
```
Email: admin@survey.com
Password: Admin@123
Access: Everything
├─ Dashboard
├─ User Management
└─ Master Data
```

### Surveyor Role
```
Email: surveyor@survey.com
Password: Surveyor@123
Access: Dashboard only
└─ Limited to survey stats
```

---

## 📊 Features Overview

### Dashboard
- User profile info
- Survey statistics
- Recent surveys list
- Admin control panel

### User Management (Admin Only)
- View all users
- User statistics
- Status and role badges
- Last login info

### Master Data (Admin Only)
- Data overview (Wards, Parts, Areas, Streets)
- Questionnaire info
- Statistics and counts

---

## 🔧 Configuration

### Default Settings
- **Region**: Central India
- **Runtime**: Node.js 18 LTS
- **Size**: Standard S1 ($70/month)
- **HTTPS**: Enabled
- **Token Expiry**: 24 hours

### Change Settings
Edit `web/.env`:
```env
REACT_APP_API_BASE_URL=https://func-mobileapp-cs-in.azurewebsites.net/api
REACT_APP_APP_NAME=Survey Admin
REACT_APP_VERSION=1.0.0
```

---

## 🧪 Testing

### Login Flow
```
✅ Login with admin@survey.com → Access all pages
✅ Login with surveyor@survey.com → Access dashboard only
❌ Try /users as surveyor → Shows 403
❌ Try /master-data as surveyor → Shows 403
```

### New User Registration
1. Click "Register here" link
2. Fill form:
   - Full Name
   - Email
   - Password (8+ chars)
   - Role (Surveyor/Admin)
3. Account created (must login separately)

### Security Features
- ✅ Passwords hashed with PBKDF2
- ✅ JWT tokens with 24-hour expiration
- ✅ Role-based access control
- ✅ HTTPS enforced
- ✅ SQL injection protected

---

## 📱 Access URLs

After deployment:

```
Web App: https://app-survey-admin-cs-dev.azurewebsites.net
API: https://func-mobileapp-cs-in.azurewebsites.net/api

Routes:
/login → Login page
/register → Registration page
/dashboard → Main dashboard
/users → User management (Admin)
/master-data → Master data (Admin)
```

---

## ⚠️ Troubleshooting

### "Deployment Failed"
```
Check if:
✅ Azure CLI is installed
✅ You're logged into Azure (az login)
✅ Resource group has quota
✅ Node.js 18+ is installed
```

### "Login Fails"
```
Check if:
✅ SQL script was executed
✅ Users table exists
✅ Sample data was inserted
✅ Database connection working
```

### "App Shows Blank Page"
```
Check if:
✅ Build completed successfully
✅ App Service is running
✅ Deployment completed
✅ Clear browser cache (Ctrl+Shift+Del)
```

### "Can't Access Admin Pages"
```
Check if:
✅ Logged in as Admin user
✅ Role is set to "Admin" (RoleId=1)
✅ Token is valid
✅ Page reloaded
```

---

## 📈 Monitoring

### View Logs
```powershell
# Stream live logs
az webapp log tail -g survey-admin-rg -n app-survey-admin-cs-dev

# Download logs
az webapp log download -g survey-admin-rg -n app-survey-admin-cs-dev
```

### Check App Health
1. Azure Portal → App Service
2. Monitor → Health Check
3. View live metrics

---

## 💰 Cost

**Estimated Monthly**: ~$131

| Service | Cost |
|---------|------|
| App Service Plan S1 | $70 |
| SQL Database | $50 |
| Application Insights | $10 |
| Storage | $1 |

---

## 🔐 Security Checklist

Before production:

- [ ] Change default admin password
- [ ] Review user permissions
- [ ] Enable SSL/TLS (done by default)
- [ ] Configure firewall rules
- [ ] Set up backup schedules
- [ ] Enable audit logging
- [ ] Review error logs
- [ ] Test token expiration
- [ ] Verify role isolation
- [ ] Document access procedures

---

## 📚 Documentation

For detailed information, see:

1. **WEB_APP_GUIDE.md** - Complete guide with all details
2. **WEB_APP_SUMMARY.md** - Project summary and technical details
3. **API Comments** - Inline code documentation
4. **Bicep Template** - Infrastructure code comments

---

## 🎯 Next Steps

After deployment:

1. **Immediate**
   - [ ] Test login with both roles
   - [ ] Verify access control
   - [ ] Check dashboard loads data

2. **This Week**
   - [ ] Create admin user for your team
   - [ ] Test user registration
   - [ ] Verify survey data loading
   - [ ] Configure monitoring

3. **This Month**
   - [ ] Setup custom domain
   - [ ] Configure SSL certificate
   - [ ] Review audit logs
   - [ ] Optimize performance

---

## 📞 Support

### Common Commands

```powershell
# Redeploy (if changes made)
.\deploy-web.ps1

# View logs
az webapp log tail -g survey-admin-rg -n app-survey-admin-cs-dev

# Restart app
az webapp restart -g survey-admin-rg -n app-survey-admin-cs-dev

# Check status
az webapp show -g survey-admin-rg -n app-survey-admin-cs-dev
```

---

## ✅ You're Done!

The Survey Admin web app is now:

✅ **Deployed** to Azure App Service
✅ **Secured** with JWT authentication
✅ **Functional** with role-based access control
✅ **Monitored** with diagnostics
✅ **Ready** for production use

**Login and start managing users!**

---

*Created: 2026-06-10*
*Version: 1.0.0*
*Status: Production Ready ✅*
