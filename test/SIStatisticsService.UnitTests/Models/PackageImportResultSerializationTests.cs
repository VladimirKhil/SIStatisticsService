using System.Text.Json;
using System.Text.Json.Serialization;
using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.UnitTests.Models;

/// <summary>
/// Unit test to verify the PackageImportResult serialization works correctly.
/// This addresses the specific error from the Docker release environment.
/// </summary>
[TestFixture]
internal sealed class PackageImportResultSerializationTests
{
    private static readonly JsonSerializerOptions AspNetCoreJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    [Test]
    public void PackageImportResult_WithQuestionKeyDictionary_SerializesCorrectly()
    {
        // Arrange - Create a PackageImportResult with QuestionKey dictionary keys 
        // (this is the exact scenario that was failing)
        var result = new PackageImportResult();
        
        var questionKey1 = new QuestionKey(0, 1, 2);
        var questionKey2 = new QuestionKey(1, 0, 3);
        
        result.CollectedAnswers[questionKey1] =
        [
            new() { AnswerText = "Paris", RelationType = RelationType.Apellated, Count = 1 },
            new() { AnswerText = "London", RelationType = RelationType.Rejected, Count = 2 }
        ];
        
        result.CollectedAnswers[questionKey2] =
        [
            new() { AnswerText = "Four", RelationType = RelationType.Apellated, Count = 3 }
        ];

        // Act - serialize (this was throwing the original exception)
        string json;
        Assert.DoesNotThrow(() =>
        {
            json = JsonSerializer.Serialize(result, AspNetCoreJsonOptions);
        }, "Serialization should not throw an exception");

        json = JsonSerializer.Serialize(result, AspNetCoreJsonOptions);

        // Assert - verify serialized JSON contains the expected structure
        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("0,1,2"), "Should contain first question key");
        Assert.That(json, Does.Contain("1,0,3"), "Should contain second question key");
        Assert.That(json, Does.Contain("Paris"), "Should contain answer text");
        Assert.That(json, Does.Contain("apellated"), "Should contain relation type in camelCase");

        // Act - deserialize back
        PackageImportResult? deserializedResult = null;
        
        Assert.DoesNotThrow(() =>
        {
            deserializedResult = JsonSerializer.Deserialize<PackageImportResult>(json, AspNetCoreJsonOptions);
        }, "Deserialization should not throw an exception");

        // Assert - verify deserialized structure
        Assert.That(deserializedResult, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(deserializedResult!.CollectedAnswers, Has.Count.EqualTo(2));

            Assert.That(deserializedResult.CollectedAnswers.ContainsKey(questionKey1), Is.True);
            Assert.That(deserializedResult.CollectedAnswers.ContainsKey(questionKey2), Is.True);
        });

        var deserializedAnswers1 = deserializedResult.CollectedAnswers[questionKey1];
        var deserializedAnswers2 = deserializedResult.CollectedAnswers[questionKey2];
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedAnswers1, Has.Count.EqualTo(2));
            Assert.That(deserializedAnswers2, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedAnswers1.Any(a => a.AnswerText == "Paris" && a.RelationType == RelationType.Apellated), Is.True);
            Assert.That(deserializedAnswers1.Any(a => a.AnswerText == "London" && a.RelationType == RelationType.Rejected), Is.True);
            Assert.That(deserializedAnswers2.Any(a => a.AnswerText == "Four" && a.RelationType == RelationType.Apellated), Is.True);
        }
    }
}