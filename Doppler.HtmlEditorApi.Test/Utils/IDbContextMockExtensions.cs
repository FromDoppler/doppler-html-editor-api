using System;
using System.Collections.Generic;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using Moq;

namespace Doppler.HtmlEditorApi.Test.Utils;

public static class IDbContextMockExtensions
{
    public static void SetupContentWithCampaignStatus(
        this Mock<IDbContext> dbContextMock,
        string expectedAccountName,
        int expectedIdCampaign,
        FirstOrDefaultContentWithCampaignStatusDbQuery.Result result)
    {
        var setup = dbContextMock.Setup(x => x.ExecuteAsync(
            new FirstOrDefaultContentWithCampaignStatusDbQuery(
                expectedIdCampaign,
                expectedAccountName
            )));

        setup.ReturnsAsync(result);
    }

    public static void SetupBasicFields(
        this Mock<IDbContext> dbContextMock)
    {
        dbContextMock.Setup(x => x.ExecuteAsync(
            new QueryActiveBasicFieldsDbQuery()))
        .ReturnsAsync(new DbField[]
        {
            new() { IdField = 319, Name = "FIRST_NAME" },
            new() { IdField = 320, Name = "LAST_NAME" },
            new() { IdField = 321, Name = "EMAIL" },
            new() { IdField = 322, Name = "GENDER" },
            new() { IdField = 323, Name = "BIRTHDAY" },
            new() { IdField = 324, Name = "COUNTRY" },
            new() { IdField = 325, Name = "CONSENT" },
            new() { IdField = 326, Name = "ORIGIN" },
            new() { IdField = 327, Name = "SCORE" },
            new() { IdField = 106667, Name = "GDPR" }
        });
    }

    public static void SetupCustomFields(
        this Mock<IDbContext> dbContextMock,
        string expectedAccountName,
        IEnumerable<DbField> result)
    {
        dbContextMock.Setup(x => x.ExecuteAsync(
            new QueryCustomFieldsDbQueryByAccountNameDbQuery(expectedAccountName)
        ))
        .ReturnsAsync(result);
    }

    public static void SetupInsertOrUpdateContentRow(
        this Mock<IDbContext> dbContextMock,
        string sqlQueryStartsWith,
        int idCampaign,
        string htmlContent,
        string meta,
        int result)
    {
        dbContextMock.Setup(x => x.ExecuteAsync(
            It.Is<IExecutableDbQuery>(q =>
                q.SqlQueryStartsWith(sqlQueryStartsWith)
                && (
                    q.Is<InsertCampaignContentDbQuery>(x =>
                    x.IdCampaign == idCampaign
                    && x.Content == htmlContent
                    && x.Meta == meta)
                    ||
                    q.Is<UpdateCampaignContentDbQuery>(x =>
                    x.IdCampaign == idCampaign
                    && x.Content == htmlContent
                    && x.Meta == meta)
                ))))
        .ReturnsAsync(result);
    }

    public static bool SqlQueryStartsWith(this IDbQuery q, string sqlQueryStartsWith)
        => q.GenerateSqlQuery().Trim().StartsWith(sqlQueryStartsWith);

    public static bool SqlQueryContains(this IDbQuery q, string sqlQueryContains)
        => q.GenerateSqlQuery().Contains(sqlQueryContains);

    public static bool Is<T>(this IDbQuery q, Func<T, bool> match)
        => q is T casted && match(casted);
}
