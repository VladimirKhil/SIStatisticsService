using System.Text.Json;
using System.Text.Json.Serialization;
using SIStatisticsService.Contract.Models;

namespace SIStatisticsService.UnitTests.Models;

[TestFixture]
internal sealed class QuestionKeyJsonConverterTests
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
    public void QuestionKey_AsValueSerialization_WorksCorrectly()
    {
        // Arrange
        var questionKey = new QuestionKey(1, 2, 3);

        // Act - serialize
        var json = JsonSerializer.Serialize(questionKey, AspNetCoreJsonOptions);
        
        // Assert - verify serialized format
        Assert.That(json, Is.EqualTo("\"1,2,3\""));

        // Act - deserialize
        var deserializedKey = JsonSerializer.Deserialize<QuestionKey>(json, AspNetCoreJsonOptions);

        // Assert - verify deserialized object
        Assert.That(deserializedKey, Is.EqualTo(questionKey));
    }

    [Test]
    public void QuestionKey_AsDictionaryKeySerialization_WorksCorrectly()
    {
        // Arrange - This is the exact scenario that was failing in the Docker release
        var dictionary = new Dictionary<QuestionKey, List<CollectedAnswer>>
        {
            [new QuestionKey(0, 1, 2)] =
            [
                new() { AnswerText = "Test Answer", RelationType = RelationType.Apellated, Count = 1 }
            ],
            [new QuestionKey(1, 0, 3)] =
            [
                new() { AnswerText = "Another Answer", RelationType = RelationType.Rejected, Count = 2 }
            ]
        };

        var result = new PackageImportResult { CollectedAnswers = dictionary };

        // Act - serialize (this was throwing NotSupportedException)
        string json;
        Assert.DoesNotThrow(() => json = JsonSerializer.Serialize(result, AspNetCoreJsonOptions));
        
        json = JsonSerializer.Serialize(result, AspNetCoreJsonOptions);

        // Assert - verify serialized structure
        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("0,1,2"));
        Assert.That(json, Does.Contain("1,0,3"));

        // Act - deserialize
        PackageImportResult? deserializedResult;
        Assert.DoesNotThrow(() => deserializedResult = JsonSerializer.Deserialize<PackageImportResult>(json, AspNetCoreJsonOptions));
        
        deserializedResult = JsonSerializer.Deserialize<PackageImportResult>(json, AspNetCoreJsonOptions);

        // Assert - verify deserialized structure
        Assert.That(deserializedResult, Is.Not.Null);
        Assert.That(deserializedResult!.CollectedAnswers, Has.Count.EqualTo(2));
        
        var firstKey = new QuestionKey(0, 1, 2);
        var secondKey = new QuestionKey(1, 0, 3);
        
        Assert.Multiple(() =>
        {
            Assert.That(deserializedResult.CollectedAnswers.ContainsKey(firstKey), Is.True);
            Assert.That(deserializedResult.CollectedAnswers.ContainsKey(secondKey), Is.True);
        });
    }

    [Test]
    public void QuestionKey_InvalidFormats_ThrowAppropriateExceptions()
    {
        // Test missing components
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuestionKey>("\"1,2\"", AspNetCoreJsonOptions));
        
        // Test too many components
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuestionKey>("\"1,2,3,4\"", AspNetCoreJsonOptions));
        
        // Test non-integer components
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuestionKey>("\"a,b,c\"", AspNetCoreJsonOptions));
        
        // Test empty string
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuestionKey>("\"\"", AspNetCoreJsonOptions));
    }

    [Test]
    public void QuestionKey_EdgeCases_HandleCorrectly()
    {
        // Test with zero values
        var zeroKey = new QuestionKey(0, 0, 0);
        var zeroJson = JsonSerializer.Serialize(zeroKey, AspNetCoreJsonOptions);
        var zeroDeserialized = JsonSerializer.Deserialize<QuestionKey>(zeroJson, AspNetCoreJsonOptions);
        Assert.That(zeroDeserialized, Is.EqualTo(zeroKey));

        // Test with negative values (if they should be supported)
        var negativeKey = new QuestionKey(-1, -2, -3);
        var negativeJson = JsonSerializer.Serialize(negativeKey, AspNetCoreJsonOptions);
        var negativeDeserialized = JsonSerializer.Deserialize<QuestionKey>(negativeJson, AspNetCoreJsonOptions);
        Assert.That(negativeDeserialized, Is.EqualTo(negativeKey));
    }
}