using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TUD = Doppler.HtmlEditorApi.Test.Utils.TestUsersData;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi
{
    public class AuthorizationTest
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _output;

        public AuthorizationTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData("/hello/anonymous", HttpStatusCode.OK)]
        public async Task GET_helloAnonymous_should_not_require_token(string url, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData("/hello/anonymous", TUD.TOKEN_EMPTY, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_EXPIRE_20961002, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_EXPIRE_20010908, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_BROKEN, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_SUPERUSER_EXPIRE_20961002, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_TEST1_EXPIRE_20961002, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/anonymous", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.OK)]
        public async Task GET_helloAnonymous_should_accept_any_token(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData("/hello/valid-token", HttpStatusCode.Unauthorized)]
        [InlineData("/hello/superuser", HttpStatusCode.Unauthorized)]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", HttpStatusCode.Unauthorized)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", HttpStatusCode.Unauthorized)]
        public async Task GET_authenticated_endpoints_should_require_token(string url, HttpStatusCode expectedStatusCode)
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
        [InlineData("/hello/valid-token", TUD.TOKEN_EMPTY, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/valid-token", TUD.TOKEN_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/valid-token", TUD.TOKEN_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData("/hello/valid-token", TUD.TOKEN_BROKEN, HttpStatusCode.Unauthorized, "invalid_token", "")]
        [InlineData("/hello/valid-token", TUD.TOKEN_SUPERUSER_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/valid-token", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData("/hello/valid-token", TUD.TOKEN_TEST1_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/valid-token", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData("/hello/superuser", TUD.TOKEN_EMPTY, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/superuser", TUD.TOKEN_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/superuser", TUD.TOKEN_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData("/hello/superuser", TUD.TOKEN_BROKEN, HttpStatusCode.Unauthorized, "invalid_token", "")]
        [InlineData("/hello/superuser", TUD.TOKEN_SUPERUSER_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/superuser", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData("/hello/superuser", TUD.TOKEN_TEST1_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData("/hello/superuser", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_EMPTY, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_BROKEN, HttpStatusCode.Unauthorized, "invalid_token", "")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_TEST1_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_EMPTY, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_BROKEN, HttpStatusCode.Unauthorized, "invalid_token", "")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_TEST1_EXPIRE_20961002, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token has no expiration\"")]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized, "invalid_token", "error_description=\"The token expired at")]
        public async Task GET_authenticated_endpoints_should_require_a_valid_token(string url, string token, HttpStatusCode expectedStatusCode, string error, string extraErrorInfo)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.StartsWith("Bearer", response.Headers.WwwAuthenticate.ToString());
            Assert.Contains($"error=\"{error}\"", response.Headers.WwwAuthenticate.ToString());
            Assert.Contains(extraErrorInfo, response.Headers.WwwAuthenticate.ToString());
        }

        [Theory]
        [InlineData("/hello/valid-token", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/valid-token", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/valid-token", TUD.TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData("/hello/valid-token", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.OK)]
        public async Task GET_helloValidToken_should_accept_valid_token(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData("/hello/superuser", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/hello/superuser", TUD.TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/hello/superuser", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        public async Task GET_helloSuperUser_should_require_a_valid_token_with_isSU_flag(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData("/hello/superuser", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        public async Task GET_helloSuperUser_should_accept_valid_token_with_isSU_flag(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/accounts/456/hello", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_SUPERUSER_FALSE_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        [InlineData("/accounts/test2@test.com/hello", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
        public async Task GET_account_endpoint_should_require_a_valid_token_with_isSU_flag_or_a_token_for_the_right_account(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData($"/accounts/{TUD.ID_TEST1_S}/hello", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_SUPERUSER_EXPIRE_20330518, HttpStatusCode.OK)]
        [InlineData($"/accounts/{TUD.EMAIL_TEST1}/hello", TUD.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.OK)]
        public async Task GET_account_endpoint_should_accept_valid_token_with_isSU_flag_or_a_token_for_the_right_account(string url, string token, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
