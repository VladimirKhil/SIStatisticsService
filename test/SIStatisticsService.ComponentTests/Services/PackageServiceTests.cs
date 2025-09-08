using SIPackages;
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

    [Test]
    public async Task ImportPackageAsync_CollectsAppellatedAndRejectedAnswers_Ok()
    {
        var uniqueId = Guid.NewGuid().ToString();
        
        // Create simple test question texts to avoid SIPackages GetText() issues
        var questionText1 = $"Q1_{uniqueId}";
        var questionText2 = $"Q2_{uniqueId}";
        var questionText3 = $"Q3_{uniqueId}";
        
        // First, populate the database with appellated and rejected answers via question reports
        var questionReport1 = new QuestionReport
        {
            QuestionText = questionText1,
            ThemeName = $"Geography {uniqueId}",
            ReportText = "Paris",
            ReportType = QuestionReportType.Apellated
        };

        var questionReport2 = new QuestionReport
        {
            QuestionText = questionText1,
            ThemeName = $"Geography {uniqueId}",
            ReportText = "London",
            ReportType = QuestionReportType.Rejected
        };

        var questionReport3 = new QuestionReport
        {
            QuestionText = questionText2,
            ThemeName = $"Mathematics {uniqueId}",
            ReportText = "Four",
            ReportType = QuestionReportType.Apellated
        };

        var questionReport4 = new QuestionReport
        {
            QuestionText = questionText3,
            ThemeName = $"Science {uniqueId}",
            ReportText = "Blue",
            ReportType = QuestionReportType.Accepted  // This should NOT be collected
        };

        // Import the question reports to populate the database
        await PackagesService.ImportQuestionReportAsync(questionReport1);
        await PackagesService.ImportQuestionReportAsync(questionReport2);
        await PackagesService.ImportQuestionReportAsync(questionReport3);
        await PackagesService.ImportQuestionReportAsync(questionReport4);

        // Create a mock package using SIPackages objects
        var package = new Package();
        
        var round = new Round { Name = "Test Round" };
        package.Rounds.Add(round);

        // Theme 1: Geography with the question that has appellated/rejected answers
        var theme1 = new Theme { Name = $"Geography {uniqueId}" };
        var question1 = new Question();
        // Set the text parameter to match what we stored in the database
        var question1Body = new List<ContentItem> { new() { Value = questionText1 } };
        question1.Parameters["question"] = new StepParameter { ContentValue = question1Body };
        question1.Right.Add("Paris (official)");
        question1.Wrong.Add("Berlin");
        theme1.Questions.Add(question1);
        round.Themes.Add(theme1);

        // Theme 2: Mathematics with the question that has appellated answers
        var theme2 = new Theme { Name = $"Mathematics {uniqueId}" };
        var question2 = new Question();
        var question2Body = new List<ContentItem> { new() { Value = questionText2 } };
        question2.Parameters["question"] = new StepParameter { ContentValue = question2Body };
        question2.Right.Add("4");
        question2.Wrong.Add("5");
        theme2.Questions.Add(question2);
        round.Themes.Add(theme2);

        // Theme 3: Science with accepted answer (should not be collected)
        var theme3 = new Theme { Name = $"Science {uniqueId}" };
        var question3 = new Question();
        var question3Body = new List<ContentItem> { new() { Value = questionText3 } };
        question3.Parameters["question"] = new StepParameter { ContentValue = question3Body };
        question3.Right.Add("Blue");
        question3.Wrong.Add("Red");
        theme3.Questions.Add(question3);
        round.Themes.Add(theme3);

        // Act: Import the package and collect the results
        var result = await PackagesService.ImportPackageAsync(package);

        // Assert: Verify the results
        Assert.That(result, Is.Not.Null, "Import result should not be null");

        Assert.That(result.CollectedAnswers, Is.Not.Empty, "Should have collected some answers");

        // Verify Geography question (Round 0, Theme 0, Question 0) has both appellated and rejected
        var geographyKey = new QuestionKey(0, 0, 0);
        Assert.That(result.CollectedAnswers.ContainsKey(geographyKey), Is.True, 
            "Should contain geography question key");

        var geographyAnswers = result.CollectedAnswers[geographyKey];
        Assert.That(geographyAnswers, Has.Count.EqualTo(2), 
            "Geography question should have 2 collected answers (1 appellated + 1 rejected)");

        var appellatedAnswer = geographyAnswers.FirstOrDefault(a => a.RelationType == RelationType.Apellated);
        var rejectedAnswer = geographyAnswers.FirstOrDefault(a => a.RelationType == RelationType.Rejected);

        Assert.Multiple(() =>
        {
            Assert.That(appellatedAnswer, Is.Not.Null, "Should have appellated answer");
            Assert.That(rejectedAnswer, Is.Not.Null, "Should have rejected answer");
        });

        Assert.Multiple(() =>
        {
            Assert.That(appellatedAnswer!.AnswerText, Is.EqualTo("Paris"), "Appellated answer text should match");
            Assert.That(appellatedAnswer.Count, Is.GreaterThan(0), "Appellated answer should have count > 0");
        });

        Assert.Multiple(() =>
        {
            Assert.That(rejectedAnswer!.AnswerText, Is.EqualTo("London"), "Rejected answer text should match");
            Assert.That(rejectedAnswer.Count, Is.GreaterThan(0), "Rejected answer should have count > 0");
        });

        // Verify Mathematics question (Round 0, Theme 1, Question 0) has only appellated
        var mathKey = new QuestionKey(0, 1, 0);
        Assert.That(result.CollectedAnswers.ContainsKey(mathKey), Is.True,
            "Should contain mathematics question key");

        var mathAnswers = result.CollectedAnswers[mathKey];
        Assert.That(mathAnswers, Has.Count.EqualTo(1), 
            "Mathematics question should have 1 collected answer (appellated only)");

        var mathAppellatedAnswer = mathAnswers.FirstOrDefault(a => a.RelationType == RelationType.Apellated);
        Assert.That(mathAppellatedAnswer, Is.Not.Null, "Should have math appellated answer");
        
        Assert.Multiple(() =>
        {
            Assert.That(mathAppellatedAnswer!.AnswerText, Is.EqualTo("Four"), "Math appellated answer text should match");
            Assert.That(mathAppellatedAnswer.Count, Is.GreaterThan(0), "Math appellated answer should have count > 0");
        });

        // Verify Science question (Round 0, Theme 2, Question 0) should NOT be collected 
        // because it only has "Accepted" answers, not "Appellated" or "Rejected"
        var scienceKey = new QuestionKey(0, 2, 0);
        Assert.That(result.CollectedAnswers.ContainsKey(scienceKey), Is.False,
            "Should NOT contain science question key because it only has accepted answers");

        // Verify that only appellated and rejected answers are collected
        var allCollectedAnswers = result.CollectedAnswers.Values.SelectMany(answers => answers);
        Assert.That(allCollectedAnswers.All(a => a.RelationType == RelationType.Apellated || a.RelationType == RelationType.Rejected),
            Is.True, "Should only collect appellated and rejected answers");
    }
}