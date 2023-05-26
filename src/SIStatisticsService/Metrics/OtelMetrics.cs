using System.Diagnostics.Metrics;

namespace SIStatisticsService.Metrics;

/// <summary>
/// Holds service metrics.
/// </summary>
public sealed class OtelMetrics
{
    private Counter<int> UploadedGameReportsCounter { get; }

    private Counter<int> UploadedPackagesCounter { get; }

    public string MeterName { get; }

    public OtelMetrics(string meterName = "SIStatistics")
    {
        var meter = new Meter(meterName);
        MeterName = meterName;

        UploadedGameReportsCounter = meter.CreateCounter<int>("game-reports-uploaded");
        UploadedPackagesCounter = meter.CreateCounter<int>("packages-uploaded");
    }

    public void AddGameReport() => UploadedGameReportsCounter.Add(1);

    public void AddPackage() => UploadedPackagesCounter.Add(1);
}
