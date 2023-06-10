using System.Diagnostics.Metrics;

namespace SIStatisticsService.Metrics;

/// <summary>
/// Holds service metrics.
/// </summary>
public sealed class OtelMetrics
{
    private Counter<int> UploadedGameReportsCounter { get; }

    private Counter<int> UploadedPackagesCounter { get; }

    private Counter<int> UploadedQuestionsCounter { get; }

    public string MeterName { get; }

    public OtelMetrics(string meterName = "SIStatistics")
    {
        var meter = new Meter(meterName);
        MeterName = meterName;

        UploadedGameReportsCounter = meter.CreateCounter<int>("game-reports-uploaded");
        UploadedPackagesCounter = meter.CreateCounter<int>("packages-content-uploaded");
        UploadedQuestionsCounter = meter.CreateCounter<int>("question-reports-uploaded");
    }

    public void AddGameReport() => UploadedGameReportsCounter.Add(1);

    public void AddPackage() => UploadedPackagesCounter.Add(1);

    public void AddQuestions(int count = 1) => UploadedQuestionsCounter.Add(count);
}
