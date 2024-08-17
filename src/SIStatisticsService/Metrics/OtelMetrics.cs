using System.Diagnostics.Metrics;

namespace SIStatisticsService.Metrics;

/// <summary>
/// Holds service metrics.
/// </summary>
public sealed class OtelMetrics
{
    public const string MeterName = "SIStatistics";

    private Counter<int> UploadedGameReportsCounter { get; }

    private Counter<int> UploadedPackagesCounter { get; }

    private Counter<int> UploadedQuestionsCounter { get; }

    private Counter<int> LimitExceedCounter { get; }

    public OtelMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        UploadedGameReportsCounter = meter.CreateCounter<int>("game-reports-uploaded");
        UploadedPackagesCounter = meter.CreateCounter<int>("packages-content-uploaded");
        UploadedQuestionsCounter = meter.CreateCounter<int>("question-reports-uploaded");
        LimitExceedCounter = meter.CreateCounter<int>("liit-exceed");
    }

    public void AddGameReport() => UploadedGameReportsCounter.Add(1);

    public void AddPackage() => UploadedPackagesCounter.Add(1);

    public void AddQuestions(int count = 1) => UploadedQuestionsCounter.Add(count);

    public void AddLimitExceed() => LimitExceedCounter.Add(1);
}
