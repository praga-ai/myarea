# Local Development Setup

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- SQLite3 (for database)
- Azure Functions Core Tools

## Database Setup (SQLite)

The project uses a local SQLite database for development to avoid Azure SQL dependencies.

### Initialize Database

```bash
cd api/Scripts
./init-sqlite-db.ps1
```

This will create `api/app.db` with:
- All required tables (Ward, Part, Area, Street, Questionnaire, etc.)
- Sample survey data for testing
- Sample questionnaires with options

### Database Location

- **File**: `api/app.db`
- **Connection String**: `Data Source=./app.db;`
- **Configured in**: `api/local.settings.json`

## Running the API Locally

```bash
cd api
func start
```

The API will start on `http://localhost:7071`

### Available Endpoints

- `POST /api/auth/login` - Login (demo: surveyor@myarea.com / Surveyor@123)
- `GET /api/surveys` - Get available surveys
- `POST /api/surveys` - Submit survey response
- `GET /api/wards` - Get wards
- `GET /api/parts` - Get parts for a ward
- `GET /api/areas` - Get areas for a part
- `GET /api/streets` - Get streets for an area
- `GET /api/questionnaires` - Get survey questions

## Running the Web Admin Dashboard

```bash
cd survey-admin-web
npm install
npm start
```

Runs on `http://localhost:3000`

## Running the Mobile App

```bash
cd mobile
npm install

# iOS (macOS only)
npm run ios

# Android
npm run android

# Or build APK for deployment
npm run build:android
```

## Test Data

### Survey Hierarchy
```
Ward A
├── Part P001
│   ├── Area 1
│   │   ├── Main Street
│   │   └── Oak Street
│   └── Area 2
└── Part P002
    └── Area 3
        ├── Elm Street
        └── Pine Street
```

### Sample Questionnaire
1. Water supply satisfaction (Radio)
2. Street lighting satisfaction (Radio)
3. Road condition (Radio)
4. Waste disposal facility (Radio)
5. Healthcare facility rating (Radio)

## Troubleshooting

### "app.db not found"
Run the SQLite initialization script:
```bash
./api/Scripts/init-sqlite-db.ps1
```

### API won't start
- Ensure Azure Functions Core Tools is installed
- Check that port 7071 is not in use
- Verify `local.settings.json` has correct SqlConnectionString

### Mobile app won't connect to API
- Ensure API is running on `http://localhost:7071`
- On Android emulator, use `http://10.0.2.2:7071` instead
- Check firewall settings

## Production Deployment

For production, the application uses:
- **Database**: Azure SQL Server
- **Web API**: Azure Functions
- **Admin Dashboard**: Azure App Service
- **Mobile**: GitHub Actions builds APK/IPA artifacts

See `.github/workflows/` and `bicep/` for deployment configuration.
