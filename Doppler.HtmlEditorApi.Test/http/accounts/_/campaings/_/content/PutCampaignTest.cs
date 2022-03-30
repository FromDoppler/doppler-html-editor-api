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
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using System.Net.Http.Json;
using System;

namespace Doppler.HtmlEditorApi;
public class PutCampaignTest : IClassFixture<WebApplicationFactory<Startup>>
{
    #region Content examples
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
    #endregion Content examples
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;
    public PutCampaignTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", true, typeof(UpdateCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", false, typeof(InsertCampaignContentDbQuery))]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    public async Task PUT_campaign_should_split_html_in_head_and_content(string htmlInput, string expectedHead, string expectedContent, string type, bool existingContent, Type queryType)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var idCampaign = 456;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = existingContent,
                EditorType = null,
            });

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

        dynamic dbQuery = null;

        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<IExecutableDbQuery>(q =>
            q.GetType() == queryType
            && AssertHelper.GetDynamicValueAndContinue(q, out dbQuery))));

        Assert.Equal(idCampaign, dbQuery.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, dbQuery.Content);
        AssertHelper.EqualIgnoringSpaces(expectedHead, dbQuery.Head);
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
    [InlineData("html", "[[[custom1]]] [[[Custom2]]] [[[UNKNOWN_FIELD]]]", "|*|12345*|* |*|456789*|* [[[UNKNOWN_FIELD]]]")]
    public async Task PUT_campaign_should_replace_fields_and_remove_unknown_fieldIds(string type, string htmlInput, string expectedContent)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var idCampaign = 456;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = null,
            });

        dbContextMock.SetupBasicFields();
        dbContextMock.SetupCustomFields(
            expectedAccountName: accountName,
            result: new DbField[] {
                new()
                {
                    IdField = 12345,
                    IsBasicField = false,
                    Name = "CUSTOM1"
                },
                new()
                {
                    IdField = 456789,
                    IsBasicField = false,
                    Name = "custom2"
                }
            });

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

        UpdateCampaignContentDbQuery dbQuery = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<UpdateCampaignContentDbQuery>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));

        Assert.Equal(idCampaign, dbQuery.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, dbQuery.Content);
    }

    [Theory]
    [InlineData(
        1,
        "html",
        // Sanitization not required
        "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first name]]]|*|12345678*|*\">[[[first name]]]</a> [[[cumpleaños]]]</b></p>",
        "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    [InlineData(
        2,
        "unlayer",
        "<a href=\"https://\tgoo gle1\n.com    \r\n  \">Link</a>",
        "<a href=\"https://google1.com\">Link</a>")]
    public async Task PUT_campaign_should_sanitize_links(int idCampaign, string type, string htmlInput, string expectedContent)
    {
        // Arrange
        var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TUD.EMAIL_TEST1;
        var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = null,
            });

        dbContextMock.SetupBasicFields();

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

        UpdateCampaignContentDbQuery dbQuery = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<UpdateCampaignContentDbQuery>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));

        Assert.Equal(idCampaign, dbQuery.IdCampaign);
        Assert.Equal(expectedContent, dbQuery.Content);
    }
}
