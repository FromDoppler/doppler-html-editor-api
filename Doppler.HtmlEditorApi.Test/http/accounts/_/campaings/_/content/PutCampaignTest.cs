using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Doppler.HtmlEditorApi.Repositories;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using Xunit.Abstractions;

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
    [InlineData("/accounts/x@x.com/campaigns/456/content", HttpStatusCode.Unauthorized)]
    public async Task PUT_campaign_should_require_token(string url, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { htmlContent = "", meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/campaigns/456/content", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task PUT_campaign_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { htmlContent = "", meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task PUT_campaign_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { htmlContent = "", meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("invalid_token", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("token expired", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1)]
    [InlineData("/accounts/otro@test.com/campaigns/456/content", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com")]
    public async Task PUT_campaign_should_accept_right_tokens_and_return_Ok(string url, string token, string expectedAccountName)
    {
        // Arrange
        var repositoryMock = new Mock<ICampaignContentRepository>();

        repositoryMock
            .Setup(x => x.GetCampaignState(expectedAccountName, It.IsAny<int>()))
            .ReturnsAsync(new CampaignState(true, true, null, CampaignStatus.DRAFT));
        repositoryMock
            .Setup(x => x.UpdateCampaignContent(expectedAccountName, It.IsAny<BaseHtmlContentData>()))
            .Returns(Task.CompletedTask);

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            Mock.Of<IFieldsRepository>(),
            token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent = "<html></html>",
            meta = new { }
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        repositoryMock.VerifyAll();
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    public async Task PUT_campaign_should_return_error_when_htmlContent_is_not_present(string url, string token)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"htmlContent\":\\[\"The htmlContent field is required.\"\\]", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    public async Task PUT_campaign_should_return_error_when_htmlContent_is_a_empty_string(string url, string token)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { htmlContent = "", meta = new { } }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"htmlContent\":\\[\"The htmlContent field is required.\"\\]", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    public async Task PUT_campaign_should_return_error_when_type_is_unlayer_and_meta_is_not_present(string url, string token)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent = "<html></html>"
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"meta\":\\[\"The meta field is required for unlayer content.\"\\]", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    public async Task PUT_campaign_should_return_error_when_type_is_unlayer_and_meta_is_empty_string(string url, string token)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent = "<html></html>",
            meta = ""
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"meta\":\\[\"The meta field is required for unlayer content.\"\\]", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    public async Task PUT_campaign_should_return_error_when_type_is_not_defined(string url, string token)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            htmlContent = "<html></html>",
            meta = ""
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"type\":\\[\"The type field is required.\"\\]", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, "")]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, null)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, "noexisto")]
    public async Task PUT_campaign_should_return_error_when_type_is_invalid(string url, string token, string type)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type,
            htmlContent = "<html></html>",
            meta = ""
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches("\"$.type\":\\[\"The JSON value could not be converted to Doppler.HtmlEditorApi.Model.CampaignContent. Path: $.type | LineNumber: \\d+ | BytePositionInLine: \\d+.\"\\]", responseContent);
    }

    [Theory]
    [InlineData("html")]
    [InlineData("unlayer")]
    public async Task PUT_campaign_should_return_404_error_when_campaign_does_not_exist(string type)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "My HTML Content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = false,
                ContentExists = false,
                EditorType = null,
                Status = null
            });

        var client = _factory.CreateSutClient(
            dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = type,
            htmlContent,
            meta = "true" // it does not care
        }));

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData("html")]
    [InlineData("unlayer")]
    public async Task PUT_campaign_should_return_404_error_when_user_does_not_exist(string type)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "My HTML Content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = false,
                ContentExists = false,
                EditorType = null,
                Status = null
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = type,
            htmlContent,
            meta = "true" // it does not care
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData("html", CampaignStatus.OTHER)]
    [InlineData("unlayer", CampaignStatus.OTHER)]
    public async Task PUT_campaign_should_return_bad_request_error_when_campaign_is_not_writable(string type, CampaignStatus campaignStatus)
    {
        // Arrange
        var repositoryMock = new Mock<ICampaignContentRepository>();
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var htmlContent = "My HTML Content";
        Regex matchTitle = new Regex("\"title\"\\s*:\\s*\"The campaign content is read only\"");
        Regex matchDetail = new Regex("\"detail\"\\s*:\\s*\"The content cannot be edited because status campaign is OTHER\"");

        repositoryMock
            .Setup(x => x.GetCampaignState(expectedAccountName, It.IsAny<int>()))
            .ReturnsAsync(new CampaignState(true, true, null, campaignStatus));

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            Mock.Of<IFieldsRepository>(),
            token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = type,
            htmlContent,
            meta = new { }
        }));

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Matches(matchTitle, responseContent);
        Assert.Matches(matchDetail, responseContent);
        repositoryMock.VerifyAll();
        repositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, true, "UPDATE")]
    [InlineData(55, true, "UPDATE")]
    [InlineData(4, true, "UPDATE")]
    [InlineData(5, true, "UPDATE")]
    [InlineData(null, false, "INSERT")]
    public async Task PUT_campaign_should_store_html_content_and_ensure_campaign_status(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "My HTML Content";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = contentExists,
                EditorType = currentEditorType,
                Status = 1
            });

        dbContextMock
            .SetupInsertOrUpdateContentRow(
                sqlQueryStartsWith,
                expectedIdCampaign,
                htmlContent,
                meta: null,
                result: 1);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "html",
            htmlContent
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Fact]
    public async Task PUT_campaign_should_store_unlayer_content_and_ensure_campaign_status_when_content_no_exist()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "My HTML Content";
        var metaAsString = "{\"data\":\"My Meta Content\"}";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = false,
                EditorType = null,
                Status = 1
            });

        dbContextMock
            .SetupInsertOrUpdateContentRow(
                "INSERT",
                expectedIdCampaign,
                htmlContent,
                metaAsString,
                result: 1);

        var setCurrentStep = 2;
        var setHtmlSourceType = 2;
        var whenCurrentStepIs = 1;
        var whenIdCampaignIs = expectedIdCampaign;
        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new UpdateCampaignStatusDbQuery(
                    setCurrentStep,
                    setHtmlSourceType,
                    whenIdCampaignIs,
                    whenCurrentStepIs)))
            .ReturnsAsync(1);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement(metaAsString)
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData(null, true, "UPDATE")]
    [InlineData(55, true, "UPDATE")]
    [InlineData(4, true, "UPDATE")]
    [InlineData(5, true, "UPDATE")]
    public async Task PUT_campaign_should_store_unlayer_content_and_ensure_campaign_status(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "My HTML Content";
        var metaAsString = "{\"data\":\"My Meta Content\"}";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = contentExists,
                EditorType = currentEditorType,
                Status = 1
            });

        dbContextMock
            .SetupInsertOrUpdateContentRow(
                sqlQueryStartsWith,
                expectedIdCampaign,
                htmlContent,
                metaAsString,
                result: 1);

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement(metaAsString)
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData("[[[firstname]]]", "VALUES (319)")]
    [InlineData("[[[firstname]]] [[[lastname]]]", "VALUES (319),(320)")]
    [InlineData("[[[firstname]]] [[[lastname]]] [[[noexist]]]", "VALUES (319),(320)")]
    public async Task PUT_campaign_should_store_field_relations(string htmlContent, string expectedSubQuery)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = 1
            });

        dbContextMock.SetupBasicFields();

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement("{}")
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<SaveNewCampaignFields>(q =>
                q.IdContent == expectedIdCampaign
                && q.SqlQueryContains(expectedSubQuery))
        ), Times.Once);
    }

    [Fact]
    public async Task PUT_campaign_should_no_store_field_relations_when_no_fields()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "<html>No content</html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = 1
            });

        dbContextMock.SetupBasicFields();

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement("{}")
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<SaveNewCampaignFields>()
        ), Times.Never);
    }

    [Theory]
    [InlineData(
        $@"<a href=""https://www.google.com"">Google</a><br>
            <a href=""{"\n"} https://{"\t"}goo gle1{"\n"}.com    {"\r\n"}  "">Google1 (dirty)</a><br>
            <a href=""https://google2.com"">Google2</a><br>
            <a href=""{"\n"} https://{"\t"}goo gle2{"\n"}.com    {"\r\n"}  "">Google2 (dirty)</a><br>",
        new[] { "https://www.google.com", "https://google1.com", "https://google2.com" })]
    [InlineData(
        @"<a href=""https://www.google.com?q=[[[firstname]]]"">Find my name!</a><br>
            <a href=""https://www.google.com?q=[[[apellido]]]"">Find my lastname</a><br>
            <a href=""https://www.google.com?q=[[[nombre]]]"">Find my name again!</a>",
        new[] { "https://www.google.com?q=|*|319*|*", "https://www.google.com?q=|*|320*|*" })]
    public async Task PUT_campaign_should_add_and_remove_link_relations(string htmlContent, string[] expectedLinks)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = 1
            });

        dbContextMock.SetupBasicFields();

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement("{}")
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();

        string[] linksSendToSaveNewCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<SaveNewCampaignLinks>(q =>
                q.IdContent == expectedIdCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToSaveNewCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToSaveNewCampaignLinks);

        string[] linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
                q.IdContent == expectedIdCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks);

        string[] linksSendToDeleteRemovedCampaignLinks = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteRemovedCampaignLinks>(q =>
                q.IdContent == expectedIdCampaign
                && AssertHelper.GetValueAndContinue(q.Links.ToArray(), out linksSendToDeleteRemovedCampaignLinks))
        ), Times.Once);
        Assert.Equal(expectedLinks, linksSendToDeleteRemovedCampaignLinks);
    }

    [Fact]
    public async Task PUT_campaign_should_no_add_links_relations_when_no_links()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "<html>No content</html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = 1
            });

        dbContextMock.SetupBasicFields();

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement("{}")
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();

        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<SaveNewCampaignLinks>()
        ), Times.Never);
    }

    [Fact]
    public async Task PUT_campaign_should_remove_links_relations_when_no_links()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var htmlContent = "<html>No content</html>";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 5,
                Status = 1
            });

        dbContextMock.SetupBasicFields();

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "unlayer",
            htmlContent,
            meta = Utils.ParseAsJsonElement("{}")
        }));
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();

        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
                q.IdContent == expectedIdCampaign
                && !q.Links.Any())
        ), Times.Once);

        dbContextMock.Verify(x => x.ExecuteAsync(
            It.Is<DeleteRemovedCampaignLinks>(q =>
                q.IdContent == expectedIdCampaign
                && !q.Links.Any())
        ), Times.Once);
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
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TestUsersData.EMAIL_TEST1;
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
                Status = 1
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
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TestUsersData.EMAIL_TEST1;
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
                Status = 1
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
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var accountName = TestUsersData.EMAIL_TEST1;
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
                Status = 1
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
