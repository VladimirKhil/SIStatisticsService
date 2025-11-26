using LinqToDB;
using Microsoft.Extensions.Options;
using Npgsql;
using SIPackages;
using SIPackages.Core;
using SIStatisticsService.Configuration;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Exceptions;
using SIStatisticsService.Helpers;
using SIStatisticsService.Metrics;

namespace SIStatisticsService.Services;

/// <inheritdoc cref="IPackagesService" />
public sealed class PackagesService(
    SIStatisticsDbConnection connection,
    OtelMetrics metrics,
    IOptions<SIStatisticsServiceOptions> options,
    ILogger<PackagesService> logger) : IPackagesService
{
    private readonly SIStatisticsServiceOptions _options = options.Value;

    public async Task<QuestionInfoResponse> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken)
    {
        // Normalize the input parameters to match how they are stored in the database
        var normalizedThemeName = ValueNormalizer.Normalize(themeName);
        var normalizedQuestionText = ValueNormalizer.Normalize(questionText);

        var query =
            from r in connection.Relations
            join t in connection.Themes on r.ThemeId equals t.Id
            join q in connection.Questions on r.QuestionId equals q.Id
            join e in connection.Entities on r.EntityId equals e.Id
            where t.Name == normalizedThemeName && q.Text == normalizedQuestionText
            select new EntityInfoResponse
            {
                EntityName = e.Name,
                RelationType = MapType(r.Type),
                Count = r.Count
            };

        return new QuestionInfoResponse
        {
            Entities = await query.ToArrayAsync(cancellationToken)
        };
    }

    public async Task<PackageImportResult> ImportPackageAsync(Package package, CancellationToken cancellationToken)
    {
        var result = new PackageImportResult();

        for (int roundIndex = 0; roundIndex < package.Rounds.Count; roundIndex++)
        {
            var round = package.Rounds[roundIndex];

            for (int themeIndex = 0; themeIndex < round.Themes.Count; themeIndex++)
            {
                var theme = round.Themes[themeIndex];
                int themeId;

                try
                {
                    themeId = await InsertThemeNameAsync(theme.Name, cancellationToken);
                }
                catch (LimitExceedException)
                {
                    metrics.AddLimitExceed();
                    continue;
                }

                for (int questionIndex = 0; questionIndex < theme.Questions.Count; questionIndex++)
                {
                    var question = theme.Questions[questionIndex];

                    if (question.GetContent().Any(ci => ci.Type != ContentTypes.Text))
                    {
                        // Skip non-text questions
                        continue;
                    }

                    var questionText = question.GetText();
                    int questionId;

                    try
                    {
                        questionId = await InsertQuestionTextAsync(questionText, cancellationToken);
                    }
                    catch (LimitExceedException)
                    {
                        metrics.AddLimitExceed();
                        continue;
                    }

                    foreach (var answer in question.Right)
                    {
                        await InsertOrUpdateAnswer(themeId, questionId, answer, Database.Models.Questions.RelationType.Right, cancellationToken);
                    }

                    foreach (var answer in question.Wrong)
                    {
                        await InsertOrUpdateAnswer(themeId, questionId, answer, Database.Models.Questions.RelationType.Wrong, cancellationToken);
                    }

                    // Collect appellated and rejected answers for this question
                    var collectedAnswers = await GetAppellatedAndRejectedAnswersAsync(themeId, questionId, cancellationToken);

                    if (collectedAnswers.Count != 0)
                    {
                        var questionKey = new QuestionKey(roundIndex, themeIndex, questionIndex);
                        result.CollectedAnswers[questionKey] = collectedAnswers;
                    }
                }
            }
        }

        metrics.AddPackage();
        return result;
    }

    public async Task ImportQuestionReportAsync(QuestionReport questionReport, CancellationToken cancellationToken)
    {
        int themeId, questionId;

        try
        {
            themeId = await InsertThemeNameAsync(questionReport.ThemeName ?? "", cancellationToken);
            questionId = await InsertQuestionTextAsync(questionReport.QuestionText ?? "", cancellationToken);
        }
        catch (LimitExceedException)
        {
            metrics.AddLimitExceed();
            return;
        }

        await InsertOrUpdateAnswer(
            themeId,
            questionId,
            questionReport.ReportText ?? "",
            ReverseMapType(questionReport.ReportType),
            cancellationToken);
    }

    private static RelationType MapType(Database.Models.Questions.RelationType type) =>
        type switch
        {
            Database.Models.Questions.RelationType.Right => RelationType.Right,
            Database.Models.Questions.RelationType.Wrong => RelationType.Wrong,
            Database.Models.Questions.RelationType.Apellated => RelationType.Apellated,
            Database.Models.Questions.RelationType.Accepted => RelationType.Accepted,
            Database.Models.Questions.RelationType.Rejected => RelationType.Rejected,
            Database.Models.Questions.RelationType.Complained => RelationType.Complained,
            _ => throw new NotSupportedException()
        };

    private static Database.Models.Questions.RelationType ReverseMapType(QuestionReportType type) =>
        type switch
        {
            QuestionReportType.Apellated => Database.Models.Questions.RelationType.Apellated,
            QuestionReportType.Accepted => Database.Models.Questions.RelationType.Accepted,
            QuestionReportType.Rejected => Database.Models.Questions.RelationType.Rejected,
            QuestionReportType.Complained => Database.Models.Questions.RelationType.Complained,
            _ => throw new NotSupportedException()
        };

    private async Task InsertOrUpdateAnswer(
        int themeId,
        int questionId,
        string answer,
        Database.Models.Questions.RelationType relationType,
        CancellationToken cancellationToken)
    {
        int entityId;

        try
        {
            entityId = await InsertEntityAsync(answer, cancellationToken);
        }
        catch (LimitExceedException)
        {
            metrics.AddLimitExceed();
            return;
        }

        await connection.Relations.InsertOrUpdateAsync(
            () => new Database.Models.Questions.RelationModel
            {
                ThemeId = themeId,
                QuestionId = questionId,
                EntityId = entityId,
                Count = 1,
                Type = relationType
            },
            relation => new Database.Models.Questions.RelationModel
            {
                ThemeId = themeId,
                QuestionId = questionId,
                EntityId = entityId,
                Count = relation.Count + 1,
                Type = relationType
            },
            () => new Database.Models.Questions.RelationModel
            {
                ThemeId = themeId,
                QuestionId = questionId,
                EntityId = entityId,
                Type = relationType
            },
            cancellationToken);

        metrics.AddQuestions();
    }

    private async Task<int> InsertEntityAsync(string entityName, CancellationToken cancellationToken)
    {
        entityName = ValueNormalizer.Normalize(entityName);

        try
        {
            await connection.Entities.InsertOrUpdateAsync(
                () => new Database.Models.Questions.EntityModel
                {
                    Name = entityName
                },
                null,
                () => new Database.Models.Questions.EntityModel
                {
                    Name = entityName
                },
                cancellationToken);
        }
        catch (PostgresException exc) when (exc.SqlState == PostgresErrorCodes.ProgramLimitExceeded)
        {
            LogLimit(entityName, exc);
            throw new LimitExceedException(exc);
        }

        return (await connection.Entities.FirstAsync(e => e.Name == entityName, token: cancellationToken)).Id;
    }

    private async Task<int> InsertQuestionTextAsync(string questionText, CancellationToken cancellationToken)
    {
        questionText = ValueNormalizer.Normalize(questionText);

        try
        {
            await connection.Questions.InsertOrUpdateAsync(
                () => new Database.Models.Questions.QuestionModel
                {
                    Text = questionText
                },
                null,
                () => new Database.Models.Questions.QuestionModel
                {
                    Text = questionText
                },
                cancellationToken);
        }
        catch (PostgresException exc) when (exc.SqlState == PostgresErrorCodes.ProgramLimitExceeded)
        {
            LogLimit(questionText, exc);
            throw new LimitExceedException(exc);
        }

        return (await connection.Questions.FirstAsync(q => q.Text == questionText, token: cancellationToken)).Id;
    }

    private async Task<int> InsertThemeNameAsync(string themeName, CancellationToken cancellationToken)
    {
        themeName = ValueNormalizer.Normalize(themeName);

        try
        {
            await connection.Themes.InsertOrUpdateAsync(
                () => new Database.Models.Questions.ThemeModel
                {
                    Name = themeName
                },
                null,
                () => new Database.Models.Questions.ThemeModel
                {
                    Name = themeName
                },
                cancellationToken);
        }
        catch (PostgresException exc) when (exc.SqlState == PostgresErrorCodes.ProgramLimitExceeded)
        {
            LogLimit(themeName, exc);
            throw new LimitExceedException(exc);
        }

        return (await connection.Themes.FirstAsync(t => t.Name == themeName, token: cancellationToken)).Id;
    }

    private async Task<List<CollectedAnswer>> GetAppellatedAndRejectedAnswersAsync(
        int themeId,
        int questionId,
        CancellationToken cancellationToken)
    {
        var query =
            from r in connection.Relations
            join e in connection.Entities on r.EntityId equals e.Id
            where r.ThemeId == themeId &&
                  r.QuestionId == questionId &&
                  (r.Type == Database.Models.Questions.RelationType.Apellated ||
                  r.Type == Database.Models.Questions.RelationType.Rejected) &&
                  r.Count >= _options.CollectedAnswersThreshold
            select new CollectedAnswer
            {
                AnswerText = e.Name,
                RelationType = MapType(r.Type),
                Count = r.Count
            };

        return await query.ToListAsync(cancellationToken);
    }

    private void LogLimit(string text, PostgresException exc) =>
        logger.LogInformation(exc, "Limit exceeded. Text: {text}, text length: {textLength}", text, text.Length);
}
