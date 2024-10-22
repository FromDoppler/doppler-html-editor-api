using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi;

public class CreatePromoCodeTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public CreatePromoCodeTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task POST_promo_code_should_require_token()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        var url = "/accounts/x@x.com/campaigns/111/content/promo-code";
        var expectedStatusCode = HttpStatusCode.Unauthorized;

        // Act
        var response = await client.PostAsync(url, JsonContent.Create(new { }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Fact]
    public async Task POST_promo_code_should_not_accept_the_token_of_another_account()
    {
        // Arrange
        var url = "/accounts/x@x.com/campaigns/111/content/promo-code";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedStatusCode = HttpStatusCode.Forbidden;
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task POST_promo_code_should_not_accept_a_expired_token()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20010908;
        var expectedStatusCode = HttpStatusCode.Unauthorized;
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code", TestUsersData.TOKEN_TEST1_EXPIRE_20330518)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518)]
    public async Task POST_promo_code_should_accept_right_tokens_and_call_repository_and_return_createdResourceId(string url, string token)
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var idCampaign = 111;
        var newIdDynamicContentPromoCode = 10;

        dbContextMock.SetupCampaignStatus(
            TestUsersData.EMAIL_TEST1,
            idCampaign,
            new()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = 4,
                Status = 1,
                TestType = null,
            });

        dbContextMock.Setup(x => x.ExecuteAsync(It.Is<InsertPromoCodeDbQuery>(q => q.IdCampaign == idCampaign)))
        .ReturnsAsync(new InsertPromoCodeDbQuery.Result()
        {
            IdDynamicContentPromoCode = newIdDynamicContentPromoCode
        });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true
        }));
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Matches($$"""{"createdResourceId":{{newIdDynamicContentPromoCode}}}""", responseContent);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1)]
    [InlineData("/accounts/otro@test.com/campaigns/111/content/promo-code", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com")]
    public async Task POST_promo_code_should_accept_right_tokens_and_return_404_when_not_exist(string url, string token, string accountName)
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var idCampaign = 111;

        dbContextMock.SetupCampaignStatus(
            accountName,
            idCampaign,
            new()
            {
                OwnCampaignExists = false,
                ContentExists = true,
                EditorType = 4,
                Status = 1,
                TestType = null,
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true
        }));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
