using System.Text.Json.Serialization;

namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents a question key with round, theme, and question indexes.
/// </summary>
/// <param name="RoundIndex">Round index.</param>
/// <param name="ThemeIndex">Theme index within the round.</param>
/// <param name="QuestionIndex">Question index within the theme.</param>
[JsonConverter(typeof(QuestionKeyJsonConverter))]
public sealed record QuestionKey(int RoundIndex, int ThemeIndex, int QuestionIndex);