using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIStatisticsService.Contract.Models;

/// <summary>
/// JSON converter for QuestionKey that enables it to be used as dictionary keys.
/// </summary>
public sealed class QuestionKeyJsonConverter : JsonConverter<QuestionKey>
{
    /// <inheritdoc/>
    public override QuestionKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected string token for QuestionKey");
        }

        var value = reader.GetString();
        return ParseQuestionKey(value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, QuestionKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.RoundIndex},{value.ThemeIndex},{value.QuestionIndex}");
    }

    /// <inheritdoc/>
    public override QuestionKey ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException("Expected property name token for QuestionKey");
        }

        var value = reader.GetString();
        return ParseQuestionKey(value);
    }

    /// <inheritdoc/>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, QuestionKey value, JsonSerializerOptions options)
    {
        writer.WritePropertyName($"{value.RoundIndex},{value.ThemeIndex},{value.QuestionIndex}");
    }

    private static QuestionKey ParseQuestionKey(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("QuestionKey cannot be null or empty");
        }

        var parts = value.Split(',');
        if (parts.Length != 3)
        {
            throw new JsonException("QuestionKey must have exactly 3 comma-separated integers");
        }

        if (!int.TryParse(parts[0], out var roundIndex) ||
            !int.TryParse(parts[1], out var themeIndex) ||
            !int.TryParse(parts[2], out var questionIndex))
        {
            throw new JsonException("QuestionKey parts must be valid integers");
        }

        return new QuestionKey(roundIndex, themeIndex, questionIndex);
    }
}