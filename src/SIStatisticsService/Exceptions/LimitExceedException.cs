namespace SIStatisticsService.Exceptions;

/// <summary>
/// Represents an exception happened when text could not be imported because it is too long.
/// </summary>
internal sealed class LimitExceedException : Exception
{
    public LimitExceedException(Exception? innerException) : base("Limit exceed", innerException)
    {
    }
}
