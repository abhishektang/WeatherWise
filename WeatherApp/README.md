# Weather Application - POS Works Development Prototype

A modern, cross-platform weather application built with .NET MAUI and C# backend, demonstrating best practices in software development.

## 🌟 Features

- **Real-time Weather Data**: Current conditions and forecasts powered by OpenWeatherMap API
- **Current Weather Display**: Temperature, feels like, humidity, wind speed, visibility, pressure, sunrise/sunset
- **Hourly Forecast**: Next 24 hours with temperature, conditions, and precipitation chance
- **5-Day Forecast**: Extended weather outlook with high/low temperatures and conditions
- **Location Services**: 
  - Automatic location detection using device GPS
  - Manual city search with autocomplete
  - Support for multiple locations
- **Modern UI**: Clean, dark-themed interface inspired by MSN Weather
- **Cross-Platform**: Runs on Windows, macOS, iOS, and Android

## 🏗️ Architecture

### Technology Stack
- **Backend**: C# with .NET 10
- **Frontend**: .NET MAUI (Multi-platform App UI)
- **Architecture Pattern**: MVVM (Model-View-ViewModel)
- **API**: OpenWeatherMap REST API
- **Location Services**: MAUI Essentials Geolocation

### Project Structure
```
WeatherApp/
├── Models/              # Data models
│   ├── WeatherData.cs
│   └── Location.cs
├── Services/           # Business logic and API integration
│   ├── IWeatherService.cs
│   ├── WeatherService.cs
│   ├── ILocationService.cs
│   └── LocationService.cs
├── ViewModels/         # MVVM ViewModels
│   ├── BaseViewModel.cs
│   └── WeatherViewModel.cs
├── Views/              # UI Pages
│   ├── WeatherPage.xaml
│   └── WeatherPage.xaml.cs
└── Helpers/            # Utility classes
    └── Converters.cs
```

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 (17.13+) or Visual Studio Code
- macOS (for iOS/macOS development) or Windows (for Windows/Android development)

### Setup Instructions

1. **Clone the repository**
   ```bash
   cd WeatherApp
   ```

2. **Get an API Key**
   - Sign up for a free API key at [OpenWeatherMap](https://openweathermap.org/api)
   - Open `Services/WeatherService.cs`
   - Replace `YOUR_API_KEY_HERE` with your actual API key:
     ```csharp
     private const string ApiKey = "your_actual_api_key_here";
     ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the application**
   ```bash
   # For macOS
   dotnet build -f net10.0-maccatalyst
   
   # For Windows
   dotnet build -f net10.0-windows10.0.19041.0
   
   # For iOS
   dotnet build -f net10.0-ios
   
   # For Android
   dotnet build -f net10.0-android
   ```

5. **Run the application**
   ```bash
   # For macOS
   dotnet run -f net10.0-maccatalyst
   
   # Or use Visual Studio's run button
   ```

## 🔧 Configuration

### Android Permissions
Location permissions are automatically requested. Ensure the following permissions are in `AndroidManifest.xml`:
- `ACCESS_COARSE_LOCATION`
- `ACCESS_FINE_LOCATION`

### iOS Permissions
Add to `Info.plist`:
- `NSLocationWhenInUseUsageDescription`

## 🎨 Design Principles

### Code Quality
- **Clean Architecture**: Separation of concerns with distinct layers
- **SOLID Principles**: Single responsibility, dependency injection
- **Async/Await**: Proper asynchronous programming patterns
- **Error Handling**: Comprehensive try-catch blocks with logging
- **Null Safety**: Nullable reference types enabled

### Security
- **API Key Management**: Externalized configuration (TODO: move to secure storage)
- **HTTPS**: All API calls use secure connections
- **Permissions**: Proper permission handling for location services

### User Experience
- **Responsive Design**: Adapts to different screen sizes
- **Loading States**: Activity indicators during data fetch
- **Error Messages**: Clear user feedback for errors
- **Pull to Refresh**: Easy data updates

## 📱 Platform Support

| Platform | Status | Minimum Version |
|----------|--------|----------------|
| Windows  | ✅ Supported | Windows 10 1809+ |
| macOS    | ✅ Supported | macOS 15.0+ |
| iOS      | ✅ Supported | iOS 15.0+ |
| Android  | ✅ Supported | Android 5.0 (API 21)+ |

## 🔮 Future Enhancements

### Planned Features
- [ ] Weather alerts and notifications
- [ ] Multiple saved locations
- [ ] Detailed weather graphs (temperature, precipitation trends)
- [ ] Weather radar/maps
- [ ] Widget support
- [ ] Dark/light theme toggle
- [ ] Unit preference (Celsius/Fahrenheit)
- [ ] Offline mode with cached data
- [ ] Local emergency phone numbers
- [ ] Seasonal backgrounds
- [ ] Air quality index

### Technical Improvements
- [ ] Unit test coverage
- [ ] Integration tests
- [ ] CI/CD pipeline
- [ ] Localization support
- [ ] Performance optimization
- [ ] Analytics integration
- [ ] Crash reporting

## 🧪 Testing

### Unit Tests (To be implemented)
```bash
dotnet test
```

### Manual Testing Checklist
- [ ] Location permission grant/deny
- [ ] Current location weather fetch
- [ ] City search functionality
- [ ] Hourly forecast display
- [ ] Daily forecast display
- [ ] Error handling (no internet, invalid location)
- [ ] Refresh functionality
- [ ] UI responsiveness

## 📝 Best Practices Demonstrated

1. **Dependency Injection**: Services registered in `MauiProgram.cs`
2. **Interface Segregation**: Separate interfaces for services
3. **Repository Pattern**: Weather service abstracts API calls
4. **MVVM Pattern**: Clear separation of UI and business logic
5. **Data Binding**: Two-way binding in ViewModels
6. **Command Pattern**: User actions handled via ICommand
7. **Async Programming**: Non-blocking UI operations
8. **Error Handling**: Graceful degradation and user feedback
9. **Code Documentation**: XML comments on public APIs
10. **Resource Management**: Proper disposal of HTTP clients

## 🤝 Contributing

This is a prototype for POS Works Development. For production use:
1. Move API keys to secure storage (Azure Key Vault, etc.)
2. Implement comprehensive unit tests
3. Add integration tests
4. Set up CI/CD pipeline
5. Add error tracking and analytics
6. Implement proper logging framework

## 📄 License

Copyright © 2024 POS Works. All rights reserved.

## 🙏 Acknowledgments

- OpenWeatherMap for weather data API
- Microsoft for .NET MAUI framework
- MSN Weather for UI/UX inspiration

## 📞 Support

For questions or issues, please contact the development team at POS Works.

---

**Built with ❤️ for POS Works Development**
