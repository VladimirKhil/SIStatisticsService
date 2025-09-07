# Fallback Source Usage Examples

## API Endpoint Usage

The `GetPackagesStatisticAsync` method now supports both primary and fallback source parameters through the Web API:

### GET Request Examples

1. **With primary source only:**
   ```
   GET /api/v1/games/packages?platform=Local&from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z&source=https://primary-source.com/packages
   ```

2. **With primary source and fallback source:**
   ```
   GET /api/v1/games/packages?platform=Local&from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z&source=https://primary-source.com/packages&fallbackSource=https://fallback-source.com/packages
   ```

### C# Client Usage

```csharp
// Using the TopPackagesRequest with fallback source
var request = new TopPackagesRequest(
    StatisticFilter: new StatisticFilter
    {
        Platform = GamePlatforms.Local,
        From = DateTimeOffset.Now.AddDays(-30),
        To = DateTimeOffset.Now
    },
    Source: new Uri("https://primary-source.com/packages"),
    FallbackSource: new Uri("https://fallback-source.com/packages")
);

var packagesStatistic = await statisticsClient.GetLatestTopPackagesAsync(request);
```

### Service Method Usage

```csharp
// Direct service call with fallback source
var packages = await gamesService.GetPackagesStatisticAsync(
    statisticFilter,
    source: new Uri("https://primary-source.com/packages"),
    fallbackSource: new Uri("https://fallback-source.com/packages")
);
```

## How Fallback Source Works

1. **Primary source found**: Returns the package with the primary source URI
2. **Primary source not found, fallback source found**: Returns the package with the fallback source URI
3. **Neither source found**: Returns the package with a null source
4. **No source filters specified**: Returns the package with any available source (first found)

## Implementation Details

- The method efficiently performs a single SQL query to retrieve package sources
- No additional database queries are made when using fallback sources
- All existing functionality remains backward compatible
- The fallback source is only used when the primary source is not found for a specific package