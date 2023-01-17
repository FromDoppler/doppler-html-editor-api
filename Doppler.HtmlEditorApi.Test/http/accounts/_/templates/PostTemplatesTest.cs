using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

public class PostTemplateTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;
    public PostTemplateTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates", HttpStatusCode.Unauthorized)]
    public async Task POST_template_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.PostAsync(url, JsonContent.Create(new { htmlContent = "", meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/templates", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task POST_template_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task POST_template_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData("WEIRD NAME")]
    [InlineData(123)]
    [InlineData("unset")]
    [InlineData("html")]
    public async Task POST_template_should_not_accept_type_different_than_unlayer(object type)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/templates";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new
        {
            meta = new { },
            htmlContent = "HTML CONTENT",
            templateName = "TemplateName",
            type
        });

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync(url, jsonContent);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("One or more validation errors occurred.", responseContentJson.GetProperty("title").GetString());
    }

    [Fact]
    public async Task POST_template_should_save_a_new_template()
    {
        // Arrange
        var accountName = TestUsersData.EMAIL_TEST1;
        var url = $"/accounts/{accountName}/templates";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var meta = new { test = "NEW META" };
        var expectedMeta = """{"test":"NEW META"}""";
        var htmlContent = "NEW HTML CONTENT";
        var templateName = "NEW NAME";
        var previewImage = "NEW PREVIEW IMAGE";
        var jsonContent = JsonContent.Create(new
        {
            meta,
            htmlContent,
            templateName,
            previewImage,
            type = "unlayer"
        });

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new CreatePrivateTemplateDbQuery(
                    accountName, 5, htmlContent, expectedMeta, previewImage, templateName
                    )))
            .ReturnsAsync(new CreatePrivateTemplateDbQuery.Result()
            {
                NewTemplateId = 456
            });

        var client = _factory.CreateSutClient(
            dbContextMock.Object,
            token);

        // Act
        var response = await client.PostAsync(url, jsonContent);
        //var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        dbContextMock.VerifyAll();
    }
}
