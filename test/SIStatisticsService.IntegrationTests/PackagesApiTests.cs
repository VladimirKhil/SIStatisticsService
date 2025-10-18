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

    [Test]
    public async Task UploadPackage_WithExistingAppellations_ReturnsCollectedAnswers()
    {
        // First, upload the package to establish the questions in the database
        using (var fs = File.OpenRead("content.xml"))
        {
            await SIStatisticsClient.SendPackageContentAsync(fs);
        }

        // Simulate user reports for apellated answers with high count
        // We need to submit multiple reports to reach the threshold (>=5)
        var questionReport = new QuestionReport
        {
            ThemeName = "THEME 1",
            QuestionText = "Text 1", 
            ReportText = "Appellated Answer",
            ReportType = QuestionReportType.Apellated
        };

        var packageInfo = new PackageInfo(
            Name: "Test Package",
            Hash: "test-hash-123",
            Authors: ["Test Author"]
        );

        var gameResultInfo = new GameResultInfo(packageInfo)
        {
            FinishTime = DateTimeOffset.UtcNow
        };

        var gameReport = new GameReport
        {
            Info = gameResultInfo,
            QuestionReports = [questionReport]
        };

        // Submit the report 6 times to exceed the threshold of 5
        for (int i = 0; i < 6; i++)
        {
            await SIStatisticsClient.SendGameReportAsync(gameReport);
        }

        // Now upload the package again and check if CollectedAnswers contains the appellated answer
        PackageImportResult? importResult;
        
        using (var fs = File.OpenRead("content.xml"))
        {
            importResult = await SIStatisticsClient.SendPackageContentAsync(fs);
        }

        // Verify the import result contains the collected answers
        Assert.That(importResult, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(importResult!.CollectedAnswers, Is.Not.Null);

            // There should now be collected answers due to the appellations we submitted
            Assert.That(importResult.CollectedAnswers, Is.Not.Empty,
                "CollectedAnswers should contain appellated answers that exceeded the threshold");
        });

        // Verify the structure of collected answers
        var firstAnswer = importResult.CollectedAnswers.First();
        
        Assert.Multiple(() =>
        {
            Assert.That(firstAnswer.Key, Is.Not.Null);
            Assert.That(firstAnswer.Value, Is.Not.Null);
        });
        
        Assert.That(firstAnswer.Value, Is.Not.Empty);

        var collectedAnswer = firstAnswer.Value.First();
        
        Assert.Multiple(() =>
        {
            Assert.That(collectedAnswer.AnswerText, Is.EqualTo("Appellated Answer"));
            Assert.That(collectedAnswer.RelationType, Is.EqualTo(RelationType.Apellated));
            Assert.That(collectedAnswer.Count, Is.GreaterThanOrEqualTo(5));
        });

        // Debug output to see the actual structure
        Console.WriteLine($"CollectedAnswers count: {importResult.CollectedAnswers.Count}");
        
        foreach (var kvp in importResult.CollectedAnswers)
        {
            Console.WriteLine($"Question {kvp.Key.RoundIndex},{kvp.Key.ThemeIndex},{kvp.Key.QuestionIndex}: {kvp.Value.Count} collected answers");
            
            foreach (var answer in kvp.Value)
            {
                Console.WriteLine($"  - {answer.AnswerText} (count: {answer.Count}, type: {answer.RelationType})");
            }
        }
    }
}
