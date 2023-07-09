using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class GamesServiceTests : TestsBase
{
    [Test]
    public async Task AddGameResultAsync_HandleDuplicates_Ok()
    {
        var randomId = Guid.NewGuid();

        var gameResult = new GameResultInfo
        {
            Name = $"Test game {randomId}",
            Results = new Dictionary<string, int>(),
            Reviews =new Dictionary<string, string>(),
            Package = new PackageInfo
            {
                Name = $"Test package {randomId}",
                Hash = "",
                Authors = new string[] { $"Test author 1 {randomId}", $"Test author 2 {randomId}" }
            }
        };

        await GamesService.AddGameResultAsync(gameResult);
        Assert.DoesNotThrowAsync(() => GamesService.AddGameResultAsync(gameResult));
    }
}
