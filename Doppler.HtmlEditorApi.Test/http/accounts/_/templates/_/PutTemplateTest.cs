using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

public class PutTemplateTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public PutTemplateTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates/456", HttpStatusCode.Unauthorized)]
    public async Task PUT_template_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.PutAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/templates/456", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task PUT_template_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task PUT_template_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("invalid_token", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("token expired", response.Headers.WwwAuthenticate.ToString());
    }

    [Fact]
    public async Task PUT_template_should_require_fields()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new { });

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("One or more validation errors occurred.", responseContentJson.GetProperty("title").GetString());
        Assert.Equal("The meta field is required.", responseContentJson.GetProperty("errors").GetProperty("meta")[0].GetString());
        Assert.Equal("The htmlContent field is required.", responseContentJson.GetProperty("errors").GetProperty("htmlContent")[0].GetString());
        Assert.Equal("The templateName field is required.", responseContentJson.GetProperty("errors").GetProperty("templateName")[0].GetString());
    }

    [Fact]
    public async Task PUT_template_should_require_type_field()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new
        {
            meta = new { },
            htmlContent = "HTML CONTENT",
            templateName = "TemplateName"
        });

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("One or more validation errors occurred.", responseContentJson.GetProperty("title").GetString());
        Assert.Equal("The type field is required.", responseContentJson.GetProperty("errors").GetProperty("type")[0].GetString());
    }


    [Theory]
    [InlineData("WEIRD NAME")]
    [InlineData(123)]
    [InlineData("unset")]
    [InlineData("html")]
    public async Task PUT_template_should_not_accept_type_different_than_unlayer(object type)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456";
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
        var response = await client.PutAsync(url, jsonContent);
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
    public async Task PUT_template_should_not_accept_empty_meta()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/templates/456";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new
        {
            meta = "",
            htmlContent = "HTML CONTENT",
            templateName = "TemplateName",
            type = "unlayer"
        });

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("The meta field is required.", responseContentJson.GetProperty("errors").GetProperty("meta")[0].GetString());
    }

    [Fact]
    public async Task PUT_template_should_return_404_when_template_does_not_exist()
    {
        // Arrange
        var accountName = TestUsersData.EMAIL_TEST1;
        var templateId = 456;
        var url = $"/accounts/{accountName}/templates/{templateId}";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new
        {
            meta = new { },
            htmlContent = "HTML CONTENT",
            templateName = "TemplateName",
            type = "unlayer"
        });

        var repositoryMock = new Mock<ITemplateRepository>();
        repositoryMock
            .Setup(x => x.GetOwnOrPublicTemplate(accountName, templateId))
            .ReturnsAsync((TemplateModel)null);

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Template not found, belongs to a different account, or it is a public template.", responseContent);
    }

    [Fact]
    public async Task PUT_template_should_return_404_when_template_is_public()
    {
        // Arrange
        var accountName = TestUsersData.EMAIL_TEST1;
        var templateId = 456;
        var url = $"/accounts/{accountName}/templates/{templateId}";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var jsonContent = JsonContent.Create(new
        {
            meta = new { },
            htmlContent = "HTML CONTENT",
            templateName = "TemplateName",
            type = "unlayer"
        });

        var repositoryMock = new Mock<ITemplateRepository>();
        repositoryMock
            .Setup(x => x.GetOwnOrPublicTemplate(accountName, templateId))
            .ReturnsAsync(new TemplateModel(
                TemplateId: templateId,
                IsPublic: true,
                PreviewImage: "PreviewImage",
                Name: "Name",
                Content: new UnlayerTemplateContentData(
                    HtmlComplete: "HtmlComplete",
                    Meta: "Meta")));

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Template not found, belongs to a different account, or it is a public template.", responseContent);
    }

    [Fact]
    public async Task PUT_template_should_save_the_new_data()
    {
        // Arrange
        var accountName = TestUsersData.EMAIL_TEST1;
        var templateId = 456;
        var url = $"/accounts/{accountName}/templates/{templateId}";
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

        var repositoryMock = new Mock<ITemplateRepository>();
        repositoryMock
            .Setup(x => x.GetOwnOrPublicTemplate(accountName, templateId))
            .ReturnsAsync(new TemplateModel(
                TemplateId: templateId,
                IsPublic: false,
                PreviewImage: "OLD PREVIEW IMAGE",
                Name: "OLD NAME",
                Content: new UnlayerTemplateContentData(
                    HtmlComplete: "OLD HTML CONTENT",
                    Meta: "{\"test\":\"OLD META\"}")));

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            token);

        // Act
        var response = await client.PutAsync(url, jsonContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TemplateModel savedData = null;
        repositoryMock.Verify(x => x.UpdateTemplate(
            It.Is<TemplateModel>(y => AssertHelper.GetValueAndContinue(y, out savedData))));
        Assert.False(savedData.IsPublic);
        Assert.Equal(templateName, savedData.Name);
        Assert.Equal(previewImage, savedData.PreviewImage);
        Assert.Equal(templateId, savedData.TemplateId);
        var unlayerContent = Assert.IsType<UnlayerTemplateContentData>(savedData.Content);
        Assert.Equal(htmlContent, unlayerContent.HtmlComplete);
        Assert.Equal(expectedMeta, unlayerContent.Meta);
    }
}
