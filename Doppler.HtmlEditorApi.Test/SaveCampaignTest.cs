using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Storage;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using TUD = Doppler.HtmlEditorApi.Test.Utils.TestUsersData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi
{
    public class SaveCampaignTest
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _output;

        public SaveCampaignTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
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
        [InlineData("/accounts/x@x.com/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/accounts/x@x.com/campaigns/456/content", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518, TUD.EMAIL_TEST1)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, TUD.EMAIL_TEST1)]
        [InlineData("/accounts/otro@test.com/campaigns/456/content", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com")]
        public async Task PUT_campaign_should_accept_right_tokens_and_return_Ok(string url, string token, string expectedAccountName)
        {
            // Arrange
            var repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.SaveCampaignContent(expectedAccountName, It.IsAny<BaseHtmlContentData>()))
                .Returns(Task.CompletedTask);

            var client = _factory
                .WithWebHostBuilder(c =>
                {
                    c.ConfigureServices(s =>
                    {
                        s.AddSingleton(repositoryMock.Object);
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.PutAsync(url, JsonContent.Create(new
            {
                type = "unlayer",
                htmlContent = "<html></html>",
                meta = new { }
            }));
            _output.WriteLine(response.GetHeadersAsString());
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518)]
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
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518, "")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518, null)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content", TUD.TOKEN_TEST1_EXPIRE_20330518, "noexisto")]
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
        public async Task PUT_campaign_should_return_500_error_when_campaign_does_not_exist(string type)
        {
            // Arrange
            var url = $"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content";
            var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
            var expectedAccountName = TUD.EMAIL_TEST1;
            var expectedIdCampaign = 456;
            var htmlContent = "My HTML Content";

            var dbContextMock = new Mock<IDbContext>();
            dbContextMock
                .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                    It.IsAny<string>(),
                    It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                        x.AccountName == expectedAccountName
                        && x.IdCampaign == expectedIdCampaign)))
                .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
                {
                    OwnCampaignExists = false,
                    ContentExists = false,
                    EditorType = null,
                });

            var client = _factory
                .WithWebHostBuilder(c =>
                {
                    c.ConfigureServices(s =>
                    {
                        s.AddSingleton(dbContextMock.Object);
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            dbContextMock.VerifyAll();
            dbContextMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("html")]
        [InlineData("unlayer")]
        public async Task PUT_campaign_should_return_500_error_when_user_does_not_exist(string type)
        {
            // Arrange
            var url = $"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content";
            var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
            var expectedAccountName = TUD.EMAIL_TEST1;
            var expectedIdCampaign = 456;
            var htmlContent = "My HTML Content";

            var dbContextMock = new Mock<IDbContext>();
            dbContextMock
                .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                    It.IsAny<string>(),
                    It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                        x.AccountName == expectedAccountName
                        && x.IdCampaign == expectedIdCampaign)))
                .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
                {
                    OwnCampaignExists = false,
                    ContentExists = false,
                    EditorType = null,
                });

            var client = _factory
                .WithWebHostBuilder(c =>
                {
                    c.ConfigureServices(s =>
                    {
                        s.AddSingleton(dbContextMock.Object);
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            dbContextMock.VerifyAll();
            dbContextMock.VerifyNoOtherCalls();
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
            var url = $"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content";
            var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
            var expectedAccountName = TUD.EMAIL_TEST1;
            var expectedIdCampaign = 456;
            var htmlContent = "My HTML Content";

            var dbContextMock = new Mock<IDbContext>();
            dbContextMock
                .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                    It.IsAny<string>(),
                    It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                        x.AccountName == expectedAccountName
                        && x.IdCampaign == expectedIdCampaign)))
                .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
                {
                    OwnCampaignExists = true,
                    ContentExists = contentExists,
                    EditorType = currentEditorType,
                });

            dbContextMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<string>(s => s.Trim().StartsWith(sqlQueryStartsWith)),
                    It.Is<ContentRow>(c =>
                        c.IdCampaign == expectedIdCampaign
                        && c.Content == htmlContent
                        && c.Meta == null)))
                .ReturnsAsync(1);

            dbContextMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<string>(s => s.Trim().StartsWith("UPDATE Campaign")),
                    It.Is<UpdateCampaignStatusDbQuery.Parameters>(c =>
                        c.setCurrentStep == 2
                        && c.setHtmlSourceType == 2
                        && c.whenCurrentStepIs == 1
                        && c.whenIdCampaignIs == expectedIdCampaign)))
                .ReturnsAsync(1);

            var client = _factory
                .WithWebHostBuilder(c =>
                {
                    c.ConfigureServices(s =>
                    {
                        s.AddSingleton(dbContextMock.Object);
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            dbContextMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null, true, "UPDATE")]
        [InlineData(55, true, "UPDATE")]
        [InlineData(4, true, "UPDATE")]
        [InlineData(5, true, "UPDATE")]
        [InlineData(null, false, "INSERT")]
        public async Task PUT_campaign_should_store_unlayer_content_and_ensure_campaign_status(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
        {
            // Arrange
            var url = $"/accounts/{TUD.EMAIL_TEST1}/campaigns/456/content";
            var token = TUD.TOKEN_TEST1_EXPIRE_20330518;
            var expectedAccountName = TUD.EMAIL_TEST1;
            var expectedIdCampaign = 456;
            var htmlContent = "My HTML Content";
            var metaAsString = "{\"data\":\"My Meta Content\"}";

            var dbContextMock = new Mock<IDbContext>();
            dbContextMock
                .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                    It.IsAny<string>(),
                    It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                        x.AccountName == expectedAccountName
                        && x.IdCampaign == expectedIdCampaign)))
                .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
                {
                    OwnCampaignExists = true,
                    ContentExists = contentExists,
                    EditorType = currentEditorType,
                });

            dbContextMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<string>(s => s.Trim().StartsWith(sqlQueryStartsWith)),
                    It.Is<ContentRow>(c =>
                        c.IdCampaign == expectedIdCampaign
                        && c.Content == htmlContent
                        && c.Meta == metaAsString)))
                .ReturnsAsync(1);

            dbContextMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<string>(s => s.Trim().StartsWith("UPDATE Campaign")),
                    It.Is<UpdateCampaignStatusDbQuery.Parameters>(c =>
                        c.setCurrentStep == 2
                        && c.setHtmlSourceType == 2
                        && c.whenCurrentStepIs == 1
                        && c.whenIdCampaignIs == expectedIdCampaign)))
                .ReturnsAsync(1);

            var client = _factory
                .WithWebHostBuilder(c =>
                {
                    c.ConfigureServices(s =>
                    {
                        s.AddSingleton(dbContextMock.Object);
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            dbContextMock.VerifyNoOtherCalls();
        }
    }
}
