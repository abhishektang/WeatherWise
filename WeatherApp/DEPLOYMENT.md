# Deployment Guide - Weather Application

## 🚀 Deployment Overview

This guide covers deploying the Weather Application to various platforms for the POS Works presentation.

---

## 🖥️ Development Environment Deployment

### Local Testing (Recommended for Presentation)

#### macOS (Mac Catalyst)
```bash
cd WeatherApp
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst
```

**Advantages:**
- Quick to run
- Easy to debug
- Full feature access
- Native macOS experience

#### Windows Desktop
```bash
cd WeatherApp
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

**Advantages:**
- Native Windows UI
- Desktop integration
- Performance optimized

---

## 📱 Mobile Deployment

### iOS Deployment

#### Prerequisites
- macOS with Xcode installed
- Apple Developer account
- iOS device or simulator

#### Steps
```bash
# For Simulator
dotnet build -f net10.0-ios

# Run on simulator
dotnet run -f net10.0-ios
```

#### Device Deployment
1. Open project in Visual Studio for Mac
2. Connect iOS device
3. Select device as target
4. Trust developer certificate on device
5. Build and deploy

### Android Deployment

#### Prerequisites
- Android SDK installed
- Android emulator or physical device

#### Steps
```bash
# Install Android dependencies (if needed)
dotnet build -t:InstallAndroidDependencies -f net10.0-android

# Build
dotnet build -f net10.0-android

# Run on emulator/device
dotnet run -f net10.0-android
```

---

## 🌐 Web Deployment (Future)

### Blazor Hybrid Option
The architecture supports conversion to Blazor Hybrid for web deployment:

1. Add Blazor WebView component
2. Migrate XAML to Razor components
3. Deploy to Azure App Service
4. Configure HTTPS and domains

**Estimated effort:** 2-3 weeks

---

## 🏢 Enterprise Deployment

### Internal Distribution (POS Works Office)

#### Option 1: Direct Installation (Demo/Testing)
**Best for:** Presentation and initial testing
**Steps:**
1. Build release configuration
2. Copy `.app` (macOS) or `.exe` (Windows) to target machine
3. Install and run

```bash
# Build release
dotnet publish -f net10.0-maccatalyst -c Release

# Output location:
# bin/Release/net10.0-maccatalyst/maccatalyst-arm64/publish/
```

#### Option 2: Package Distribution
**Best for:** Multiple installations

**Windows (MSIX):**
```bash
dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=MSIX
```

**macOS (PKG):**
```bash
dotnet publish -f net10.0-maccatalyst -c Release
# Use macOS Packages utility to create .pkg
```

---

## 📦 App Store Deployment (Production)

### Apple App Store (iOS/macOS)

#### Prerequisites
- Apple Developer Program membership ($99/year)
- App Store Connect account
- Provisioning profiles and certificates

#### Steps
1. **Prepare App**
   ```bash
   dotnet publish -f net10.0-ios -c Release
   ```

2. **Archive in Xcode**
   - Open `.xcarchive` in Xcode
   - Validate app
   - Upload to App Store Connect

3. **App Store Connect**
   - Fill app metadata
   - Upload screenshots
   - Set pricing
   - Submit for review

**Timeline:** 1-3 days for review

### Google Play Store (Android)

#### Prerequisites
- Google Play Developer account ($25 one-time)
- Signed APK/AAB

#### Steps
1. **Generate signed bundle**
   ```bash
   dotnet publish -f net10.0-android -c Release
   ```

2. **Sign APK**
   ```bash
   # Create keystore
   keytool -genkey -v -keystore weather.keystore -alias weatherapp

   # Sign APK
   jarsigner -verbose -sigalg SHA256withRSA -digestalg SHA-256 
             -keystore weather.keystore weather-app.apk weatherapp
   ```

3. **Upload to Play Console**
   - Create app listing
   - Upload APK/AAB
   - Complete store listing
   - Submit for review

**Timeline:** Hours to 1 day for review

### Microsoft Store (Windows)

#### Prerequisites
- Microsoft Partner Center account
- MSIX package

#### Steps
1. **Create MSIX package**
   ```bash
   dotnet publish -f net10.0-windows10.0.19041.0 -c Release 
                  -p:WindowsPackageType=MSIX
   ```

2. **Partner Center**
   - Create app submission
   - Upload MSIX
   - Fill app details
   - Submit for certification

**Timeline:** 1-3 days for review

---

## ☁️ Azure Deployment (Future Integration)

### Azure App Service
For web-based version:
```bash
# Publish to Azure
dotnet publish -c Release
az webapp up --name posworks-weather --resource-group weather-rg
```

### Azure Static Web Apps
For Blazor WASM version:
```yaml
# .github/workflows/azure-static-web-apps.yml
name: Deploy to Azure Static Web Apps
on: [push]
jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Build And Deploy
        uses: Azure/static-web-apps-deploy@v1
