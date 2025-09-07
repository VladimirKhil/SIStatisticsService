using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class GamesServiceSourceTests : TestsBase
{
	[Test]
    public async Task GetPackagesStatisticAsync_WithSourceFilter_PackageWithoutSource_ReturnedWithNullSource()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-1); // Use unique time to avoid test interference

        // Create a game result with a package that has no source
        var gameResult = new GameResultInfo(new PackageInfo($"Test package no source {randomId}", "hash123", [$"Test author {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages with source filter - should return the package but with null source
        var requestedSource = new Uri("https://example.com/packages");
        var packagesWithFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, requestedSource));

        var packageWithoutSource = packagesWithFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package no source {randomId}");
        Assert.That(packageWithoutSource, Is.Not.Null, "Package without source should still be returned when source filter is applied");
        Assert.That(packageWithoutSource!.Package!.Source, Is.Null, "Package source should be null when it doesn't match the filter");
    }

	[Test]
    public async Task GetPackagesStatisticAsync_WithSourceFilter_PackageWithDifferentSource_ReturnedWithNullSource()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-2); // Use unique time to avoid test interference

        // Create a game result with a package that has a different source
        var packageSource = new Uri("https://different-source.com/packages");

		var gameResult = new GameResultInfo(new PackageInfo($"Test package different source {randomId}", "hash456", [$"Test author {randomId}"], null, packageSource))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages with different source filter - should return the package but with null source
        var requestedSource = new Uri("https://requested-source.com/packages");
        var packagesWithFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, requestedSource));

        var packageWithDifferentSource = packagesWithFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package different source {randomId}");
        Assert.That(packageWithDifferentSource, Is.Not.Null, "Package with different source should still be returned when different source filter is applied");
        Assert.That(packageWithDifferentSource!.Package!.Source, Is.Null, "Package source should be null when it doesn't match the requested source");
    }

	[Test]
    public async Task GetPackagesStatisticAsync_WithSourceFilter_PackageWithMatchingSource_Returned()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-3); // Use unique time to avoid test interference

        // Create a game result with a package that has the requested source
        var packageSource = new Uri("https://matching-source.com/packages");

		var gameResult = new GameResultInfo(new PackageInfo($"Test package matching source {randomId}", "hash789", [$"Test author {randomId}"], null, packageSource))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages with matching source filter - should return the package with correct source
        var packagesWithFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, packageSource));

        var packageWithMatchingSource = packagesWithFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package matching source {randomId}");
        Assert.That(packageWithMatchingSource, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithMatchingSource.Package!.Source, Is.EqualTo(packageSource));
			Assert.That(packageWithMatchingSource.GameCount, Is.EqualTo(1));
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithSourceFilter_PackageWithMultipleSources_FiltersCorrectly()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-4); // Use unique time to avoid test interference

        // Create multiple games with the same package but different sources
        var source1 = new Uri("https://source1.com/packages");
        var source2 = new Uri("https://source2.com/packages");

        var gameResult1 = new GameResultInfo(new PackageInfo($"Test package multi source {randomId}", "hashMulti", [$"Test author {randomId}"], null, source1))
        {
            Name = $"Test game 1 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package multi source {randomId}", "hashMulti", [$"Test author {randomId}"], null, source2))
        {
            Name = $"Test game 2 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime.AddMinutes(1)
        };

        await GamesService.AddGameResultAsync(gameResult1);
        await GamesService.AddGameResultAsync(gameResult2);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.AddMinutes(2),
            Platform = GamePlatforms.Local
        };

        // Get packages with source1 filter - should return the package with source1
        var packagesWithSource1Filter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, source1));

        var packageWithSource1 = packagesWithSource1Filter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package multi source {randomId}");
        Assert.That(packageWithSource1, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithSource1.Package!.Source, Is.EqualTo(source1));
			Assert.That(packageWithSource1.GameCount, Is.EqualTo(2), "Game count should include all games for the package regardless of source");
		});

		// Get packages with source2 filter - should return the package with source2
		var packagesWithSource2Filter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, source2));

        var packageWithSource2 = packagesWithSource2Filter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package multi source {randomId}");
        Assert.That(packageWithSource2, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithSource2.Package!.Source, Is.EqualTo(source2));
			Assert.That(packageWithSource2.GameCount, Is.EqualTo(2), "Game count should include all games for the package regardless of source");
		});

		// Get packages with non-existent source filter - should return the package but with null source
		var nonExistentSource = new Uri("https://non-existent.com/packages");
        var packagesWithNonExistentFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, nonExistentSource));

        var packageWithNonExistentSource = packagesWithNonExistentFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package multi source {randomId}");
        Assert.That(packageWithNonExistentSource, Is.Not.Null, "Package should still be returned when filtered by non-existent source");

		Assert.Multiple(() =>
		{
			Assert.That(packageWithNonExistentSource.Package!.Source, Is.Null, "Package source should be null when no matching source is found");
			Assert.That(packageWithNonExistentSource.GameCount, Is.EqualTo(2), "Game count should include all games for the package regardless of source");
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithSourceFilter_MixedSourceScenario_FiltersCorrectly()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-5); // Use unique time to avoid test interference

        // Create packages with different source scenarios
        var targetSource = new Uri("https://target-source.com/packages");
        var otherSource = new Uri("https://other-source.com/packages");

        // Package 1: Has target source
        var gameResult1 = new GameResultInfo(new PackageInfo($"Package with target source {randomId}", "hash1", ["Author1"], null, targetSource))
        {
            Name = $"Game 1 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        // Package 2: Has different source
        var gameResult2 = new GameResultInfo(new PackageInfo($"Package with other source {randomId}", "hash2", ["Author2"], null, otherSource))
        {
            Name = $"Game 2 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        // Package 3: Has no source
        var gameResult3 = new GameResultInfo(new PackageInfo($"Package without source {randomId}", "hash3", ["Author3"]))
        {
            Name = $"Game 3 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        // Package 4: Another game with target source (same package as Package 1)
        var gameResult4 = new GameResultInfo(new PackageInfo($"Package with target source {randomId}", "hash1", ["Author1"], null, targetSource))
        {
            Name = $"Game 4 {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime.AddMinutes(1)
        };

        await GamesService.AddGameResultAsync(gameResult1);
        await GamesService.AddGameResultAsync(gameResult2);
        await GamesService.AddGameResultAsync(gameResult3);
        await GamesService.AddGameResultAsync(gameResult4);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.AddMinutes(2),
            Platform = GamePlatforms.Local
        };

        // Get packages with target source filter - all packages should be returned but with appropriate sources
        var packagesWithTargetSource = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, targetSource));

        // Find the package that should have the target source
        var targetPackage = packagesWithTargetSource.Packages.FirstOrDefault(p => p.Package!.Name == $"Package with target source {randomId}");
        Assert.That(targetPackage, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(targetPackage.Package!.Source, Is.EqualTo(targetSource));
			Assert.That(targetPackage.GameCount, Is.EqualTo(2), "Should count both games for the same package");
		});

		// Find packages that should have null sources due to no match
		var otherSourcePackage = packagesWithTargetSource.Packages.FirstOrDefault(p => p.Package!.Name == $"Package with other source {randomId}");
        Assert.That(otherSourcePackage, Is.Not.Null, "Package with different source should still be returned");
        Assert.That(otherSourcePackage.Package!.Source, Is.Null, "Package with different source should have null source when filtered");

        var noSourcePackage = packagesWithTargetSource.Packages.FirstOrDefault(p => p.Package!.Name == $"Package without source {randomId}");
        Assert.That(noSourcePackage, Is.Not.Null, "Package without source should still be returned");
        Assert.That(noSourcePackage.Package!.Source, Is.Null, "Package without source should have null source when filtered");
    }

	[Test]
    public async Task GetPackagesStatisticAsync_WithoutSourceFilter_PackageWithoutSource_IncludedWithNullSource()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-6); // Use unique time to avoid test interference

        // Create a game result with a package that has no source
        var gameResult = new GameResultInfo(new PackageInfo($"Test package no source verify {randomId}", "hash123", [$"Test author {randomId}"]))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages without source filter - should return the package with null source
        var packagesWithoutFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter));

        var packageWithoutSource = packagesWithoutFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package no source verify {randomId}");
        Assert.That(packageWithoutSource, Is.Not.Null, "Package without source should be returned when no source filter is applied");

		Assert.Multiple(() =>
		{
			Assert.That(packageWithoutSource!.Package!.Source, Is.Null, "Package source should be null when no source was provided");
			Assert.That(packageWithoutSource.GameCount, Is.EqualTo(1));
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithoutSourceFilter_PackageWithSource_IncludedWithSource()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-7); // Use unique time to avoid test interference

        // Create a game result with a package that has a source
        var packageSource = new Uri("https://verify-source.com/packages");
        var gameResult = new GameResultInfo(new PackageInfo($"Test package with source verify {randomId}", "hash456", [$"Test author {randomId}"], null, packageSource))
        {
            Name = $"Test game {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages without source filter - should return the package with its source
        var packagesWithoutFilter = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter));

        var packageWithSource = packagesWithoutFilter.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package with source verify {randomId}");
        Assert.That(packageWithSource, Is.Not.Null, "Package with source should be returned when no source filter is applied");

		Assert.Multiple(() =>
		{
			Assert.That(packageWithSource!.Package!.Source, Is.EqualTo(packageSource), "Package source should match the provided source");
			Assert.That(packageWithSource.GameCount, Is.EqualTo(1));
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithPrimaryAndFallbackSource_FallsBackWhenPrimaryNotFound()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-8); // Use unique time to avoid test interference

        var primarySource = new Uri("https://primary-source.com/packages");
        var fallbackSource = new Uri("https://fallback-source.com/packages");

        // Create a package with only the fallback source
        var gameResult = new GameResultInfo(new PackageInfo($"Test package fallback {randomId}", "hashFallback", [$"Test author {randomId}"], null, fallbackSource))
        {
            Name = $"Test game fallback {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages with primary source (not found) and fallback source (should be used)
        var packagesWithFallback = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, primarySource, fallbackSource));

        var packageWithFallbackSource = packagesWithFallback.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package fallback {randomId}");
        Assert.That(packageWithFallbackSource, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithFallbackSource.Package!.Source, Is.EqualTo(fallbackSource), "Should use fallback source when primary is not found");
			Assert.That(packageWithFallbackSource.GameCount, Is.EqualTo(1));
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithPrimaryAndFallbackSource_UsesPrimaryWhenAvailable()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-9); // Use unique time to avoid test interference

        var primarySource = new Uri("https://primary-source-available.com/packages");
        var fallbackSource = new Uri("https://fallback-source-available.com/packages");

        // Create multiple games with the same package but different sources
        var gameResult1 = new GameResultInfo(new PackageInfo($"Test package primary fallback {randomId}", "hashPrimary", [$"Test author {randomId}"], null, primarySource))
        {
            Name = $"Test game primary {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        var gameResult2 = new GameResultInfo(new PackageInfo($"Test package primary fallback {randomId}", "hashPrimary", [$"Test author {randomId}"], null, fallbackSource))
        {
            Name = $"Test game fallback {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime.AddMinutes(1)
        };

        await GamesService.AddGameResultAsync(gameResult1);
        await GamesService.AddGameResultAsync(gameResult2);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.AddMinutes(2),
            Platform = GamePlatforms.Local
        };

        // Get packages with both primary and fallback sources available - should use primary
        var packagesWithBothSources = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, primarySource, fallbackSource));

        var packageWithBothSources = packagesWithBothSources.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package primary fallback {randomId}");
        Assert.That(packageWithBothSources, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithBothSources.Package!.Source, Is.EqualTo(primarySource), "Should use primary source when both primary and fallback are available");
			Assert.That(packageWithBothSources.GameCount, Is.EqualTo(2));
		});
	}

	[Test]
    public async Task GetPackagesStatisticAsync_WithPrimaryAndFallbackSource_ReturnsNullWhenNeitherFound()
    {
        var randomId = Guid.NewGuid();
        var testStartTime = DateTimeOffset.Now.AddDays(-10); // Use unique time to avoid test interference

        var primarySource = new Uri("https://not-found-primary.com/packages");
        var fallbackSource = new Uri("https://not-found-fallback.com/packages");
        var actualSource = new Uri("https://actual-source.com/packages");

        // Create a package with a different source than primary and fallback
        var gameResult = new GameResultInfo(new PackageInfo($"Test package neither source {randomId}", "hashNeither", [$"Test author {randomId}"], null, actualSource))
        {
            Name = $"Test game neither {randomId}",
            Results = [],
            Reviews = [],
            FinishTime = testStartTime
        };

        await GamesService.AddGameResultAsync(gameResult);

        var filter = new StatisticFilter
        {
            From = testStartTime.Subtract(TimeSpan.FromMinutes(1)),
            To = testStartTime.Add(TimeSpan.FromMinutes(1)),
            Platform = GamePlatforms.Local
        };

        // Get packages with primary and fallback sources that don't match - should return null source
        var packagesWithNeitherSource = await GamesService.GetPackagesStatisticAsync(new TopPackagesRequest(filter, primarySource, fallbackSource));

        var packageWithNeitherSource = packagesWithNeitherSource.Packages.FirstOrDefault(p => p.Package!.Name == $"Test package neither source {randomId}");
        Assert.That(packageWithNeitherSource, Is.Not.Null);

		Assert.Multiple(() =>
		{
			Assert.That(packageWithNeitherSource.Package!.Source, Is.Null, "Should return null source when neither primary nor fallback sources are found");
			Assert.That(packageWithNeitherSource.GameCount, Is.EqualTo(1));
		});
	}
}
