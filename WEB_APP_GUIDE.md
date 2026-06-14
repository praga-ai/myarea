# Survey Admin Web App - Complete Setup Guide

## Overview

**Survey Admin** is a web-based administration portal for the Survey Mobile App. It provides user registration, authentication, and role-based access control for two user types:

### User Roles

| Role | Permissions | Access |
|------|-------------|--------|
| **Admin** | Full access to all features | Dashboard, User Management, Master Data |
| **Surveyor** | Submit surveys only | Dashboard |

---

## Architecture

### Technology Stack

- **Frontend**: React 18 + TypeScript
- **Backend**: Azure Functions (C# .NET 8)
- **Database**: Azure SQL Database
- **Authentication**: JWT Tokens
- **Deployment**: Azure App Service + Static Web Apps
- **Infrastructure**: Bicep IaC

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Survey Admin Web App                    │
├─────────────────────────────────────────────────────────────┤
│  Frontend (React)                                           │
│  ├── Login/Register Pages                                   │
│  ├── Dashboard                                              │
│  ├── User Management (Admin)                                │
│  └── Master Data (Admin)                                    │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTPS
                      ↓
┌─────────────────────────────────────────────────────────────┐
│              Azure Functions (API Gateway)                  │
├─────────────────────────────────────────────────────────────┤
│  Authentication Endpoints:                                  │
│  ├── POST   /api/auth/register    → RegisterFunction       │
│  ├── POST   /api/auth/login       → LoginFunction          │
│  ├── POST   /api/auth/verify-token→ VerifyTokenFunction    │
│  └── GET    /api/auth/users       → GetUsersFunction       │
│                                                             │
│  Survey Endpoints:                                          │
│  ├── GET    /api/surveys          → GetSurveysFunction     │
│  ├── POST   /api/survey           → CreateSurveyFunction   │
│  └── GET    /api/questionnaires   → GetQuestionnairesFunc. │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────────┐
│            Azure SQL Database (Central India)               │
├─────────────────────────────────────────────────────────────┤
│  Tables:                                                    │
│  ├── Users (UserId, Email, PasswordHash, RoleId)           │
│  ├── Role (RoleId, RoleName, Description)                  │
│  ├── Survey (SurveyId, WardId, PartId, AreaId, etc.)       │
│  ├── Questionnaire (QuestionnaireId, QuestionText, etc.)    │
│  ├── QuestionnaireOption (OptionId, OptionText, etc.)       │
│  └── UserAuditLog (AuditLogId, UserId, Action, etc.)       │
└─────────────────────────────────────────────────────────────┘
```

---

## Database Schema

### Users Table

```sql
CREATE TABLE [Users] (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FullName NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME DEFAULT GETUTCDATE(),
    LastLoginDate DATETIME NULL,
    FOREIGN KEY (RoleId) REFERENCES Role(RoleId)
);
```

### Role Table

```sql
CREATE TABLE Role (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME DEFAULT GETUTCDATE()
);
```

### UserAuditLog Table

```sql
CREATE TABLE UserAuditLog (
    AuditLogId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    Status NVARCHAR(50),
    Description NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES [Users](UserId)
);
```

---

## API Endpoints

### Authentication Endpoints

#### 1. Register User
```
POST /api/auth/register

Request Body:
{
  "email": "user@example.com",
  "fullName": "John Doe",
  "password": "SecurePass@123",
  "confirmPassword": "SecurePass@123",
  "roleId": 2
}

Response:
{
  "success": true,
  "message": "User registered successfully",
  "user": {
    "userId": 123,
    "email": "user@example.com",
    "fullName": "John Doe",
    "roleName": "Surveyor",
    "isActive": true,
    "lastLoginDate": null
  }
}
```

#### 2. Login User
```
POST /api/auth/login

Request Body:
{
  "email": "admin@survey.com",
  "password": "Admin@123"
}

Response:
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "email": "admin@survey.com",
    "fullName": "System Administrator",
    "roleName": "Admin",
    "isActive": true,
    "lastLoginDate": "2026-06-10T10:30:00Z"
  }
}
```

#### 3. Verify Token
```
POST /api/auth/verify-token

Request Body:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

