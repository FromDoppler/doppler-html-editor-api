using Doppler.HtmlEditorApi;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Text.Json;
using System.Net;
using TUD = Doppler.HtmlEditorApi.Test.Utils.TestUsersData;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi;
public class GetCampaignTest : IClassFixture<WebApplicationFactory<Startup>>
{
    #region Content examples
    const string META_CONTENT = "{\"body\":{\"rows\":[]},\"example\":true}";
    const string HEAD_CONTENT = "<title>Hello head!</title>";
    const string BODY_CONTENT = "<div>Hello body!</div>";
    const string ORPHAN_DIV_CONTENT = "<div>Hello orphan div!</div>";
    const string HTML_WITHOUT_HEAD = $@"<!doctype html>
        <html>
        <body>
        {BODY_CONTENT}
        </body>
        </html>";
    const string HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV = $@"<!doctype html>
        <html>
        {ORPHAN_DIV_CONTENT}
        </html>";
    #endregion Content examples
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public GetCampaignTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData(BODY_CONTENT, HEAD_CONTENT, BODY_CONTENT, null, null)]
    [InlineData(BODY_CONTENT, HEAD_CONTENT, BODY_CONTENT, META_CONTENT, 5)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, null, null)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, META_CONTENT, 5)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, null, null)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, META_CONTENT, 5)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, null)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, META_CONTENT, 5)]
    [InlineData(ORPHAN_DIV_CONTENT, HEAD_CONTENT, ORPHAN_DIV_CONTENT, null, null)]
    [InlineData(ORPHAN_DIV_CONTENT, HEAD_CONTENT, ORPHAN_DIV_CONTENT, META_CONTENT, 5)]
    public async Task GET_campaign_should_return_not_include_head_in_the_content(string expectedHtml, string storedHead, string storedContent, string storedMeta, int? editorType)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var idCampaign = 456;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            accountName,
            idCampaign,
            new()
            {
                IdCampaign = idCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                Content = storedContent,
                Head = storedHead,
                Meta = storedMeta,
                EditorType = editorType
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
        Assert.Equal(expectedHtml, responseContentJson.GetProperty("htmlContent").GetString());
    }

}
