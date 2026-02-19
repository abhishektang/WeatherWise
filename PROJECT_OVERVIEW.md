# Weather App - Complete Project Overview

## 🎯 Project Summary
A **cross-platform weather application** built with **.NET MAUI** featuring real-time weather data, favorites management, caching, and location services.

**Architecture:** MVVM (Model-View-ViewModel)  
**Database:** SQLite with Entity Framework Core  
**API:** Open-Meteo (Free weather API)  
**Testing:** 46 unit tests (xUnit, Moq, FluentAssertions)

---

## 📂 Project Structure & File Details

### **1. ENTRY POINT & CONFIGURATION**

#### **MauiProgram.cs** - Application Bootstrap
**Purpose:** Configures dependency injection and initializes the app

**Key Methods:**
- `CreateMauiApp()` - Sets up services, fonts, and database

**Dependency Injection Setup:**
```csharp
// Services (Singleton - one instance for app lifetime)
- HttpClient - Makes HTTP requests
- IWeatherService → WeatherService - Fetches weather data
- ILocationService → LocationService - Gets device location

// Repositories (Scoped - new instance per operation)
- IFavoritesRepository → FavoritesRepository - Manages favorites CRUD
- IWeatherCacheService → WeatherCacheService - Handles caching

// ViewModels (Singleton - preserves state across navigation)
- WeatherViewModel - Main weather logic
- FavoritesViewModel - Favorites list logic

// Views (Singleton - preserves UI state)
- WeatherPage - Main weather display
- FavoritesPage - Favorites list display
```

**Database Initialization:**
- Creates SQLite database at `weather.db` in app data directory
- Calls `EnsureCreated()` to set up tables on first launch

---

#### **App.xaml.cs** - Application Class
**Purpose:** App entry point and window creation

**Key Methods:**
- `CreateWindow()` - Creates main app window with AppShell as root

---

#### **AppShell.xaml / AppShell.xaml.cs** - Navigation Shell
**Purpose:** Defines app navigation structure and routes

**Key Methods:**
- Constructor registers route: `"WeatherPage"` → WeatherPage

**Navigation Structure:**
- TabBar with 2 tabs: Weather (WeatherPage) and Favorites (FavoritesPage)

---

### **2. VIEWS (UI Layer)**

#### **WeatherPage.xaml / WeatherPage.xaml.cs**
**Purpose:** Main weather display page

**Properties:**
- `Latitude`, `Longitude`, `LocationName` - Query parameters for navigation

**Key Methods:**
- `OnAppearing()` - Loads weather when page appears
  - If navigated with lat/lon → loads that location
  - Otherwise → loads last viewed or current location

**UI Features:**
- Search bar for location lookup
- Current weather display (temp, feels like, humidity, wind, etc.)
- 24-hour hourly forecast
- 7-day daily forecast
- Refresh button
- Add to favorites button

---

#### **FavoritesPage.xaml / FavoritesPage.xaml.cs**
**Purpose:** Displays saved favorite locations

**Key Methods:**
- `OnAppearing()` - Calls `_viewModel.InitializeAsync()` to load favorites

**UI Features:**
- CollectionView showing all favorites
- Swipe actions: Delete, Refresh weather
- Tap to navigate to WeatherPage with selected location

---

### **3. VIEWMODELS (Business Logic Layer)**

#### **BaseViewModel.cs**
**Purpose:** Base class for all ViewModels

**Properties:**
- `IsBusy` - Indicates loading state
- `Title` - Page title

**Key Methods:**
- `OnPropertyChanged()` - Notifies UI of property changes
- `SetProperty()` - Updates property and triggers notification

**Pattern:** Implements `INotifyPropertyChanged` for MVVM data binding

---

#### **WeatherViewModel.cs** (327 lines)
**Purpose:** Main weather page logic

**Dependencies:**
- `IWeatherService` - Fetches weather data
- `ILocationService` - Gets device location
- `IFavoritesRepository` - Manages favorites
- `IWeatherCacheService` - Handles caching

**Properties:**
- `WeatherData` - Current weather information
- `SearchQuery` - User's search text
- `ErrorMessage` - Error display
- `IsRefreshing` - Pull-to-refresh state
- `IsFavorite` - Is current location favorited?

**Commands (User Actions):**
- `RefreshCommand` → `RefreshWeatherAsync()`
- `SearchCommand` → `SearchLocationAsync()`
- `GetCurrentLocationCommand` → `LoadCurrentLocationWeatherAsync()`
- `ToggleFavoriteCommand` → `ToggleFavoriteAsync()`

