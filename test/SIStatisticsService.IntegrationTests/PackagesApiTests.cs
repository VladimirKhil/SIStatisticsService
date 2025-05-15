using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.IntegrationTests;

internal sealed class PackagesApiTests : TestsBase
{
    [Test]
    public async Task UploadPackage_Ok()
    {
        using (var fs = File.OpenRead("content.xml"))
        {
            await SIStatisticsClient.SendPackageContentAsync(fs);
        }

        var questionInfo = await SIStatisticsClient.GetQuestionInfoAsync("THEME 1", "Text 1");

        Assert.That(questionInfo, Is.Not.Null);

        Assert.That(questionInfo!.Entities.Count, Is.EqualTo(4));

        var right1 = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Right answer 1");
        Assert.That(right1, Is.Not.Null);

        Assert.That(right1, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(right1!.RelationType, Is.EqualTo(RelationType.Right));

        var wrong1 = questionInfo.Entities.FirstOrDefault(e => e.EntityName == "Wrong answer 1");
        Assert.That(wrong1, Is.Not.Null);

        Assert.That(wrong1, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(wrong1!.RelationType, Is.EqualTo(RelationType.Wrong));
    }
}
