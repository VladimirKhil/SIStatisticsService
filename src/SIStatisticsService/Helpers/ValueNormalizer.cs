using System.Text.RegularExpressions;

namespace SIStatisticsService.Helpers;

/// <summary>
/// Normalizes string values.
/// </summary>
public static partial class ValueNormalizer
{
    [GeneratedRegex("\\s+")]
    private static partial Regex NormalizerRegex();

    public static string Normalize(string value) => NormalizerRegex().Replace(value.Trim(), " ");
}