**Key Methods:**

1. **`InitializeAsync()`**
   - Loads last viewed weather from cache on app start

2. **`LoadCurrentLocationWeatherAsync()`**
   - Gets device GPS location
   - Checks cache first (10-minute TTL)
   - Fetches fresh data if cache stale
   - Saves to cache and preferences

3. **`SearchLocationAsync()`**
   - Searches for location using Open-Meteo Geocoding API
   - Gets first result's coordinates
   - Checks cache, then fetches weather
   - Updates display and cache

4. **`RefreshWeatherAsync()`**
   - Forces fresh API call (bypasses cache)
   - Updates current weather display

5. **`ToggleFavoriteAsync()`**
   - Adds/removes current location from favorites
   - Saves current weather data (temp, condition) to favorite

6. **`CheckIfFavoriteAsync()`**
   - Checks if current location is in favorites
   - Updates `IsFavorite` property for UI

---

#### **FavoritesViewModel.cs** (149 lines)
**Purpose:** Favorites page logic

**Dependencies:**
- `IFavoritesRepository` - CRUD operations
- `IWeatherService` - Refresh weather data
- `IWeatherCacheService` - Cache management

**Properties:**
- `Favorites` - ObservableCollection of favorite locations
- `ErrorMessage` - Error display

**Commands:**
- `LoadFavoritesCommand` → `LoadFavoritesAsync()`
- `SelectFavoriteCommand` → `SelectFavoriteAsync()`
- `DeleteFavoriteCommand` → `DeleteFavoriteAsync()`
- `RefreshWeatherCommand` → `RefreshWeatherAsync()`

**Key Methods:**

1. **`LoadFavoritesAsync()`**
   - Fetches all favorites from database
   - Orders by last accessed date
   - Populates ObservableCollection

2. **`SelectFavoriteAsync()`**
   - Records access (increments counter, updates timestamp)
   - Navigates to WeatherPage with location data

3. **`DeleteFavoriteAsync()`**
   - Shows confirmation dialog
   - Removes from database and UI collection

4. **`RefreshWeatherAsync()`**
   - Fetches latest weather for specific favorite
   - Updates temp and condition in database
   - Refreshes cache

5. **`InitializeAsync()`**
   - Called on page appear
   - Loads all favorites

---

### **4. SERVICES (Data & API Layer)**

#### **IWeatherService.cs / WeatherService.cs** (451 lines)
**Purpose:** Fetches weather data from Open-Meteo API

**Dependencies:**
- `HttpClient` - Makes HTTP requests

**Key Methods:**

1. **`GetWeatherAsync(latitude, longitude)`**
   - Builds API URL with parameters (current, hourly, daily)
   - Makes HTTP GET request
   - Parses JSON response
   - Returns `WeatherData` object

2. **`GetWeatherByLocationAsync(locationName)`**
   - Searches for location first
   - Gets coordinates of first result
   - Calls `GetWeatherAsync()` with coordinates

3. **`SearchLocationsAsync(query)`**
   - Calls Open-Meteo Geocoding API
   - Returns list of matching locations (max 5)

4. **`ParseOpenMeteoData(json, lat, lon)`** (Private)
   - Deserializes JSON response
   - Extracts current weather, hourly (24h), daily (7d) forecasts
   - Converts WMO weather codes to readable conditions

5. **`GetLocationNameAsync(latitude, longitude)`** (Private)
   - Reverse geocoding - converts coordinates to city name
   - Fallback to Nominatim API if needed

6. **`GetWeatherCondition(wmoCode)`** (Private)
   - Maps WMO codes to descriptions (e.g., 0="Clear sky", 61="Rain")

7. **`GetWeatherIconFromWMO(wmoCode)`** (Private)
   - Maps WMO codes to icon identifiers

**API Endpoints Used:**
- Weather: `https://api.open-meteo.com/v1/forecast`
- Geocoding: `https://geocoding-api.open-meteo.com/v1/search`
- Reverse Geocoding (fallback): `https://nominatim.openstreetmap.org/reverse`

---

#### **ILocationService.cs / LocationService.cs** (53 lines)
**Purpose:** Handles device location and permissions

**Key Methods:**

1. **`GetCurrentLocationAsync()`**
   - Checks location permissions
   - Requests permission if not granted
   - Gets GPS coordinates with medium accuracy
   - 10-second timeout

2. **`CheckPermissionsAsync()`**
   - Checks if location permission granted

