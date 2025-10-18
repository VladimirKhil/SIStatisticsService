using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.IntegrationTests;

internal sealed class PackagesApiTests : TestsBase
{
    [Test]
    public async Task UploadPackage_Ok()
    {
        PackageImportResult? importResult;
        
        using (var fs = File.OpenRead("content.xml"))
        {
            importResult = await SIStatisticsClient.SendPackageContentAsync(fs);
        }

        // Verify the import result
        Assert.That(importResult, Is.Not.Null);
        Assert.That(importResult!.CollectedAnswers, Is.Not.Null);

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

    [Test]
    public async Task UploadPackage_ReturnsCollectedAnswers()
    {
        PackageImportResult? importResult;
        
        using (var fs = File.OpenRead("content.xml"))
        {
            importResult = await SIStatisticsClient.SendPackageContentAsync(fs);
        }

        // Verify the package import result structure
        Assert.That(importResult, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(importResult!.CollectedAnswers, Is.Not.Null);

            // The collected answers dictionary should be initialized even if empty
            Assert.That(importResult.CollectedAnswers, Is.InstanceOf<Dictionary<QuestionKey, List<CollectedAnswer>>>());
        });

        // For a fresh import, the collected answers might be empty, but the structure should be valid
        foreach (var kvp in importResult.CollectedAnswers)
        {
            Assert.Multiple(() =>
            {
                Assert.That(kvp.Key, Is.Not.Null);
                Assert.That(kvp.Value, Is.Not.Null);
            });

            foreach (var answer in kvp.Value)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(answer.AnswerText, Is.Not.Null);
                    Assert.That(answer.Count, Is.GreaterThan(0));
                    Assert.That(answer.RelationType, Is.AnyOf(RelationType.Apellated, RelationType.Rejected));
                });
            }
        }
    }
}