Response:
{
  "valid": true,
  "user": {
    "userId": 1,
    "email": "admin@survey.com",
    "fullName": "System Administrator",
    "roleName": "Admin",
    "isActive": true,
    "lastLoginDate": "2026-06-10T10:30:00Z"
  }
}
```

#### 4. Get All Users (Admin Only)
```
GET /api/auth/users
Authorization: Bearer <JWT_TOKEN>

Response:
{
  "users": [
    {
      "userId": 1,
      "email": "admin@survey.com",
      "fullName": "System Administrator",
      "roleName": "Admin",
      "isActive": true,
      "lastLoginDate": "2026-06-10T10:30:00Z"
    },
    {
      "userId": 2,
      "email": "surveyor@survey.com",
      "fullName": "Survey Collector",
      "roleName": "Surveyor",
      "isActive": true,
      "lastLoginDate": "2026-06-09T15:45:00Z"
    }
  ],
  "total": 2
}
```

---

## Frontend Components

### File Structure

```
web/
├── src/
│   ├── components/
│   │   ├── Auth.css                    # Auth pages styling
│   │   ├── Dashboard.tsx               # Main dashboard
│   │   ├── Dashboard.css
│   │   ├── Login.tsx                   # Login page
│   │   ├── Register.tsx                # Registration page
│   │   ├── Users.tsx                   # User management (Admin)
│   │   ├── Users.css
│   │   ├── MasterData.tsx              # Master data management (Admin)
│   │   ├── MasterData.css
│   │   ├── ProtectedRoute.tsx          # Route protection
│   │   ├── Unauthorized.tsx            # Unauthorized access page
│   │   ├── NotFound.tsx                # 404 page
│   │   └── ErrorPages.css
│   ├── context/
│   │   └── AuthContext.tsx             # Authentication state & hooks
│   ├── App.tsx                         # Main app component with routing
│   ├── App.css
│   ├── index.tsx                       # App entry point
│   └── index.css
├── public/
│   └── index.html                      # HTML template
├── package.json                        # Dependencies
├── .env                                # Environment variables
└── tsconfig.json                       # TypeScript config
```

### Key Components

#### 1. **AuthContext** (`context/AuthContext.tsx`)
Manages authentication state globally:
- `user`: Current logged-in user
- `token`: JWT authentication token
- `isAuthenticated`: Auth status boolean
- `login(email, password)`: Login function
- `register(...)`: Registration function
- `logout()`: Logout function
- `hasRole(roleName)`: Check user role

#### 2. **ProtectedRoute** (`components/ProtectedRoute.tsx`)
Guards routes based on authentication and roles:
```tsx
<ProtectedRoute requiredRole="Admin">
  <AdminPage />
</ProtectedRoute>
```

#### 3. **Dashboard** (`components/Dashboard.tsx`)
Shows:
- User profile information
- Survey statistics
- List of recent surveys
- Admin control panel (if user is Admin)

#### 4. **Users** (`components/Users.tsx`)
Admin-only page for:
- Viewing all registered users
- User statistics (Admin/Surveyor count)
- User status and last login info

#### 5. **MasterData** (`components/MasterData.tsx`)
Admin-only page for:
- Master data overview (Wards, Parts, Areas, Streets)
- Questionnaire management info
- Statistics on data counts

---

## Deployment

### Prerequisites

- Azure Subscription
- Azure CLI installed
- Node.js 18+ installed
- npm or yarn package manager

### Step 1: Set Up Database Tables

Run the SQL script to create user management tables:

```bash
# In Azure SQL Query Editor or SQL Server Management Studio
sqlcmd -S server.database.windows.net -U admin -P password -d surveydb -i api/Scripts/02_CreateUserTables.sql

# Or copy and execute: api/Scripts/02_CreateUserTables.sql
```

### Step 2: Deploy Backend (Azure Functions)

The authentication functions are already created in:
- `api/Functions/RegisterFunction.cs`
- `api/Functions/LoginFunction.cs`
- `api/Functions/VerifyTokenFunction.cs`
- `api/Functions/GetUsersFunction.cs`

Publish to Azure Functions:
```bash
cd api
func azure functionapp publish func-mobileapp-cs-in
```

### Step 3: Build React App

```bash
cd web
npm install
npm run build
```

This creates an optimized production build in `web/build/`

### Step 4: Deploy to Azure App Service

#### Option A: Using Azure CLI

```bash
# Create resource group (if not exists)
az group create --name survey-admin-rg --location centralindia