3. **`RequestPermissionsAsync()`**
   - Prompts user for location permission

**Uses:** MAUI `Geolocation` API

---

#### **IFavoritesRepository.cs / FavoritesRepository.cs** (71 lines)
**Purpose:** CRUD operations for favorite locations

**Dependencies:**
- `WeatherDbContext` - EF Core database context

**Key Methods:**

1. **`GetAllAsync()`**
   - Returns all favorites ordered by last accessed date

2. **`GetByIdAsync(id)`**
   - Finds favorite by primary key

3. **`GetByCoordinatesAsync(latitude, longitude)`**
   - Finds favorite within 0.01° tolerance (~1km)

4. **`AddAsync(location)`**
   - Inserts new favorite into database

5. **`UpdateAsync(location)`**
   - Updates existing favorite

6. **`DeleteAsync(id)`**
   - Removes favorite from database

7. **`RecordAccessAsync(id)`**
   - Increments access counter
   - Updates last accessed timestamp

---

#### **IWeatherCacheService.cs / WeatherCacheService.cs** (114 lines)
**Purpose:** Caching layer for weather data

**Dependencies:**
- `WeatherDbContext` - Database access

**Key Methods:**

1. **`GetCachedWeatherAsync(latitude, longitude)`**
   - Searches within 0.01° tolerance (~1km)
   - Checks if cache is stale (>10 minutes)
   - Deserializes JSON to WeatherData
   - Returns null if stale or not found

2. **`SaveWeatherCacheAsync(weatherData)`**
   - Serializes WeatherData to JSON
   - Stores in database with timestamp
   - Cleans up old entries (keeps last 50)

3. **`GetLastViewedWeatherAsync()`**
   - Reads last location from MAUI Preferences
   - Attempts to get cached weather for that location

4. **`SaveLastViewedLocationAsync(latitude, longitude)`**
   - Saves coordinates to MAUI Preferences
   - Persists across app launches

**Cache TTL:** 10 minutes (defined in `WeatherHistory.IsStale` property)

---

### **5. DATA LAYER**

#### **WeatherDbContext.cs** (40 lines)
**Purpose:** Entity Framework Core database context

**DbSets (Tables):**
- `FavoriteLocations` - Saved favorite locations
- `WeatherHistory` - Cached weather data
- `SearchHistory` - User search queries

**Key Methods:**
- `OnModelCreating()` - Configures indexes for performance

**Indexes Created:**
- `FavoriteLocations`: `AddedDate`, `(Latitude, Longitude)`
- `WeatherHistory`: `(Latitude, Longitude, FetchedAt)`, `FetchedAt`
- `SearchHistory`: `SearchedAt`

---

#### **Entities (Database Tables)**

**FavoriteLocation.cs** (42 lines)
- `Id` (PK), `Name`, `Region`, `Country`
- `Latitude`, `Longitude`
- `AddedDate`, `LastAccessedDate`, `AccessCount`
- `LastKnownTemperature`, `LastKnownCondition` (cached for quick display)

**WeatherHistory.cs** (48 lines)
- `Id` (PK), `Latitude`, `Longitude`, `LocationName`
- `FetchedAt` (timestamp)
- `WeatherDataJson` (full weather data as JSON)
- Quick access fields: `Temperature`, `FeelsLike`, `Humidity`, `WindSpeed`, `Condition`
- `IsStale` property - returns true if >10 minutes old

**SearchHistory.cs** (17 lines)
- `Id` (PK), `Query`, `SearchedAt`, `ResultCount`

---

### **6. MODELS (Data Transfer Objects)**

#### **WeatherData.cs** (98 lines)
**Purpose:** Represents complete weather information

**Classes:**
1. **`WeatherData`** - Top-level container
   - `LocationName`, `Country`, `Latitude`, `Longitude`
   - `Current` (CurrentWeather object)
   - `HourlyForecasts` (List of 24 hours)
   - `DailyForecasts` (List of 7 days)
   - `LastUpdated` (timestamp)

2. **`CurrentWeather`** - Current conditions
   - `Temperature`, `FeelsLike`, `Humidity`, `WindSpeed`
   - `Pressure`, `CloudCover`, `Visibility`
   - `Condition`, `ConditionIcon`
   - `Sunrise`, `Sunset`, `DateTime`

3. **`HourlyForecast`** - Hourly prediction
   - `DateTime`, `Temperature`, `FeelsLike`
   - `Condition`, `Humidity`, `WindSpeed`
   - `ChanceOfRain`, `PrecipitationAmount`

