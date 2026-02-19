# WeatherWise 🌤️

A modern, cross-platform weather application built with .NET MAUI that provides real-time weather data, forecasts, and location-based services.

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-10.0-blue)](https://dotnet.microsoft.com/apps/maui)
[![C#](https://img.shields.io/badge/C%23-12-green)](https://docs.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## ✨ Features

- 🌍 **Current Weather**: Real-time weather data with GPS-based automatic location detection
- 📊 **Forecasts**: Hourly (24h) and daily (7-day) weather predictions
- 🔍 **Location Search**: Search cities worldwide with accurate geocoding
- ⭐ **Favorites**: Save and quickly access your favorite locations
- 💾 **Smart Caching**: 10-minute TTL cache system to minimize API calls
- 🎯 **Accurate Location**: Suburb/neighborhood-level precision
- 📱 **Cross-Platform**: iOS, Android, macOS, and Windows support

## 🛠️ Tech Stack

### Framework & Runtime
- **.NET 10.0** - Latest .NET framework
- **.NET MAUI** - Multi-platform app UI framework
- **C# 12** - Modern C# with latest features

### Architecture
- **MVVM Pattern** - Clean separation of concerns
- **Dependency Injection** - Service-based architecture
- **Repository Pattern** - Data access abstraction

### Database & Storage
- **SQLite** - Local database
- **Entity Framework Core 9.0** - ORM for database operations

### APIs
- **Open-Meteo API** - Free weather data (no API key required)
- **Nominatim/OpenStreetMap** - Reverse geocoding

### Key Libraries
- `CommunityToolkit.Mvvm` - MVVM helpers
- `CommunityToolkit.Maui` - MAUI extensions
- `Microsoft.EntityFrameworkCore.Sqlite` - Database

### Testing
- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Expressive assertions
- **74 Tests** - Comprehensive test coverage (38 positive, 17 negative, 19 edge cases)

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- IDE (choose one):
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload
  - [JetBrains Rider](https://www.jetbrains.com/rider/) with MAUI plugin
  - [VS Code](https://code.visualstudio.com/) with MAUI extension
- For macOS: Xcode (for iOS/macOS development)
- For Windows: Visual Studio with MAUI workload

## 🚀 Getting Started

### Clone the Repository

```bash
git clone https://github.com/abhishektang/WeatherWise.git
cd WeatherWise
```

### Restore Dependencies

```bash
dotnet restore
```

### Build the Project

```bash
# For macOS
dotnet build WeatherApp/WeatherApp.csproj -f net10.0-maccatalyst

# For iOS
dotnet build WeatherApp/WeatherApp.csproj -f net10.0-ios

# For Android
dotnet build WeatherApp/WeatherApp.csproj -f net10.0-android

# For Windows
dotnet build WeatherApp/WeatherApp.csproj -f net10.0-windows10.0.19041.0
```

### Run the Application

```bash
cd WeatherApp
dotnet run -f net10.0-maccatalyst  # For macOS
```

### Run Tests

```bash
dotnet test WeatherApp.Tests/WeatherApp.Tests.csproj
```

## 📁 Project Structure

```
WeatherWise/
├── WeatherApp/                   # Main application
│   ├── Models/                   # Data models
│   ├── ViewModels/               # MVVM view models
│   ├── Views/                    # XAML UI pages
│   ├── Services/                 # Business logic & API services
│   ├── Data/                     # Database context & entities
│   ├── Platforms/                # Platform-specific code
│   └── MauiProgram.cs           # App entry point & DI setup
│
├── WeatherApp.Tests/             # Unit tests
│   ├── Models/                   # Model tests
│   ├── Services/                 # Service tests with mocking
│   ├── ViewModels/               # ViewModel tests
│   └── Data/                     # Database tests
│
├── weather_app.slnx              # Solution file
└── README.md                     # This file
```

## 🏗️ Architecture

### MVVM Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                         VIEW (XAML)                         │
│  ┌────────────────────────────────────────────────────┐   │
│  │  WeatherPage.xaml, FavoritesPage.xaml             │   │
│  │  • User Interface                                   │   │
│  │  • Data Binding                                     │   │
│  └────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │ Data Binding
┌──────────────────────▼──────────────────────────────────────┐
│                      VIEWMODEL                              │
│  ┌────────────────────────────────────────────────────┐   │
│  │  WeatherViewModel, FavoritesViewModel             │   │
│  │  • UI State Management                             │   │
│  │  • Commands                                         │   │
│  │  • Service Orchestration                           │   │
│  └────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │ Service Calls
┌──────────────────────▼──────────────────────────────────────┐
│                      SERVICES                               │
│  ┌────────────────────────────────────────────────────┐   │
│  │  WeatherService → Open-Meteo API                  │   │
│  │  LocationService → GPS                             │   │
│  │  CacheService → SQLite                             │   │
│  │  FavoritesRepository → SQLite                      │   │
│  └────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Cache Strategy

```
Request Weather
    ↓
Check Cache (10-min TTL)
    ↓
Fresh? ──Yes──→ Return Cached Data
    │
    No
    ↓
Fetch from API
    ↓
Save to Cache
    ↓
Return Fresh Data
```

## 🔧 Configuration

### Location Permissions

The app requires location permissions to provide current weather data. Permission descriptions are configured in platform-specific files:

- **iOS/macOS**: `Platforms/MacCatalyst/Info.plist`
- **Android**: `Platforms/Android/AndroidManifest.xml`

### Database

- SQLite database is automatically created at: `FileSystem.AppDataDirectory/weather.db`
- Cache TTL: 10 minutes
- Automatic schema migration on app start

## 🧪 Testing

The project includes comprehensive unit tests covering:

- **Model Tests** (12 tests): Data structure validation
- **Service Tests** (28 tests): API mocking and business logic
- **ViewModel Tests** (24 tests): UI state and command handling
- **Entity Tests** (10 tests): Database entity validation

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📱 Platform Support

- ✅ **macOS** (Mac Catalyst) - Tested
- ✅ **iOS** - Compatible
- ✅ **Android** - Compatible
- ✅ **Windows** - Compatible

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [Open-Meteo](https://open-meteo.com/) - Free weather API
- [OpenStreetMap/Nominatim](https://nominatim.openstreetmap.org/) - Geocoding service
- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) - Cross-platform framework

## 📧 Contact

Abhishek Tanguturi - [@abhishektang](https://github.com/abhishektang)

Project Link: [https://github.com/abhishektang/WeatherWise](https://github.com/abhishektang/WeatherWise)

---

Made with ❤️ using .NET MAUI
