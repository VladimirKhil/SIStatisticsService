﻿using LinqToDB;
using Npgsql;
using SIPackages;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using SIStatisticsService.Exceptions;
using SIStatisticsService.Helpers;
using SIStatisticsService.Metrics;

namespace SIStatisticsService.Services;

/// <inheritdoc cref="IPackagesService" />
public sealed class PackagesService : IPackagesService
{
    private readonly SIStatisticsDbConnection _connection;
    private readonly OtelMetrics _metrics;
    private readonly ILogger<PackagesService> _logger;

    public PackagesService(SIStatisticsDbConnection connection, OtelMetrics metrics, ILogger<PackagesService> logger)
    {
        _connection = connection;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<QuestionInfoResponse> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken)
    {
        var query =
            from r in _connection.Relations
            join t in _connection.Themes on r.ThemeId equals t.Id
            join q in _connection.Questions on r.QuestionId equals q.Id
            join e in _connection.Entities on r.EntityId equals e.Id
            where t.Name == themeName && q.Text == questionText
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

    public async Task ImportPackageAsync(Package package, CancellationToken cancellationToken)
    {
        foreach (var round in package.Rounds)
        {
            foreach (var theme in round.Themes)
            {
                int themeId;

                try
                {
                    themeId = await InsertThemeNameAsync(theme.Name, cancellationToken);
                }
                catch (LimitExceedException)
                {
                    _metrics.AddLimitExceed();
                    continue;
                }

                foreach (var question in theme.Questions)
                {
                    var questionText = question.GetText();
                    int questionId;

                    try
                    {
                        questionId = await InsertQuestionTextAsync(questionText, cancellationToken);
                    }
                    catch (LimitExceedException)
                    {
                        _metrics.AddLimitExceed();
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
                }
            }
        }

        _metrics.AddPackage();
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
            _metrics.AddLimitExceed();
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
            _metrics.AddLimitExceed();
            return;
        }

        await _connection.Relations.InsertOrUpdateAsync(
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

        _metrics.AddQuestions();
    }

    private async Task<int> InsertEntityAsync(string entityName, CancellationToken cancellationToken)
    {
        entityName = ValueNormalizer.Normalize(entityName);

        try
        {
            await _connection.Entities.InsertOrUpdateAsync(
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

        return (await _connection.Entities.FirstAsync(e => e.Name == entityName, token: cancellationToken)).Id;
    }

    private async Task<int> InsertQuestionTextAsync(string questionText, CancellationToken cancellationToken)
    {
        questionText = ValueNormalizer.Normalize(questionText);

        try
        {
            await _connection.Questions.InsertOrUpdateAsync(
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

        return (await _connection.Questions.FirstAsync(q => q.Text == questionText, token: cancellationToken)).Id;
    }

    private async Task<int> InsertThemeNameAsync(string themeName, CancellationToken cancellationToken)
    {
        themeName = ValueNormalizer.Normalize(themeName);

        try
        {
            await _connection.Themes.InsertOrUpdateAsync(
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

        return (await _connection.Themes.FirstAsync(t => t.Name == themeName, token: cancellationToken)).Id;
    }

    private void LogLimit(string text, PostgresException exc) =>
        _logger.LogInformation(exc, "Limit exceeded. Text: {text}, text length: {textLength}", text, text.Length);
}