4. **`DailyForecast`** - Daily prediction
   - `Date`, `MaxTemperature`, `MinTemperature`, `AvgTemperature`
   - `Condition`, `ChanceOfRain`, `TotalPrecipitation`
   - `MaxWindSpeed`, `AvgHumidity`

---

#### **Location.cs** (15 lines)
**Purpose:** Represents a geographic location from search

**Properties:**
- `Name`, `Region`, `Country`
- `Latitude`, `Longitude`

---

### **7. HELPERS**

#### **Converters.cs** (55 lines)
**Purpose:** XAML value converters for data binding

**Converters:**

1. **`NullToBoolConverter`**
   - Converts null to false, non-null to true
   - Used for visibility binding (show if data exists)

2. **`StringToBoolConverter`**
   - Converts empty string to false, non-empty to true
   - Used for button enable/disable

3. **`GreaterThanZeroConverter`**
   - Converts number > 0 to true, otherwise false
   - Used for conditional display (e.g., show rain chance if > 0)

---

## 🔄 APPLICATION FLOW

### **App Launch Flow:**

```
1. Program Entry
   └─> MauiProgram.CreateMauiApp()
       ├─> Register all services (DI)
       ├─> Initialize SQLite database
       └─> Return MauiApp instance

2. App.CreateWindow()
   └─> Creates Window with AppShell

3. AppShell Constructor
   └─> Registers navigation routes
   └─> Shows TabBar (Weather + Favorites tabs)

4. WeatherPage.OnAppearing()
   └─> WeatherViewModel.InitializeAsync()
       └─> WeatherCacheService.GetLastViewedWeatherAsync()
           ├─> Reads last location from Preferences
           └─> Loads cached weather (if < 10 min old)
```

---

### **User Interaction Flows:**

#### **Flow 1: Get Current Location Weather**
```
User taps "Current Location" button
   └─> WeatherViewModel.LoadCurrentLocationWeatherAsync()
       ├─> LocationService.GetCurrentLocationAsync()
       │   ├─> Check/Request location permissions
       │   └─> Get GPS coordinates
       ├─> WeatherCacheService.GetCachedWeatherAsync(lat, lon)
       │   └─> Check database for recent cache (< 10 min)
       ├─> If cache miss:
       │   └─> WeatherService.GetWeatherAsync(lat, lon)
       │       ├─> HTTP GET to Open-Meteo API
       │       ├─> Parse JSON response
       │       └─> Return WeatherData object
       ├─> WeatherCacheService.SaveWeatherCacheAsync()
       │   └─> Save to database as JSON
       ├─> WeatherCacheService.SaveLastViewedLocationAsync()
       │   └─> Save coordinates to Preferences
       ├─> WeatherViewModel.CheckIfFavoriteAsync()
       │   └─> FavoritesRepository.GetByCoordinatesAsync()
       └─> Update UI (INotifyPropertyChanged triggers)
```

---

#### **Flow 2: Search for Location**
```
User types "New York" and taps Search
   └─> WeatherViewModel.SearchLocationAsync()
       ├─> WeatherService.SearchLocationsAsync("New York")
       │   ├─> HTTP GET to Open-Meteo Geocoding API
       │   └─> Returns List<Location> (up to 5 results)
       ├─> Take first result (New York, US)
       ├─> WeatherCacheService.GetCachedWeatherAsync(40.71, -74.00)
       ├─> If cache miss:
       │   └─> WeatherService.GetWeatherAsync(40.71, -74.00)
       ├─> WeatherCacheService.SaveWeatherCacheAsync()
       ├─> WeatherCacheService.SaveLastViewedLocationAsync()
       └─> Update UI with New York weather
```

---

#### **Flow 3: Add to Favorites**
```
User taps "Add to Favorites" button
   └─> WeatherViewModel.ToggleFavoriteAsync()
       ├─> FavoritesRepository.GetByCoordinatesAsync()
       │   └─> Check if already favorited
       ├─> If not favorited:
       │   └─> FavoritesRepository.AddAsync(new FavoriteLocation {
       │       Name = "New York",
       │       Latitude = 40.71,
       │       Longitude = -74.00,
       │       LastKnownTemperature = 72.5,
       │       LastKnownCondition = "Clear sky"
       │   })
       └─> Update IsFavorite property (button changes to "Remove")
```

---