```

---

## 🔒 Security Checklist Before Deployment

### Pre-Production
- [ ] API keys moved to secure storage (Azure Key Vault)
- [ ] HTTPS enforced for all API calls
- [ ] Certificate pinning implemented
- [ ] Input validation on all user inputs
- [ ] Error messages don't expose sensitive info
- [ ] Logging doesn't include PII
- [ ] Permissions properly requested and handled

### Production
- [ ] Code signing certificates obtained
- [ ] App Store accounts created
- [ ] Privacy policy published
- [ ] Terms of service created
- [ ] GDPR compliance reviewed
- [ ] Analytics configured
- [ ] Crash reporting enabled
- [ ] Security audit completed

---

## 📊 Deployment Checklist

### For POS Works Presentation
- [x] Application builds successfully
- [x] Runs on macOS/Windows
- [x] API key configured
- [x] Location permissions work
- [x] Weather data displays
- [x] All features functional
- [x] No critical bugs
- [x] Professional appearance

### For Beta Testing
- [ ] Release build created
- [ ] Internal testers identified
- [ ] Feedback mechanism in place
- [ ] Crash reporting enabled
- [ ] Usage analytics configured
- [ ] Test scenarios documented
- [ ] Known issues listed

### For Production
- [ ] App store accounts set up
- [ ] Certificates and provisioning profiles
- [ ] App metadata prepared
- [ ] Screenshots created (all devices)
- [ ] Privacy policy URL
- [ ] Support contact information
- [ ] Marketing materials ready
- [ ] Launch date planned

---

## 🎯 Recommended Deployment Strategy

### Phase 1: Internal Demo (Current)
**Platform:** macOS/Windows desktop  
**Method:** Direct run from Visual Studio  
**Duration:** 1 week  
**Goal:** POS Works approval

### Phase 2: Beta Testing
**Platform:** TestFlight (iOS), Internal Testing (Android)  
**Method:** Ad-hoc distribution  
**Duration:** 2-4 weeks  
**Goal:** User feedback and bug fixes

### Phase 3: Soft Launch
**Platform:** App stores (limited regions)  
**Method:** Public release  
**Duration:** 1 month  
**Goal:** Production validation

### Phase 4: Full Launch
**Platform:** All app stores worldwide  
**Method:** Public release with marketing  
**Duration:** Ongoing  
**Goal:** Public availability

---

## 🔧 Configuration for Different Environments

### Development
```json
{
  "Environment": "Development",
  "ApiKey": "dev_key_here",
  "LogLevel": "Debug",
  "EnableAnalytics": false
}
```

### Staging
```json
{
  "Environment": "Staging",
  "ApiKey": "staging_key_here",
  "LogLevel": "Information",
  "EnableAnalytics": true
}
```

### Production
```json
{
  "Environment": "Production",
  "ApiKey": "prod_key_here",
  "LogLevel": "Warning",
  "EnableAnalytics": true,
  "CrashReporting": true
}
```

---

## 📱 Device Testing Matrix

### Minimum Testing Before Launch

| Platform | Device | OS Version | Status |
|----------|--------|------------|--------|
| iOS | iPhone 13 | iOS 15+ | Required |
| iOS | iPad Air | iPadOS 15+ | Required |
| Android | Pixel 6 | Android 12+ | Required |
| Android | Samsung S21 | Android 11+ | Required |
| Windows | Surface | Windows 10 | Required |
| Windows | Desktop PC | Windows 11 | Required |
| macOS | MacBook | macOS 15+ | Required |

---

## 🚨 Rollback Procedure

### If Issues Arise Post-Deployment

1. **Immediate Actions**
   - Remove app from app stores (if critical)
   - Post status on social media/website
   - Email beta testers

2. **Fix and Redeploy**
   ```bash
   # Revert to last known good version
   git revert HEAD
   
   # Or checkout previous version
   git checkout v1.0.0
   
   # Rebuild and redeploy
   dotnet publish -c Release
   ```

3. **Communication**
   - Notify users of the issue
   - Provide timeline for fix
   - Apologize for inconvenience

---

## 📈 Post-Deployment Monitoring

### Metrics to Track
- Crash-free rate (target: >99%)
- App launch time (target: <3 seconds)
- API response time (target: <1 second)
- User retention (Day 1, Day 7, Day 30)
- Feature usage statistics
- Error rates
- User feedback/ratings

### Tools
- **Analytics:** Azure Application Insights, Google Analytics
- **Crash Reporting:** Visual Studio App Center
- **Performance:** Azure Monitor, Xamarin Profiler
- **User Feedback:** In-app surveys, App Store reviews

---

## 💰 Deployment Costs Estimate

### One-Time Costs
- Apple Developer Program: $99/year
- Google Play Developer: $25 one-time
- Microsoft Partner Center: Free (or $19/year for individual)
- Code signing certificates: $0-300/year

### Ongoing Costs
- OpenWeatherMap API: $0-$40/month (depends on usage)
- Azure hosting: $0-$50/month (if cloud backend added)
- Analytics tools: $0-$100/month
- Push notification service: $0-$20/month

**Total estimated:** $150-$600 first year, then $100-$300/year

---

## ✅ Final Pre-Deployment Checklist

- [ ] All code committed to version control
- [ ] Release notes written
- [ ] API keys secured
- [ ] Certificates obtained
- [ ] App icons created (all sizes)
- [ ] Screenshots prepared
- [ ] Privacy policy published
- [ ] Support email set up
- [ ] Testing completed
- [ ] Backup created
- [ ] Team notified
- [ ] Launch date confirmed

---

## 📞 Support & Resources

### For Deployment Issues
- [.NET MAUI Deployment Docs](https://learn.microsoft.com/dotnet/maui/deployment/)
- [Apple Developer Documentation](https://developer.apple.com/documentation/)
- [Google Play Console Help](https://support.google.com/googleplay/android-developer/)
- [Microsoft Store Documentation](https://docs.microsoft.com/windows/uwp/publish/)

### POS Works Internal
- DevOps team for CI/CD setup
- IT team for certificate management
- Marketing for app store content
- Legal for privacy policy review

---

**Deployment Status**: Ready for internal presentation ✅

*Last Updated: December 2024*
