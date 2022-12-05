using System;
using System.Collections.Generic;
using System.Linq;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Moq;
using Xunit;

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

    public static void SetupCampaignStatus(
        this Mock<IDbContext> dbContextMock,
        string expectedAccountName,
        int expectedIdCampaign,
        FirstOrDefaultCampaignStatusDbQuery.Result result)
    {
        var setup = dbContextMock.Setup(x => x.ExecuteAsync(
            new FirstOrDefaultCampaignStatusDbQuery(
                expectedIdCampaign,
                expectedAccountName
            )));

        setup.ReturnsAsync(result);
    }

    public static void SetupTemplateWithStatus(
        this Mock<IDbContext> dbContextMock,
        string accountName,
        int idTemplate,
        GetTemplateByIdWithStatusDbQuery.Result result)
    {
        var setup = dbContextMock.Setup(x => x.ExecuteAsync(
            new GetTemplateByIdWithStatusDbQuery(
                idTemplate,
                accountName
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
        int? idTemplate = null,
        int result = 1)
    {
        dbContextMock.Setup(x => x.ExecuteAsync(
            It.Is<IExecutableDbQuery>(q =>
                q.SqlQueryStartsWith(sqlQueryStartsWith)
                && (
                    q.Is<InsertCampaignContentDbQuery>(x =>
                    x.IdCampaign == idCampaign
                    && x.Content == htmlContent
                    && x.Meta == meta
                    && x.IdTemplate == idTemplate)
                    ||
                    q.Is<UpdateCampaignContentDbQuery>(x =>
                    x.IdCampaign == idCampaign
                    && x.Content == htmlContent
                    && x.Meta == meta
                    && x.IdTemplate == idTemplate)
                ))))
        .ReturnsAsync(result);
    }

    public static bool SqlQueryStartsWith(this IDbQuery q, string sqlQueryStartsWith)
        => q.GenerateSqlQuery().Trim().StartsWith(sqlQueryStartsWith, StringComparison.OrdinalIgnoreCase);

    public static bool SqlQueryContains(this IDbQuery q, string sqlQueryContains)
        => q.GenerateSqlQuery().Contains(sqlQueryContains);

    public static void VerifySqlQueryContains(this IDbQuery q, string sqlQueryContains)
        => Assert.Contains(sqlQueryContains, q.GenerateSqlQuery());

    public static bool Is<T>(this IDbQuery q)
        => q is T;

    public static bool Is<T>(this IDbQuery q, Func<T, bool> match)
        => q is T casted && match(casted);

    public static bool SqlParametersContain(this IDbQuery q, string name, object value)
    {
        var parameters = q.GenerateSqlParameters();
        var propInfo = q.GetType().GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (propInfo == null)
        {
            return false;
        }
        var propValue = propInfo.GetValue(parameters);
        return (value == null && propValue == null) || (value != null && value.Equals(propValue));
    }

    public static void VerifySqlParametersContain(this IDbQuery q, string name, object value)
        => Assert.True(q.SqlParametersContain(name, value), $"Query does not contain parameter {name} = {value}");

    public static void VerifyLinksSendToSaveNewCampaignLinks(
        this Mock<IDbContext> dbContextMock,
        int idCampaign,
        string[] expectedLinks)
    {
        string[] linksSendToSaveNewCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<SaveNewCampaignLinks>(q =>
                q.IdContent == idCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToSaveNewCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToSaveNewCampaignLinks);
    }

    public static void VerifyLinksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks(
        this Mock<IDbContext> dbContextMock,
        int idCampaign,
        string[] expectedLinks)
    {
        string[] linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
                q.IdContent == idCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks);
    }

    public static void VerifyLinksSendToDeleteRemovedCampaignLinks(
        this Mock<IDbContext> dbContextMock,
        int idCampaign,
        string[] expectedLinks
    )
    {
        string[] linksSendToDeleteRemovedCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteRemovedCampaignLinks>(q =>
                q.IdContent == idCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToDeleteRemovedCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToDeleteRemovedCampaignLinks);
    }

    public static T VerifyAndGetExecutableDbQuery<T>(this Mock<IDbContext> dbContextMock)
        where T : IExecutableDbQuery
    {
        T dbQuery = default;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<T>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));
        return dbQuery;
    }

    public static IExecutableDbQuery VerifyAndGetExecutableDbQuery(this Mock<IDbContext> dbContextMock)
        => VerifyAndGetExecutableDbQuery<IExecutableDbQuery>(dbContextMock);

    public static T VerifyAndGetSingleItemDbQuery<T, T2>(this Mock<IDbContext> dbContextMock)
        where T : ISingleItemDbQuery<T2>
    {
        T dbQuery = default;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<T>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));
        return dbQuery;
    }

    public static ISingleItemDbQuery<T> VerifyAndGetSingleItemDbQuery<T>(this Mock<IDbContext> dbContextMock)
        => VerifyAndGetSingleItemDbQuery<ISingleItemDbQuery<T>, T>(dbContextMock);

    public static T VerifyAndGetCollectionDbQuery<T, T2>(this Mock<IDbContext> dbContextMock)
        where T : ICollectionDbQuery<T2>
    {
        T dbQuery = default;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<T>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));
        return dbQuery;
    }

    public static ICollectionDbQuery<T> VerifyAndGetCollectionDbQuery<T>(this Mock<IDbContext> dbContextMock)
        => VerifyAndGetCollectionDbQuery<ICollectionDbQuery<T>, T>(dbContextMock);
}