#### **Flow 4: View Favorites**
```
User taps "Favorites" tab
   └─> FavoritesPage.OnAppearing()
       └─> FavoritesViewModel.InitializeAsync()
           └─> FavoritesViewModel.LoadFavoritesAsync()
               ├─> FavoritesRepository.GetAllAsync()
               │   └─> SELECT * FROM FavoriteLocations ORDER BY LastAccessedDate DESC
               └─> Populate ObservableCollection<FavoriteLocation>
               └─> UI updates automatically (data binding)
```

---

#### **Flow 5: Select Favorite**
```
User taps on "New York" in favorites list
   └─> FavoritesViewModel.SelectFavoriteAsync(newYorkFavorite)
       ├─> FavoritesRepository.RecordAccessAsync(id)
       │   ├─> UPDATE FavoriteLocations SET
       │   │   LastAccessedDate = NOW(),
       │   │   AccessCount = AccessCount + 1
       │   └─> WHERE Id = id
       └─> Shell.Current.GoToAsync("//WeatherPage?lat=40.71&lon=-74.00&name=New%20York")
           └─> WeatherPage.OnAppearing()
               └─> Loads weather for New York
                   (follows "Get Current Location Weather" flow)
```

---

#### **Flow 6: Refresh Favorite Weather**
```
User swipes favorite and taps "Refresh"
   └─> FavoritesViewModel.RefreshWeatherAsync(favorite)
       ├─> WeatherService.GetWeatherAsync(favorite.Latitude, favorite.Longitude)
       │   └─> Force fresh API call (no cache)
       ├─> Update favorite object:
       │   ├─> favorite.LastKnownTemperature = weatherData.Current.Temperature
       │   └─> favorite.LastKnownCondition = weatherData.Current.Condition
       ├─> FavoritesRepository.UpdateAsync(favorite)
       │   └─> UPDATE FavoriteLocations SET ...
       ├─> WeatherCacheService.SaveWeatherCacheAsync(weatherData)
       └─> Update UI (ObservableCollection item refresh)
```

---

#### **Flow 7: Delete Favorite**
```
User swipes favorite and taps "Delete"
   └─> FavoritesViewModel.DeleteFavoriteAsync(favorite)
       ├─> Shell.Current.DisplayAlertAsync("Delete?")
       │   └─> Shows confirmation dialog
       ├─> If confirmed:
       │   └─> FavoritesRepository.DeleteAsync(favorite.Id)
       │       └─> DELETE FROM FavoriteLocations WHERE Id = id
       └─> Favorites.Remove(favorite)
           └─> UI updates (item removed from list)
```

---

## 🏗️ Architecture Patterns Used

### **1. MVVM (Model-View-ViewModel)**
- **Views:** XAML + Code-behind (minimal logic)
- **ViewModels:** Business logic + Commands + Properties
- **Models:** Data structures (WeatherData, Location, etc.)
- **Data Binding:** Two-way communication between View and ViewModel

### **2. Dependency Injection**
- All services registered in `MauiProgram.cs`
- Constructor injection throughout application
- Lifecycle management: Singleton vs Scoped

### **3. Repository Pattern**
- `FavoritesRepository` abstracts database operations
- Interface `IFavoritesRepository` allows for testing and swapping implementations

### **4. Service Layer**
- `WeatherService` - API calls
- `LocationService` - Device location
- `WeatherCacheService` - Caching logic
- Clear separation of concerns

### **5. Caching Strategy**
- **L1 Cache:** In-memory (ViewModel properties)
- **L2 Cache:** Database (10-minute TTL)
- **L3 Cache:** Preferences (last viewed location)

### **6. Command Pattern**
- All user actions mapped to ICommand
- Enables XAML binding and testability

---

## 🧪 Testing

**Test Project:** `WeatherApp.Tests`  
**Framework:** xUnit  
**Mocking:** Moq  
**Assertions:** FluentAssertions

**Test Coverage: 46 Tests**
- **FavoritesRepositoryTests** (15 tests) - CRUD operations
- **WeatherDataTests** (12 tests) - Model validation
- **FavoriteLocationTests** (9 tests) - Entity tests
- **WeatherHistoryTests** (10 tests) - Cache TTL logic

**Test Types:**
- Positive tests (happy path)
- Negative tests (null values, missing data)
- Edge cases (0,0 coordinates, negative temps, Arctic conditions)

---

## 🎨 Key Features

1. **Real-time Weather Data**
   - Current conditions
   - 24-hour hourly forecast
   - 7-day daily forecast

2. **Location Services**
   - GPS-based current location
   - Search by city name
   - Reverse geocoding (coordinates → city name)

