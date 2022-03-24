using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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
            It.Is<FirstOrDefaultContentWithCampaignStatusDbQuery>(q =>
                q.AccountName == expectedAccountName
                && q.IdCampaign == expectedIdCampaign
            )));

        setup.ReturnsAsync(result);
    }

    public static void SetupBasicFields(
        this Mock<IDbContext> dbContextMock)
    {
        dbContextMock.Setup(x => x.ExecuteAsync(
            It.IsAny<QueryActiveBasicFieldsDbQuery>()
        ))
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
            It.Is<QueryCustomFieldsDbQueryByAccountNameDbQuery>(q =>
                q.GenerateSqlQuery().Contains("IsBasicField = 0")
                && q.GenerateSqlQuery().Contains("Email = @accountName")
                && q.AccountName == expectedAccountName
        )))
        .ReturnsAsync(result);
    }

    // TODO: should I use it?
    public static Moq.Language.Flow.ISetup<IDbContext, Task<IEnumerable<TResult>>> SetupExecuteAsync<TdbQuery, TResult>(
        this Mock<IDbContext> dbContextMock,
        Expression<Func<TdbQuery, bool>> match)
        where TdbQuery : ICollectionDbQuery<TResult>
        => dbContextMock.Setup(x => x.ExecuteAsync(It.Is<TdbQuery>(match)));

    // ISingleItemDbQuery
    // IListDbQuery
    // IExecutableDbQuery

}
