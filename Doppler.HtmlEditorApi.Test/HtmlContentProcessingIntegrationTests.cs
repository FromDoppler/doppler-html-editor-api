using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Test.Utils;
using TUD = Doppler.HtmlEditorApi.Test.Utils.TestUsersData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;
using Xunit.Abstractions;
using Xunit;

namespace Doppler.HtmlEditorApi;

public class HtmlContentProcessingIntegrationTests
    : IClassFixture<WebApplicationFactory<Startup>>
{
    #region Content examples
    const string META_CONTENT = "{\"body\":{\"rows\":[]},\"example\":true}";
    const string HEAD_CONTENT = "<title>Hello head!</title>";
    const string BODY_CONTENT = "<div>Hello body!</div>";
    const string ORPHAN_DIV_CONTENT = "<div>Hello orphan div!</div>";
    const string ONLY_HEAD = $"<head>{HEAD_CONTENT}</head>";
    const string HTML_WITH_HEAD_AND_BODY = $@"<!doctype html>
        <html>
        <head>
            {HEAD_CONTENT}
        </head>
        <body>
            {BODY_CONTENT}
        </body>
        </html>";
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
    const string HTML_WITHOUT_BODY_WITH_ORPHAN_DIV = $@"<!doctype html>
        <html>
            <head>
                {HEAD_CONTENT}
            </head>
            <div>
                {ORPHAN_DIV_CONTENT}
            </div>
        </html>";
    const string HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD = $@"<!doctype html>
        <html>
            <div>
                {ORPHAN_DIV_CONTENT}
            </div>
        </html>";
    const string HTML_WITHOUT_BODY = $@"<!doctype html>
        <html>
            <head>
                {HEAD_CONTENT}
            </head>
        </html>";
    #endregion Content examples

    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public HtmlContentProcessingIntegrationTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", true)]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", false)]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "unlayer", true)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", true)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", false)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "unlayer", true)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", true)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", false)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "unlayer", true)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", true)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", false)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "unlayer", true)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", true)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", false)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "unlayer", true)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", true)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", false)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "unlayer", true)]
    public async Task PUT_campaign_should_split_html_in_head_and_content(string htmlInput, string expectedHead, string expectedContent, string type, bool existingContent)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var idCampaign = 456;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                    x.AccountName == accountName
                    && x.IdCampaign == idCampaign)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = existingContent,
                EditorType = null,
            });

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<ContentRow>()))
            .ReturnsAsync(1);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = type,
            htmlContent = htmlInput,
            meta = "true" // it does not care
        }));

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();

        ContentRow contentRow = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<ContentRow>(x => AssertHelper.GetValueAndContinue(x, out contentRow))));

        Assert.Equal(idCampaign, contentRow.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, contentRow.Content);
        AssertHelper.EqualIgnoringSpaces(expectedHead, contentRow.Head);
    }

    [Theory]
    [InlineData("unlayer", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* , tenemos una oferta para vos</div>")]
    [InlineData("html", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* , tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[FIRST_NAME]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("html", "<div>Hola [[[first_name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[nombre]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[first name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("html", "Hoy ([[[cumpleaños]]]) es tu cumpleaños", "Hoy (|*|323*|*) es tu cumpleaños")]
    [InlineData(
        "unlayer",
        "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first name]]]|*|12345678*|*\">[[[first name]]]</a> [[[cumpleaños]]]</b></p>",
        "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    [InlineData(
        "unlayer",
        "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first%20name]]]%20[[[cumplea&#241;os]]]\">[[[first%20name]]]</a> [[[cumplea&ntilde;os]]]</b></p>",
        "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*%20|*|323*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    public async Task PUT_campaign_should_remove_unknown_fieldIds(string type, string htmlInput, string expectedContent)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var idCampaign = 456;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.IsAny<ByCampaignIdAndAccountNameParameters>()))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = null,
            });

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<ContentRow>()))
            .ReturnsAsync(1);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = type,
            htmlContent = htmlInput,
            meta = "true" // it does not care
        }));

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ContentRow contentRow = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<ContentRow>(x => AssertHelper.GetValueAndContinue(x, out contentRow))));

        Assert.Equal(idCampaign, contentRow.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, contentRow.Content);
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
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultContentWithCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                    x.AccountName == accountName
                    && x.IdCampaign == idCampaign)))
            .ReturnsAsync(new FirstOrDefaultContentWithCampaignStatusDbQuery.Result()
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