3. **Favorites Management**
   - Save unlimited locations
   - Quick access with tap
   - Swipe actions (delete, refresh)
   - Access tracking (counter + timestamp)

4. **Smart Caching**
   - 10-minute TTL reduces API calls
   - Offline-first approach
   - Automatic cache cleanup (keeps last 50)

5. **Cross-Platform**
   - iOS, Android, macOS, Windows (same codebase)

---

## 🔑 Interview Talking Points

### **Technical Decisions:**

1. **Why MVVM?**
   - Separation of concerns
   - Testability (can test ViewModels without UI)
   - Data binding reduces boilerplate

2. **Why SQLite?**
   - Lightweight, embedded database
   - No server required
   - Cross-platform support
   - Perfect for local caching

3. **Why Open-Meteo API?**
   - Free, no API key required
   - No rate limits
   - Comprehensive data (current + forecasts)
   - Reliable uptime

4. **Caching Strategy:**
   - Reduces API calls (better performance, lower bandwidth)
   - 10-minute TTL balances freshness vs performance
   - Offline capability (can view last weather without internet)

5. **Dependency Injection Benefits:**
   - Loose coupling (easy to swap implementations)
   - Testability (can mock dependencies)
   - Lifecycle management (Singleton vs Scoped)

6. **Repository Pattern:**
   - Abstracts database logic
   - Interface allows for testing (in-memory database in tests)
   - Single responsibility (database operations only)

### **Challenges Solved:**

1. **Test Project Configuration**
   - Problem: MAUI dependencies conflict in test project
   - Solution: Direct source file inclusion, exclude MAUI-dependent files

2. **Obsolete API Warnings**
   - Problem: `Application.MainPage` and `DisplayAlert` deprecated
   - Solution: Migrated to `Shell.Current` and `DisplayAlertAsync`

3. **Null Safety**
   - Problem: Potential null reference exceptions
   - Solution: Null-conditional operators (`?.`), guard clauses

4. **Cache Staleness**
   - Problem: How to determine if cached data is too old?
   - Solution: `IsStale` computed property (10-minute threshold)

### **Code Quality Highlights:**

- **Zero build warnings** (started with 11, fixed all)
- **46 passing tests** (100% pass rate)
- **Comprehensive test coverage** (positive + negative + edge cases)
- **Clean architecture** (proper separation of concerns)
- **Defensive programming** (guard clauses, null checks)
- **Modern APIs** (async/await, LINQ, EF Core)

---

## 📱 Demo Flow for Interview

1. **Launch App**
   - "App loads last viewed weather from cache"
   - "10-minute cache TTL reduces unnecessary API calls"

2. **Current Location**
   - "App requests location permission"
   - "Uses GPS to get coordinates"
   - "Reverse geocoding converts coordinates to city name"

3. **Search Feature**
   - "Type 'Paris' → searches Open-Meteo Geocoding API"
   - "Shows weather for Paris, France"

4. **Add to Favorites**
   - "Saves location with current temp and condition"
   - "Can access later with one tap"

5. **Favorites Tab**
   - "Shows all saved locations ordered by last accessed"
   - "Displays cached temperature and condition"
   - "Swipe actions: Refresh (updates weather), Delete (removes)"

6. **Navigation**
   - "Tap favorite → navigates to WeatherPage with that location"
   - "Updates access counter and timestamp"

7. **Caching Demo**
   - "Search for London → API call"
   - "Go to favorites, come back, search London again → cached (instant)"
   - "Wait 11 minutes, search London → API call (cache stale)"

---

## 🚀 Technologies Used

- **.NET 10 / C# 13**
- **.NET MAUI 10.0** (Multi-platform App UI)
- **Entity Framework Core 10.0** (ORM)
- **SQLite** (Database)
- **Open-Meteo API** (Weather data)
- **xUnit 2.9** (Testing framework)
- **Moq 4.20** (Mocking library)
- **FluentAssertions 7.0** (Assertion library)

---

## 📊 Project Statistics

- **Total Files:** 47 C# files
- **Lines of Code:** ~2,500+ lines (excluding tests)
- **Test Files:** 4 test classes
- **Test Coverage:** 46 tests
- **Database Tables:** 3 (FavoriteLocations, WeatherHistory, SearchHistory)
- **Build Warnings:** 0
- **Test Pass Rate:** 100%

---

Good luck with your interview! This overview covers everything from architecture to implementation details. Focus on explaining the MVVM pattern, caching strategy, and how dependency injection makes the code testable and maintainable.
