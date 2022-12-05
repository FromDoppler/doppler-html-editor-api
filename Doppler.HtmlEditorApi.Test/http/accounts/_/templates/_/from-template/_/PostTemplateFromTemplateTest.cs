using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
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

public class PostTemplateFromTemplateTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public PostTemplateFromTemplateTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates/from-template/456", HttpStatusCode.Unauthorized)]
    public async Task GET_template_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates/from-template/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/templates/from-template/456", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task GET_template_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/456", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task GET_template_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("invalid_token", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("token expired", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/459", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 459)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/459", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 459)]
    [InlineData("/accounts/otro@test.com/templates/from-template/459", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 459)]
    public async Task GET_template_should_accept_right_tokens_and_return_404_when_not_exist(string url, string token, string accountName, int baseTemplateId)
    {
        // Arrange
        TemplateModel templateModel = null;
        var repositoryMock = new Mock<ITemplateRepository>();

        repositoryMock
            .Setup(x => x.GetOwnOrPublicTemplate(accountName, baseTemplateId))
            .ReturnsAsync(templateModel);

        var client = _factory.CreateSutClient(
            serviceToOverride1: repositoryMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/459", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 459)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/459", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 459)]
    [InlineData("/accounts/otro@test.com/templates/from-template/459", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 459)]
    public async Task GET_template_should_accept_right_tokens_and_call_repository_and_return_createdResourceId(string url, string token, string accountName, int baseTemplateId)
    {
        // Arrange
        const int unlayerEditorType = 5;
        var newTemplateId = 8;
        var expectedSchemaVersion = 999;
        var isPublic = true;
        var previewImage = "PreviewImage";
        var name = "Name";
        var htmlComplete = "<html></html>";
        var meta = JsonSerializer.Serialize(new
        {
            schemaVersion = expectedSchemaVersion
        });

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.Setup(x =>
            x.ExecuteAsync(new GetTemplateByIdWithStatusDbQuery(baseTemplateId, accountName)))
            .ReturnsAsync(new GetTemplateByIdWithStatusDbQuery.Result()
            {
                IsPublic = isPublic,
                EditorType = unlayerEditorType,
                HtmlCode = htmlComplete,
                Meta = meta,
                PreviewImage = previewImage,
                Name = name,
            });

        dbContextMock.Setup(x =>
            x.ExecuteAsync(new CreatePrivateTemplateDbQuery(
                accountName,
                unlayerEditorType,
                htmlComplete,
                meta,
                previewImage,
                name)))
            .ReturnsAsync(new CreatePrivateTemplateDbQuery.Result()
            {
                NewTemplateId = newTemplateId
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);
        var headers = response.GetHeadersAsString();
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        dbContextMock.VerifyAll();
        dbContextMock.VerifyNoOtherCalls();
        Assert.Matches($$"""{"createdResourceId":{{newTemplateId}}}""", responseContent);
        Assert.Contains($"Location: http://localhost/accounts/{accountName}/templates/{newTemplateId}", headers);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/from-template/459", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 459)]
    public async Task GET_template_should_error_when_base_template_content_is_mseditor(string url, string token, string accountName, int baseTemplateId)
    {
        // Arrange
        var editorType = 4;
        var isPublic = true;
        var previewImage = "PreviewImage";
        var name = "Name";
        var contentData = new UnknownTemplateContentData(editorType);

        var templateModel = new TemplateModel(
            TemplateId: baseTemplateId,
            IsPublic: isPublic,
            PreviewImage: previewImage,
            Name: name,
            Content: contentData);

        var repositoryMock = new Mock<ITemplateRepository>();

        repositoryMock
            .Setup(x => x.GetOwnOrPublicTemplate(accountName, baseTemplateId))
            .ReturnsAsync(templateModel);

        var client = _factory.CreateSutClient(
            serviceToOverride1: repositoryMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("https://httpstatuses.io/500", responseContentJson.GetProperty("type").GetString());
        Assert.Equal("Internal Server Error", responseContentJson.GetProperty("title").GetString());
        Assert.Equal("Unsupported template content type Doppler.HtmlEditorApi.Domain.UnknownTemplateContentData", responseContentJson.GetProperty("detail").GetString());
        Assert.Equal(500, responseContentJson.GetProperty("status").GetInt32());
    }
}
