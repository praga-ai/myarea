# MyArea Surveyor - React Native Mobile App

Pure React Native (no Expo) surveyor application for conducting field surveys with automated GitHub Actions builds.

## Features

✅ **Surveyor-Only App**
- Login with surveyor credentials
- Role-based access control
- Offline support

✅ **Survey Management**
- Questionnaire management
- Response tracking
- Location selection hierarchy

✅ **Automated Builds**
- GitHub Actions CI/CD
- Android APK builds
- iOS IPA builds
- Automated releases

## Setup & Development

### Prerequisites
- Node.js 18+
- Java 17 (for Android)
- Xcode 15+ (for iOS)
- Android SDK 34+

### Installation

```bash
cd mobile
npm install
```

### Running Locally

**Android:**
```bash
npm run android
```

**iOS:**
```bash
npm run ios
```

**Metro Bundler:**
```bash
npm start
```

## Building

### Android APK (Debug)
```bash
npm run android
```

### Android APK (Release)
```bash
npm run build:android
```
Output: `mobile/android/app/build/outputs/apk/release/app-release.apk`

### Android App Bundle (for Play Store)
```bash
npm run build:android:aab
```
Output: `mobile/android/app/build/outputs/bundle/release/app-release.aab`

### iOS IPA (Release)
```bash
npm run build:ios
```

## GitHub Actions CI/CD

### Automated Builds
The app automatically builds on:
- **Push to main**: Triggers Android & iOS builds
- **Push to develop**: Triggers Android & iOS builds
- **Pull requests**: Runs build verification

### Build Artifacts
- Android APK: Available for 30 days
- iOS IPA: Available for 30 days
- Releases: Created on main branch builds

### Trigger a Build
Simply push to main or develop branch:
```bash
git push origin main
```

## Project Structure

```
mobile/
├── src/
│   ├── context/
│   │   └── AuthContext.ts          # Auth management
│   ├── screens/
│   │   ├── LoginScreen.tsx         # Surveyor login
│   │   └── SurveyScreen.tsx        # Survey taking
│   ├── services/
│   │   └── ApiClient.ts            # API calls
│   ├── App.tsx                     # Main app component
│   └── index.ts                    # Entry point
├── android/                         # Android native code
├── ios/                             # iOS native code
├── app.json                         # App configuration
├── package.json                     # Dependencies
├── metro.config.js                  # Metro bundler config
├── babel.config.js                  # Babel config
├── tsconfig.json                    # TypeScript config
└── .eslintrc.js                     # ESLint config
```

## Configuration

### app.json
Update app metadata:
- `name`: App display name
- `packageName`: Android package (com.example.app)
- `bundleId`: iOS bundle ID
- `version`: Semantic version

### Permissions

**Android** (AndroidManifest.xml):
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
```

**iOS** (Info.plist):
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>We need your location for surveys</string>
```

## Demo Credentials

```
Email: surveyor@myarea.com
Password: Surveyor@123
```

## Troubleshooting

### Android Build Fails
```bash
# Clear cache
cd android && ./gradlew clean && cd ..
npm install
npm run build:android
```

### iOS Build Fails
```bash
# Update CocoaPods
cd mobile/ios
pod deintegrate
pod install --repo-update
cd ../..
npm run build:ios
```

### Metro Bundler Issues
```bash
npm start -- --reset-cache
```

### Dependencies Not Found
```bash
rm -rf node_modules package-lock.json
npm install
```

## Debugging

### Android Debugging
```bash
# Connect device via USB, enable USB debugging
npm run android
```

### iOS Debugging
```bash
npm run ios
```

### React DevTools
```bash
npm install -g react-devtools
react-devtools
# In another terminal:
npm start
```

## Release Process

### Android Play Store
1. Build AAB: `npm run build:android:aab`
2. Upload to Google Play Console
3. Review and publish

### iOS App Store
1. Build IPA: `npm run build:ios`
2. Upload via Transporter or Xcode
3. Review and publish

## API Integration

Base URL: `https://func-mobileapp-cs-in.azurewebsites.net`

**Endpoints:**
- `POST /api/auth/login` - Authenticate
- `GET /api/wards` - Get locations
- `GET /api/questionnaires` - Get surveys
- `POST /api/surveys` - Submit response

## Performance Tips

- Use React Navigation memoization
- Enable ProGuard for Android (release builds)
- Use AsyncStorage for caching
- Optimize images and assets

## Security

- Credentials stored in AsyncStorage (encrypted on device)
- HTTPS for all API calls
- Token-based authentication
- No sensitive data in logs

## Resources

- [React Native Docs](https://reactnative.dev)
- [React Navigation](https://reactnavigation.org)
- [Android Build Docs](https://developer.android.com/build)
- [iOS Build Docs](https://developer.apple.com/build)

## License

All rights reserved - MyArea Project
