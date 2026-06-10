# MyArea Surveyor Mobile App

A React Native mobile application designed exclusively for surveyors to take surveys in the field.

## Features

✅ **Surveyor-Only Access**
- Login exclusively with surveyor credentials
- Role-based access control

✅ **Survey Management**
- View available questionnaires
- Answer survey questions
- Track survey responses

✅ **Location Selection**
- Cascading dropdowns: Ward → Part → Area → Street
- Easy location hierarchy navigation
- Accurate geolocation tagging

✅ **Offline Support**
- Store responses locally
- Sync when connection available
- Persistent data storage with AsyncStorage

✅ **User-Friendly Interface**
- Clean, intuitive design
- Easy-to-read questions
- Responsive buttons and inputs
- Progress tracking

## Getting Started

### Prerequisites
- Node.js 18+ 
- Expo CLI
- iOS or Android device/emulator

### Installation

```bash
cd mobile
npm install
```

### Running the App

```bash
# Start the development server
npm start

# On iOS
npm run ios

# On Android
npm run android

# Or use Expo Go app:
# 1. Scan the QR code with Expo Go
# 2. App will load on your device
```

## Login Credentials

### Demo Surveyor Account
```
Email: surveyor@myarea.com
Password: Surveyor@123
```

These credentials work offline and are also validated against the backend API.

## App Structure

```
mobile/
├── src/
│   ├── screens/
│   │   ├── LoginScreen.tsx          # Surveyor login
│   │   └── SurveyScreen.tsx         # Survey taking interface
│   ├── context/
│   │   └── AuthContext.ts           # Authentication management
│   ├── services/
│   │   └── ApiClient.ts             # API communication
│   ├── navigation/
│   │   └── RootNavigator.tsx        # Navigation structure
│   └── App.tsx                      # Main app component
├── app.json                         # Expo configuration
├── package.json                     # Dependencies
└── README_SURVEYOR_APP.md          # This file
```

## Key Features

### Authentication
- Local demo credentials for offline testing
- Backend API integration for production
- Secure token storage
- Session management

### Survey Flow
1. **Login**: Surveyor authenticates with email/password
2. **Location Selection**: Choose Ward → Part → Area → Street
3. **Questionnaire**: Answer survey questions
4. **Submit**: Send responses to backend
5. **Confirmation**: Get response confirmation

### API Integration
- Fetches questionnaires from backend
- Retrieves location hierarchies
- Submits survey responses
- Handles offline scenarios gracefully

## Customization

### Change Theme Color
Edit colors in:
- `LoginScreen.tsx`: `#8B1538` (primary color)
- `SurveyScreen.tsx`: Button and accent colors

### Add More Fields
- Modify `SurveyScreen.tsx` to add additional input types
- Update API client to handle new data

### Offline Mode
- Responses are cached in AsyncStorage
- Implement sync when connection returns

## API Endpoints

The app communicates with:
```
API Base: https://func-mobileapp-cs-in.azurewebsites.net
- POST   /api/auth/login              - Authenticate surveyor
- GET    /api/wards                   - Get wards
- GET    /api/parts/{wardId}          - Get parts by ward
- GET    /api/areas/{partId}          - Get areas by part
- GET    /api/streets/{areaId}        - Get streets by area
- GET    /api/questionnaires          - Get survey questions
- POST   /api/surveys                 - Submit survey response
```

## Troubleshooting

### App won't start
```bash
# Clear cache and reinstall
rm -rf node_modules
npm install
npm start
```

### Login not working
- Check internet connection
- Verify credentials are correct
- Check if backend API is accessible

### Location dropdowns empty
- Ensure API is responding
- Check network connectivity
- Verify backend data is populated

## Development

### Run with debugging
```bash
npm start
# Press 'd' to open React Native debugger
```

### Enable verbose logging
```bash
npm start -- --verbose
```

## Release Build

### iOS
```bash
eas build --platform ios --auto-submit
```

### Android
```bash
eas build --platform android --auto-submit
```

## Support

For issues or feature requests:
1. Check existing documentation
2. Review API endpoint responses
3. Check network connectivity
4. Verify backend is running

## License

All rights reserved - MyArea Admin Project
