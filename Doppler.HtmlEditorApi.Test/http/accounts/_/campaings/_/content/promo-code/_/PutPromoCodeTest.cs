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

public class PutPromoCodeTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly ITestOutputHelper _output;
    private readonly WebApplicationFactory<Startup> _factory;
    public PutPromoCodeTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task PUT_promo_code_should_require_token()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        var url = "/accounts/x@x.com/campaigns/111/content/promo-code/222";
        var expectedStatusCode = HttpStatusCode.Unauthorized;

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new { }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData("/accounts/x@x.com/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task PUT_promo_code_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true
        }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task PUT_promo_code_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true
        }));
        _output.WriteLine(response.GetHeadersAsString());

        // Assert
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("invalid_token", response.Headers.WwwAuthenticate.ToString());
        Assert.Contains("token expired", response.Headers.WwwAuthenticate.ToString());
    }

    [Theory]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, 222)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/111/content/promo-code/222", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, 222)]
    public async Task PUT_promo_code_should_accept_right_tokens_and_return_Ok(string url, string token, int promoCodeId)
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();

        dbContextMock.Setup(x => x.ExecuteAsync(It.Is<UpdatePromoCodeDbQuery>(q => q.Id == promoCodeId))).ReturnsAsync(1);

        var client = _factory.CreateSutClient(serviceToOverride1: dbContextMock.Object, token);

        // Act
        var response = await client.PutAsync(url, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true
        }));
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PUT_promo_code_should_return_NotFound_when_campaign_promocode_relation_does_not_exist()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();

        var campaignId = 111;
        var promoCodeId = 222;
        var endpointPath = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/{campaignId}/content/promo-code/{promoCodeId}";
        var token = TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518;

        // Return 0, it does not found CampaignId/PromoCodeId relation to update
        dbContextMock
            .Setup(x => x.ExecuteAsync(It.Is<UpdatePromoCodeDbQuery>(q => q.Id == promoCodeId && q.IdCampaign == campaignId)))
            .ReturnsAsync(0);

        var client = _factory.CreateSutClient(serviceToOverride1: dbContextMock.Object, token);

        // Act
        var response = await client.PutAsync(endpointPath, JsonContent.Create(new
        {
            type = "type",
            value = 10,
            includeShipping = true,
            firstPurchase = true,
        }));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal("The Campaign/PromoCode relation doesn't exist.", responseContent);
    }
}
