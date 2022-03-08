using System.Net.Http;
using System.Net.Http.Headers;
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
        var setup = dbContextMock.Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultContentWithCampaignStatusDbQuery.Result>(
            It.IsAny<string>(),
            It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                x.AccountName == expectedAccountName
                && x.IdCampaign == expectedIdCampaign)));

        setup.ReturnsAsync(result);
    }
}
