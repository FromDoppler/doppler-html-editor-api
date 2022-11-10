using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi;

public class GetTemplateTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public GetTemplateTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates/456", HttpStatusCode.Unauthorized)]
    public async Task GET_template_should_require_token(string url, HttpStatusCode expectedStatusCode)
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
    [InlineData("/accounts/x@x.com/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/templates/456", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task GET_template_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task GET_template_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData("/accounts/otro@test.com/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 456)]
    public async Task GET_template_should_accept_right_tokens_and_return_404_when_not_exist(string url, string token, string accountName, int idTemplate)
    {
        // Arrange
        TemplateModel templateModel = null;
        var repositoryMock = new Mock<ITemplateRepository>();

        repositoryMock
            .Setup(x => x.GetTemplate(accountName, idTemplate))
            .ReturnsAsync(templateModel);

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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    [InlineData("/accounts/otro@test.com/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 456)]
    public async Task GET_template_should_accept_right_tokens_and_return_unlayer_template(string url, string token, string accountName, int idTemplate)
    {
        // Arrange
        var expectedSchemaVersion = 999;
        var isPublic = true;
        var previewImage = "PreviewImage";
        var name = "Name";
        var contentData = new UnlayerTemplateContentData(
            HtmlComplete: "<html></html>",
            Meta: JsonSerializer.Serialize(new
            {
                schemaVersion = expectedSchemaVersion
            }));

        var templateModel = new TemplateModel(
            TemplateId: idTemplate,
            IsPublic: isPublic,
            PreviewImage: previewImage,
            Name: name,
            Content: contentData);

        var repositoryMock = new Mock<ITemplateRepository>();

        repositoryMock
            .Setup(x => x.GetTemplate(accountName, idTemplate))
            .ReturnsAsync(templateModel);

        var client = _factory.CreateSutClient(
            serviceToOverride1: repositoryMock.Object,
            token: token);

        // Act
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        var contentModelResponse = JsonSerializer.Deserialize<Template>
            (responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Matches("\"type\":\"unlayer\"", responseContent);
        Assert.NotNull(contentModelResponse.meta);
        Assert.True(contentModelResponse.meta.Value.TryGetProperty("schemaVersion", out var resultSchemaVersionProp), "schemaVersion property is not present");
        Assert.Equal(JsonValueKind.Number, resultSchemaVersionProp.ValueKind);
        Assert.True(resultSchemaVersionProp.TryGetInt32(out var resultSchemaVersion), "schemaVersion is not a valid Int32 value");
        Assert.Equal(expectedSchemaVersion, resultSchemaVersion);
        Assert.Equal(name, contentModelResponse.templateName);
        Assert.Equal(previewImage, contentModelResponse.previewImage);
        Assert.Equal(isPublic, contentModelResponse.isPublic);
        Assert.Equal(contentData.HtmlComplete, contentModelResponse.htmlContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_templte_should_error_when_template_content_is_mseditor(string url, string token, string accountName, int idTemplate)
    {
        var editorType = 4;
        var content = "content";
        var meta = (string)null;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupTemplateWithStatus(
            accountName,
            idTemplate,
            new()
            {
                IsPublic = true,
                EditorType = editorType,
                HtmlCode = content,
                Meta = meta,
                PreviewImage = "",
                Name = "Name"
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
        Assert.Equal("https://httpstatuses.io/500", responseContentJson.GetProperty("type").GetString());
        Assert.Equal("Internal Server Error", responseContentJson.GetProperty("title").GetString());
        Assert.Equal("Unsupported template content type Doppler.HtmlEditorApi.Domain.UnknownTemplateContentData", responseContentJson.GetProperty("detail").GetString());
        Assert.Equal(500, responseContentJson.GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456)]
    public async Task GET_template_should_return_unlayer_template(string url, string token, string accountName, int idTemplate)
    {
        var meta = "{\"demo\":\"unlayer\"}";
        var html = "<html></html>";
        var name = "Name";
        var previewImage = "Preview";
        var isPublic = true;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.SetupTemplateWithStatus(
            accountName,
            idTemplate,
            new()
            {
                EditorType = 5,
                HtmlCode = html,
                IsPublic = isPublic,
                Meta = meta,
                Name = name,
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
        Assert.Equal("unlayer", responseContentJson.GetProperty("type").GetString());
        Assert.Equal(html, responseContentJson.GetProperty("htmlContent").GetString());
        Assert.Equal(JsonValueKind.Object, responseContentJson.GetProperty("meta").ValueKind);
        Assert.Equal(meta, responseContentJson.GetProperty("meta").ToString());
        Assert.Equal(isPublic, responseContentJson.GetProperty("isPublic").GetBoolean());
        Assert.Equal(name, responseContentJson.GetProperty("templateName").GetString());
        Assert.Equal(previewImage, responseContentJson.GetProperty("previewImage").GetString());
    }
}
