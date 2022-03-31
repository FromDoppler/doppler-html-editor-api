using Doppler.HtmlEditorApi;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Repositories.DopplerDb;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class DapperCampaignContentRepositoryTest : IClassFixture<WebApplicationFactory<Startup>>
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(1, true)] //DRAFT
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(8, false)]
    [InlineData(9, false)]
    [InlineData(10, false)]
    [InlineData(11, true)]//DRAFT
    [InlineData(12, false)]
    [InlineData(13, false)]
    [InlineData(14, false)]
    [InlineData(15, false)]
    [InlineData(16, false)]
    [InlineData(17, false)]
    [InlineData(18, true)] //IN_WINNER_SELECTION_PROCESS
    [InlineData(19, false)]
    [InlineData(20, false)]
    [InlineData(21, false)]
    [InlineData(22, false)]
    public async void GetCampaignState_with_content_writable_when_valid_doppler_status_code(int? dopplerCampaignStatus, bool expectedIsWritable)
    {
        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(It.IsAny<int>(), It.IsAny<string>()))
                )
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = dopplerCampaignStatus
            });
        var repository = new DapperCampaignContentRepository(dbContextMock.Object);

        var campaignState = await repository.GetCampaignState(It.IsAny<string>(), It.IsAny<int>());
        Assert.Equal(expectedIsWritable, campaignState.IsWritable);
        dbContextMock.VerifyAll();
    }
}