# Deploy using Bicep
az deployment group create \
  --resource-group survey-admin-rg \
  --template-file bicep/web-app.bicep \
  --parameters bicep/web-app.parameters.json

# Deploy the build
az webapp deployment source config-zip \
  --resource-group survey-admin-rg \
  --name app-survey-admin-cs-dev \
  --src web.zip
```

#### Option B: Using Azure Portal

1. Go to Azure Portal
2. Create new App Service
3. Select Node.js 18 LTS runtime
4. Configure settings (App Service Plan: Standard S1)
5. Deploy via GitHub, Docker, or ZIP

#### Option C: Using Visual Studio Code

1. Install Azure App Service extension
2. Right-click on App Service in Explorer
3. Choose "Deploy to Web App"
4. Select the target App Service

### Step 5: Configure Environment Variables

In Azure App Service → Configuration → Application settings:

```
REACT_APP_API_BASE_URL: https://func-mobileapp-cs-in.azurewebsites.net/api
REACT_APP_APP_NAME: Survey Admin
REACT_APP_VERSION: 1.0.0
SCM_DO_BUILD_DURING_DEPLOYMENT: true
```

### Step 6: Enable HTTPS & Custom Domain (Optional)

```bash
# Add custom domain
az webapp config hostname add \
  --resource-group survey-admin-rg \
  --webapp-name app-survey-admin-cs-dev \
  --hostname survey-admin.yourdomain.com

# Enable HTTPS
az webapp update \
  --resource-group survey-admin-rg \
  --name app-survey-admin-cs-dev \
  --https-only true
```

---

## Testing

### Demo Credentials

```
Admin User:
  Email: admin@survey.com
  Password: Admin@123
  Role: Admin
  Access: Dashboard, Users, Master Data

Surveyor User:
  Email: surveyor@survey.com
  Password: Surveyor@123
  Role: Surveyor
  Access: Dashboard only
