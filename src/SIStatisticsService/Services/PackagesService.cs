using LinqToDB;
using SIPackages;
using SIPackages.Core;
using SIStatisticsService.Contract.Models;
using SIStatisticsService.Contracts;
using SIStatisticsService.Database;
using System.Data.Common;
using System.Text;

namespace SIStatisticsService.Services;

/// <inheritdoc cref="IPackagesService" />
public sealed class PackagesService : IPackagesService
{
    private readonly SIStatisticsDbConnection _connection;

    public PackagesService(SIStatisticsDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<QuestionInfoResponse> GetQuestionInfoAsync(string themeName, string questionText, CancellationToken cancellationToken)
    {
        var query = from t in _connection.Themes.Where(theme => theme.Name == themeName)
                    from q in _connection.Questions.Where(question => question.Text == questionText)
                    from e in _connection.Entities
                    from r in _connection.Relations.Where(rel => rel.ThemeId == t.Id && rel.QuestionId == q.Id && rel.EntityId == e.Id)
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

    public async Task ImportPackageAsync(Package package, CancellationToken cancellationToken)
    {
        using var tx = await _connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

        foreach (var round in package.Rounds)
        {
            foreach (var theme in round.Themes)
            {
                var themeId = await InsertThemeNameAsync(theme.Name, cancellationToken);

                foreach (var question in theme.Questions)
                {
                    var questionText = new StringBuilder();

                    foreach (var atom in question.Scenario)
                    {
                        if (atom.Type == AtomTypes.Text || atom.Type == AtomTypes.Oral)
                        {
                            if (questionText.Length > 0)
                            {
                                questionText.AppendLine();
                            }

                            questionText.Append(atom.Text);
                        }
                    }

                    var questionId = await InsertQuestionTextAsync(questionText.ToString(), cancellationToken);

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

        await tx.CommitAsync(cancellationToken);
    }

    private async Task InsertOrUpdateAnswer(
        int themeId,
        int questionId,
        string answer,
        Database.Models.Questions.RelationType relationType,
        CancellationToken cancellationToken)
    {
        var entityId = await InsertEntityAsync(answer, cancellationToken);

        var updatedRowCount = await _connection.Relations
            .Where(
                rm => rm.ThemeId == themeId && rm.QuestionId == questionId && rm.EntityId == entityId && rm.Type == relationType)
            .Set(rm => rm.Count, rm => rm.Count + 1)
            .UpdateAsync(cancellationToken);

        if (updatedRowCount > 0)
        {
            return;
        }

        await _connection.Relations.InsertWithInt32IdentityAsync(
            () => new Database.Models.Questions.RelationModel
            {
                ThemeId = themeId,
                QuestionId = questionId,
                EntityId = entityId,
                Count = 1,
                Type = relationType
            },
        cancellationToken);
    }

    private async Task<int> InsertEntityAsync(string entityName, CancellationToken cancellationToken)
    {
        var existingEntityId = (await _connection.Entities.FirstOrDefaultAsync(e => e.Name == entityName, token: cancellationToken))?.Id;

        var entityId = existingEntityId ?? await _connection.Entities.InsertWithInt32IdentityAsync(
            () => new Database.Models.Questions.EntityModel
            {
                Name = entityName
            },
            cancellationToken);

        return entityId;
    }

    private async Task<int> InsertQuestionTextAsync(string questionText, CancellationToken cancellationToken)
    {
        var existingQuestionId = (await _connection.Questions.FirstOrDefaultAsync(q => q.Text == questionText, token: cancellationToken))?.Id;

        var questionId = existingQuestionId ?? await _connection.Questions.InsertWithInt32IdentityAsync(
            () => new Database.Models.Questions.QuestionModel
            {
                Text = questionText
            },
            cancellationToken);

        return questionId;
    }

    private async Task<int> InsertThemeNameAsync(string themeName, CancellationToken cancellationToken)
    {
        var existingThemeId = (await _connection.Themes.FirstOrDefaultAsync(t => t.Name == themeName, token: cancellationToken))?.Id;

        var themeId = existingThemeId ?? await _connection.Themes.InsertWithInt32IdentityAsync(
            () => new Database.Models.Questions.ThemeModel
            {
                Name = themeName
            },
            cancellationToken);

        return themeId;
    }
}
