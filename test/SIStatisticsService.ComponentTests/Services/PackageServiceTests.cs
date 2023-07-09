using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.ComponentTests.Services;

internal sealed class PackageServiceTests : TestsBase
{
    [Test]
    public async Task ImportQuestionReportAsync_HandleDuplicates_Ok()
    {
        var randomId = Guid.NewGuid();

        var questionReport = new QuestionReport
        {
            QuestionText = $"Test question {randomId}",
            ThemeName = $"Test theme {randomId}",
            ReportText = $"Test report {randomId}",
            ReportType = QuestionReportType.Accepted
        };

        await PackagesService.ImportQuestionReportAsync(questionReport);
        Assert.DoesNotThrowAsync(() => PackagesService.ImportQuestionReportAsync(questionReport));
    }
}