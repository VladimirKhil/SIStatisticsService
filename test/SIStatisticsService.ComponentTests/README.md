# Component Tests with TestContainers

This document explains how to use the component tests with TestContainers for the SIStatisticsService project.

## Overview

The component tests now use TestContainers to provide an isolated PostgreSQL database for testing. This approach offers several benefits:

- **Isolation**: Each test run gets a fresh database instance
- **Consistency**: Tests run in the same environment regardless of local setup
- **No external dependencies**: No need to manually set up a local PostgreSQL instance
- **Automatic cleanup**: Containers are automatically removed after tests complete

## Setup

The TestContainer implementation is located in:
- `TestContainerBase.cs` - Base class that manages the PostgreSQL container
- `TestsBase.cs` - Inherits from TestContainerBase and provides test services

### Prerequisites

- Docker must be installed and running on your machine
- .NET 8.0 SDK

### Dependencies

The following NuGet packages have been added:
- `Testcontainers.PostgreSql` (4.0.0) - Provides PostgreSQL TestContainer support
- `FluentMigrator.Runner` (6.2.0) - For applying database migrations during tests

## How It Works

1. **Container Startup**: Before any tests run, a PostgreSQL container is started with:
   - Image: `postgres:15-alpine`
   - Database: `sistatistics`
   - Username: `testuser`
   - Password: `testpass`

2. **Service Configuration**: The test base class configures all necessary services:
   - Database connection to the test container
   - Migration runner for applying schema changes
   - Business services (GamesService, PackagesService)
   - Logging and metrics

3. **Migration Application**: All database migrations are automatically applied to the test database

4. **Test Execution**: Tests run against the containerized database

5. **Cleanup**: After all tests complete, the container is automatically disposed

## Running Tests

### Run All Component Tests
```powershell
dotnet test test/SIStatisticsService.ComponentTests/SIStatisticsService.ComponentTests.csproj
```

### Run Specific Tests
```powershell
dotnet test test/SIStatisticsService.ComponentTests/SIStatisticsService.ComponentTests.csproj --filter "Name~AddGameResult"
```

### Run with Verbose Output
```powershell
dotnet test test/SIStatisticsService.ComponentTests/SIStatisticsService.ComponentTests.csproj --verbosity normal
```

## Test Results

As of the implementation:
- ✅ **8 tests passing** - Core functionality works correctly
- ❌ **2 tests failing** - Complex LINQ queries in `GetPackagesStatistic_CountFilter_Ok`

The failing tests are related to complex LINQ-to-SQL translation issues in LinqToDB, not the TestContainer setup.

## Architecture

### TestContainerBase Class

The `TestContainerBase` class provides:
- PostgreSQL container lifecycle management
- Service configuration and dependency injection
- Database migration application
- Automatic cleanup using NUnit's `[OneTimeSetUp]` and `[OneTimeTearDown]`

### Services Available in Tests

- `IPackagesService PackagesService` - For package-related operations
- `IGamesService GamesService` - For game-related operations
- Full database context with applied migrations
- Logging and metrics infrastructure

## Benefits

1. **Fast Feedback**: Tests run quickly without external database setup
2. **Reliable**: No shared state between test runs
3. **Portable**: Works on any machine with Docker
4. **CI/CD Ready**: Perfect for automated build pipelines
5. **Production-like**: Uses the same PostgreSQL version as production

## Troubleshooting

### Docker Issues
If tests fail to start:
1. Ensure Docker is running
2. Check Docker permissions
3. Verify PostgreSQL image can be pulled

### Test Failures
- Most failures are business logic related, not container setup
- Container setup failures will prevent all tests from running
- Check the test output for connection string and migration errors

## Best Practices

1. **Inherit from TestsBase**: All component test classes should inherit from `TestsBase`
2. **Use Unique Data**: Generate unique test data using `Guid.NewGuid()`
3. **Test Isolation**: Don't rely on data from other tests
4. **Performance**: The container starts once per test class, so startup cost is amortized

## Example Test

```csharp
[TestFixture]
internal sealed class MyComponentTests : TestsBase
{
    [Test]
    public async Task MyTest_Should_Work()
    {
        // Arrange
        var randomId = Guid.NewGuid();
        var gameResult = new GameResultInfo(new PackageInfo($"Test {randomId}", "hash", ["Author"]));
        
        // Act
        var gameId = await GamesService.AddGameResultAsync(gameResult);
        
        // Assert
        Assert.That(gameId, Is.GreaterThan(0));
    }
}
```

This setup provides a robust, isolated testing environment that closely mirrors production while remaining fast and maintainable.
