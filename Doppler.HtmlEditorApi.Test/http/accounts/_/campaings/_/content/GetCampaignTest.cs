using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi;

public class GetCampaignTest : IClassFixture<WebApplicationFactory<Startup>>
{
    #region Content examples
    private const string META_CONTENT = "{\"body\":{\"rows\":[]},\"example\":true}";
    private const string HEAD_CONTENT = "<title>Hello head!</title>";
    private const string BODY_CONTENT = "<div>Hello body!</div>";
    private const string ORPHAN_DIV_CONTENT = "<div>Hello orphan div!</div>";
    private const string HTML_WITHOUT_HEAD = $@"<!doctype html>
        <html>
        <body>
        {BODY_CONTENT}
        </body>
        </html>";
    private const string HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV = $@"<!doctype html>
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
    [InlineData("/accounts/x@x.com/campaigns/456/content", HttpStatusCode.Unauthorized)]
    public async Task GET_campaign_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/campaigns/456/content", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task GET_campaign_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task GET_campaign_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("invalid_token", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("token expired", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData("/accounts/otro@test.com/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 456)]
    public async Task GET_campaign_should_accept_right_tokens_and_return_404_when_not_exist(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        // Arrange
        BaseHtmlContentData emptyContentModel = null;
        var repositoryMock = new Mock<ICampaignContentRepository>();

        repositoryMock
            .Setup(x => x.GetCampaignModel(expectedAccountName, expectedIdCampaign))
            .ReturnsAsync(emptyContentModel);

        var client = _factory.CreateSutClient(
            serviceToOverride1: repositoryMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData("/accounts/otro@test.com/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 456)]
    public async Task GET_campaign_should_accept_right_tokens_and_return_unlayer_content(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        // Arrange
        var expectedSchemaVersion = 999;
        var contentRow = new UnlayerContentData(
            Meta: JsonSerializer.Serialize(new
            {
                schemaVersion = expectedSchemaVersion
            }),
            HtmlContent: "<html></html>",
            HtmlHead: null,
            PreviewImage: null);

        var repositoryMock = new Mock<ICampaignContentRepository>();

        repositoryMock
            .Setup(x => x.GetCampaignModel(expectedAccountName, expectedIdCampaign))
            .ReturnsAsync(contentRow);

        var client = _factory.CreateSutClient(
            serviceToOverride1: repositoryMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        var contentModelResponse = JsonSerializer.Deserialize<CampaignContent>
            (responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Matches("\"type\":\"unlayer\"", responseContent);
        Assert.NotNull(contentModelResponse.meta);
        Assert.True(contentModelResponse.meta.Value.TryGetProperty("schemaVersion", out var resultSchemaVersionProp), "schemaVersion property is not present");
        Assert.Equal(JsonValueKind.Number, resultSchemaVersionProp.ValueKind);
        Assert.True(resultSchemaVersionProp.TryGetInt32(out var resultSchemaVersion), "schemaVersion is not a valid Int32 value");
        Assert.Equal(expectedSchemaVersion, resultSchemaVersion);

        // TODO: fix it, why does it not work?
        // Assert.Equal("application/json", response.Headers.GetValues("Content-Type").First());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_return_404_error_when_campaign_does_not_exist(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = false,
                CampaignHasContent = false,
                EditorType = null,
                Content = null,
                Meta = null
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_return_404_error_when_user_does_not_exist(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            null);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_error_when_content_is_mseditor(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var editorType = 4;
        var content = "content";
        var meta = (string)null;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                EditorType = editorType,
                Content = content,
                Meta = meta
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_return_unlayer_content(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var meta = "{\"demo\":\"unlayer\"}";
        var html = "<html></html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                EditorType = 5,
                Content = html,
                Meta = meta
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("unlayer", responseContentJson.GetProperty("type").GetString());
        Assert.Equal(html, responseContentJson.GetProperty("htmlContent").GetString());
        Assert.Equal(JsonValueKind.Object, responseContentJson.GetProperty("meta").ValueKind);
        Assert.Equal(meta, responseContentJson.GetProperty("meta").ToString());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_return_html_content(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var html = "<html></html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                EditorType = (int?)null,
                Content = html
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(responseContentJson.TryGetProperty("meta", out _));
        Assert.Equal("html", responseContentJson.GetProperty("type").GetString());
        Assert.Equal(html, responseContentJson.GetProperty("htmlContent").GetString());
    }


    [Theory]
    // Unlayer Content
    [InlineData(
        "https://1.fromdoppler.net/image1.png",
        5,
        "{\"demo\":\"unlayer\"}"
    )]
    // HTML Content
    [InlineData(
        "https://2.fromdoppler.net/image2.png",
        null,
        null
    )]
    public async Task GET_campaign_should_include_previewImage_in_response(string previewImage, int? editorType, string meta)
    {
        // Arrange
        var idCampaign = 456;
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/{idCampaign}/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;

        var html = "<html></html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            TestUsersData.EMAIL_TEST1,
            idCampaign,
            new()
            {
                IdCampaign = idCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                EditorType = editorType,
                Content = html,
                Meta = meta,
                PreviewImage = previewImage
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(responseContentJson.TryGetProperty("previewImage", out var previewImageProperty));
        Assert.Equal(previewImage, previewImageProperty.GetString());
        dbContextMock.Verify(
            x => x.ExecuteAsync(It.Is<FirstOrDefaultContentWithCampaignStatusDbQuery>(q => q.SqlQueryContains("ca.PreviewImage"))),
            Times.Once());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_error_when_content_is_unknown(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        var editorType = 8; // Unknown
        var content = "content";
        var meta = "meta";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                EditorType = editorType,
                Content = content,
                Meta = meta
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_campaign_should_return_empty_content_as_unlayer_content(string url, string token, string expectedAccountName, int expectedIdCampaign)
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupContentWithCampaignStatus(
            expectedAccountName,
            expectedIdCampaign,
            new()
            {
                IdCampaign = expectedIdCampaign,
                CampaignExists = true,
                CampaignHasContent = false,
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("unlayer", responseContentJson.GetProperty("type").GetString());
        Assert.NotEmpty(responseContentJson.GetProperty("htmlContent").GetString());
        Assert.Equal(JsonValueKind.Object, responseContentJson.GetProperty("meta").ValueKind);
        Assert.NotEmpty(responseContentJson.GetProperty("meta").ToString());
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
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TestUsersData.EMAIL_TEST1;
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