```

### Test Scenarios

#### 1. Login Flow
- [ ] Admin can login with correct credentials
- [ ] Surveyor can login with correct credentials
- [ ] Incorrect password shows error
- [ ] Non-existent email shows error
- [ ] Token is stored in localStorage after login

#### 2. Authorization
- [ ] Admin can access Dashboard ✓
- [ ] Admin can access Users page ✓
- [ ] Admin can access Master Data page ✓
- [ ] Surveyor can access Dashboard ✓
- [ ] Surveyor cannot access Users page (shows 403) ✓
- [ ] Surveyor cannot access Master Data page (shows 403) ✓

#### 3. Dashboard
- [ ] Displays correct user profile info
- [ ] Shows survey statistics
- [ ] Displays survey list with pagination (if implemented)
- [ ] Refresh button reloads data

#### 4. User Management (Admin)
- [ ] Admin can view all users
- [ ] Shows user count statistics
- [ ] Displays role and status badges
- [ ] Shows last login date

#### 5. Registration
- [ ] New user can register with valid data
- [ ] Password confirmation validation works
- [ ] Minimum password length enforced (8 chars)
- [ ] Email validation works
- [ ] User is created with correct role

#### 6. Logout
- [ ] User can logout
- [ ] Token is removed from localStorage
- [ ] User is redirected to login page
- [ ] Accessing protected routes redirects to login

---

## Security Considerations

### Password Security
- ✓ PBKDF2 with SHA256 hashing
- ✓ 10,000 iterations
- ✓ Minimum 8 characters required
- ✓ Passwords never stored in plain text

### Authentication
- ✓ JWT tokens with 24-hour expiration
- ✓ Tokens validated on protected routes
- ✓ Token stored securely in localStorage
- ✓ HTTPS enforced in production

### Database
- ✓ SQL injection prevention via parameterized queries
- ✓ Role-based access control (RBAC)
- ✓ Audit logging of user actions
- ✓ User status management (active/inactive)

### Frontend
- ✓ Protected routes with role checking
- ✓ Automatic logout on token expiration
- ✓ XSS protection via React's built-in escaping
- ✓ CSRF tokens for state-changing operations (if needed)

---

## Troubleshooting

### Issues & Solutions

#### 1. "Unauthorized" on login
**Problem**: User exists but login fails
**Solution**: 
- Verify user exists in database
- Check password hash is correctly stored
- Ensure SQL connection string is correct

#### 2. "Token Invalid" error
**Problem**: Valid token is rejected
**Solution**:
- Check token hasn't expired (24-hour TTL)
- Verify token wasn't corrupted in transit
- Check Authorization header format: `Bearer <token>`

#### 3. "Admin pages show 403"
**Problem**: Admin user can't access admin pages
**Solution**:
- Verify user has RoleId = 1 (Admin role)
- Check token contains correct role claim
- Reload page to refresh role info

#### 4. App doesn't load
**Problem**: Blank page or 404
**Solution**:
- Check App Service is running
- Verify build completed successfully
- Check Application settings in Azure
- Clear browser cache

#### 5. API calls return CORS error
**Problem**: Frontend can't call backend
**Solution**:
- Verify API endpoint URL is correct
- Check Azure Functions CORS settings
- Ensure HTTPS is used in production

---

## Performance Optimization

### Frontend
- ✓ Code splitting for lazy loading
- ✓ Minified production build
- ✓ Asset compression (gzip)
- ✓ Caching headers configured

### Backend
- ✓ Database connection pooling
- ✓ Async/await for non-blocking operations
- ✓ Query optimization with indexes
- ✓ Function warm-up for faster responses

### Database
- ✓ Indexes on frequently queried columns
- ✓ Serverless auto-pause enabled
- ✓ Proper normalization
- ✓ Partition strategy for large tables

---

## Monitoring & Logging

### Application Insights

Monitor performance and errors:

```bash
# Create Application Insights
az monitor app-insights component create \
  --app survey-admin-insights \
  --location centralindia \
  --resource-group survey-admin-rg \
  --application-type web
```

### Log Types

- **Access Logs**: HTTP requests, status codes, latencies
- **Application Logs**: Errors, warnings, info messages
- **Audit Logs**: User logins, role changes, data modifications

### View Logs

```bash
# Stream live logs
az webapp log tail --resource-group survey-admin-rg --name app-survey-admin-cs-dev

# Download log files
az webapp log download --resource-group survey-admin-rg --name app-survey-admin-cs-dev
```

---

## Cost Estimation

### Azure Services (Monthly)

| Service | SKU | Cost |
|---------|-----|------|
| App Service Plan | Standard S1 | $70.00 |
| SQL Database | Serverless, 1 vCore | ~$50.00 |
| Application Insights | 1 GB/month | ~$10.00 |
| Functions | Consumption | ~$5.00 |
| **Total** | | **~$135/month** |

*Prices are approximate and subject to region (Central India is cheaper)*

---

## Scaling

### Horizontal Scaling

```bash
# Scale up App Service Plan
az appservice plan update \
  --name asp-survey-admin-cs-dev \
  --resource-group survey-admin-rg \
  --sku P1V2
```

### Database Scaling

```bash
# Scale up SQL Database
az sql db update \
  --server mobileapp-db-cs \
  --name surveydb \
  --edition Standard \
  --service-objective S3
```

---

## Maintenance

### Regular Tasks

- [ ] Review user audit logs monthly
- [ ] Delete inactive users quarterly
- [ ] Update dependencies monthly
- [ ] Review and optimize database indexes quarterly
- [ ] Test disaster recovery procedures semi-annually
- [ ] Review security vulnerabilities quarterly

### Backup Strategy

- Automated daily backups (Azure SQL)
- Retention: 35 days
- Geo-redundant backups enabled

---

## Support & Documentation

- **API Docs**: See API Endpoints section above
- **Mobile App**: Refer to mobile app documentation
- **Database**: Refer to database schema documentation
- **Infrastructure**: Refer to Bicep template documentation

---

*Last Updated: 2026-06-10*
*Version: 1.0.0*
