using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Infrastructure;
using Doppler.HtmlEditorApi.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi
{
    public class GetCampaignTest
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        const string TOKEN_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwMDAwMDAwMDB9.mll33c0kstVIN9Moo4HSw0CwRjn0IuDc2h1wkRrv2ahQtIG1KV5KIxYw-H3oRfd-PiCWHhIVIYDP3mWDZbsOHTlnpRGpHp4f26LAu1Xp1hDJfOfxKYEGEE62Xt_0qp7jSGQjrx-vQey4l2mNcWkOWiE0plOws7cX-wLUvA3NLPoOvEegjM0Wx6JFcvYLdMGcTGT5tPd8Pq8pe9VYstCbhOClzI0bp81iON3f7VQP5d0n64eb_lvEPFu5OfURD4yZK2htyQK7agcNNkP1c5mLEfUi39C7Qtx96aAhOjir6Wfhzv_UEs2GQKXGTHl6_-HH-ecgOdIvvbqXGLeDmTkXUQ";
        const string TOKEN_SUPERUSER_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        const string TOKEN_SUPERUSER_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjEwMDAwMDAwMDB9.FYOpOxrXSHDif3lbQLPEStMllzEktWPKQ2T4vKUq5qgVjiH_ki0W0Ansvt0PMlaLHqq7OOL9XGFebtgUcyU6aXPO9cZuq6Od196TWDLMdnxZ-Ct0NxWxulyMbjTglUiI3V6g3htcM5EaurGvfu66kbNDuHO-WIQRYFfJtbm7EuOP7vYBZ26hf5Vk5KvGtCWha4zRM55i1-CKMhXvhPN_lypn6JLENzJGYHkBC9Cx2DwzaT683NWtXiVzeMJq3ohC6jvRpkezv89QRes2xUW4fRgvgRGQvaeQ4huNW_TwQKTTikH2Jg7iHbuRqqwYuPZiWuRkjqfd8_80EdlSAnO94Q";
        const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoyMDAwMDAwMDAwfQ.E3RHjKx9p0a-64RN2YPtlEMysGM45QBO9eATLBhtP4tUQNZnkraUr56hAWA-FuGmhiuMptnKNk_dU3VnbyL6SbHrMWUbquxWjyoqsd7stFs1K_nW6XIzsTjh8Bg6hB5hmsSV-M5_hPS24JwJaCdMQeWrh6cIEp2Sjft7I1V4HQrgzrkMh15sDFAw3i1_ZZasQsDYKyYbO9Jp7lx42ognPrz_KuvPzLjEXvBBNTFsVXUE-ur5adLNMvt-uXzcJ1rcwhjHWItUf5YvgRQbbBnd9f-LsJIhfkDgCJcvZmGDZrtlCKaU1UjHv5c3faZED-cjL59MbibofhPjv87MK8hhdg";
        const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoxMDAwMDAwMDAwfQ.JBmiZBgKVSUtB4_NhD1kiUhBTnH2ufGSzcoCwC3-Gtx0QDvkFjy2KbxIU9asscenSdzziTOZN6IfFx6KgZ3_a3YB7vdCgfSINQwrAK0_6Owa-BQuNAIsKk-pNoIhJ-OcckV-zrp5wWai3Ak5Qzg3aZ1NKZQKZt5ICZmsFZcWu_4pzS-xsGPcj5gSr3Iybt61iBnetrkrEbjtVZg-3xzKr0nmMMqe-qqeknozIFy2YWAObmTkrN4sZ3AB_jzqyFPXN-nMw3a0NxIdJyetbESAOcNnPLymBKZEZmX2psKuXwJxxekvgK9egkfv2EjKYF9atpH5XwC0Pd4EWvraLAL2eg";

        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _output;

        public GetCampaignTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData("/accounts/x@x.com/campaigns/123/content", HttpStatusCode.Unauthorized)]
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
        [InlineData("/accounts/x@x.com/campaigns/123/content", TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/accounts/x@x.com/campaigns/123/content", TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
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
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
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
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518, "test1@test.com", 123)]
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_SUPERUSER_EXPIRE_20330518, "test1@test.com", 123)]
        [InlineData("/accounts/otro@test.com/campaigns/123/content", TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 123)]
        public async Task GET_campaign_should_accept_right_tokens_and_return_404_when_DB_returns_null(string url, string token, string expectedAccountName, int expectedIdCampaign)
        {
            // Arrange
            // TODO: consider to mock Dapper in place of IRepository
            ContentRow emptyContentModel = null;
            var repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.GetCampaignModel(expectedAccountName, expectedIdCampaign))
                .ReturnsAsync(emptyContentModel);

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
            var response = await client.GetAsync(url);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518, "test1@test.com", 123)]
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_SUPERUSER_EXPIRE_20330518, "test1@test.com", 123)]
        [InlineData("/accounts/otro@test.com/campaigns/123/content", TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 123)]
        public async Task GET_campaign_should_accept_right_tokens_and_return_content_as_it_is_in_DB(string url, string token, string expectedAccountName, int expectedIdCampaign)
        {
            // Arrange
            var expectedSchemaVersion = 999;
            var contentRow = ContentRow.CreateUnlayerContentRow(
                meta: JsonSerializer.Serialize(new
                {
                    schemaVersion = expectedSchemaVersion
                }),
                content: "<html></html>",
                idCampaign: expectedIdCampaign);

            // TODO: consider to mock Dapper in place of IRepository
            var repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.GetCampaignModel(expectedAccountName, expectedIdCampaign))
                .ReturnsAsync(contentRow);

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
        [InlineData("/accounts/test1@test.com/campaigns/123/content", TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518, "test1@test.com", 123)]
        public async Task GET_campaign_should_return_html_content(string url, string token, string expectedAccountName, int expectedIdCampaign)
        {
            var html = "<html></html>";
            ContentRow contentRow = new ContentRow()
            {
                Meta = null,
                Content = html,
                EditorType = null,
                IdCampaign = expectedIdCampaign
            };

            // TODO: consider to mock Dapper in place of IRepository
            var repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.GetCampaignModel(expectedAccountName, expectedIdCampaign))
                .ReturnsAsync(contentRow);

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
    }
}
