namespace SIStatisticsService.Contract.Models;

/// <summary>
/// Represents a game package info.
/// </summary>
/// <param name="Name">Package name.</param>
/// <param name="Hash">Package hash.</param>
/// <param name="Authors">Package authors.</param>
/// <param name="AuthorsContacts">Package author contacts.</param>
public sealed record PackageInfo(string? Name, string? Hash, string[] Authors, string? AuthorsContacts = null);
