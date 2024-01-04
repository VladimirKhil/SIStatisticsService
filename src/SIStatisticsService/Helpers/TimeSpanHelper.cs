namespace SIStatisticsService.Helpers;

/// <summary>
/// Provides helper method for working woth TimeSpans.
/// </summary>
internal static class TimeSpanHelper
{
    /// <summary>
    /// Adds two TimeSpan values with overflow check.
    /// </summary>
    /// <param name="value1">First value.</param>
    /// <param name="value2">Second value.</param>
    /// <returns>Sum of two values.</returns>
    internal static TimeSpan AddTimeSpan(TimeSpan value1, TimeSpan value2)
    {
        if (TimeSpan.MaxValue - value1 < value2)
        {
            return TimeSpan.MaxValue;
        }

        return value1 + value2;
    }
}
