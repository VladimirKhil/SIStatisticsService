namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents a game package info.
/// </summary>
public sealed class PackageInfo
{
    /// <summary>
    /// Game package name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Game package hash.
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// Game package authors.
    /// </summary>
    public string[] Authors { get; set; } = Array.Empty<string>();
}
