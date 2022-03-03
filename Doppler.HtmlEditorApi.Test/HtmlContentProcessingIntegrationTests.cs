using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;
using Xunit.Abstractions;
using Xunit;

namespace Doppler.HtmlEditorApi;

public class HtmlContentProcessingIntegrationTests
    : IClassFixture<WebApplicationFactory<Startup>>
{
    const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoyMDAwMDAwMDAwfQ.E3RHjKx9p0a-64RN2YPtlEMysGM45QBO9eATLBhtP4tUQNZnkraUr56hAWA-FuGmhiuMptnKNk_dU3VnbyL6SbHrMWUbquxWjyoqsd7stFs1K_nW6XIzsTjh8Bg6hB5hmsSV-M5_hPS24JwJaCdMQeWrh6cIEp2Sjft7I1V4HQrgzrkMh15sDFAw3i1_ZZasQsDYKyYbO9Jp7lx42ognPrz_KuvPzLjEXvBBNTFsVXUE-ur5adLNMvt-uXzcJ1rcwhjHWItUf5YvgRQbbBnd9f-LsJIhfkDgCJcvZmGDZrtlCKaU1UjHv5c3faZED-cjL59MbibofhPjv87MK8hhdg";
    const string TOKEN_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwMDAwMDAwMDB9.mll33c0kstVIN9Moo4HSw0CwRjn0IuDc2h1wkRrv2ahQtIG1KV5KIxYw-H3oRfd-PiCWHhIVIYDP3mWDZbsOHTlnpRGpHp4f26LAu1Xp1hDJfOfxKYEGEE62Xt_0qp7jSGQjrx-vQey4l2mNcWkOWiE0plOws7cX-wLUvA3NLPoOvEegjM0Wx6JFcvYLdMGcTGT5tPd8Pq8pe9VYstCbhOClzI0bp81iON3f7VQP5d0n64eb_lvEPFu5OfURD4yZK2htyQK7agcNNkP1c5mLEfUi39C7Qtx96aAhOjir6Wfhzv_UEs2GQKXGTHl6_-HH-ecgOdIvvbqXGLeDmTkXUQ";
    const string TOKEN_SUPERUSER_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjEwMDAwMDAwMDB9.FYOpOxrXSHDif3lbQLPEStMllzEktWPKQ2T4vKUq5qgVjiH_ki0W0Ansvt0PMlaLHqq7OOL9XGFebtgUcyU6aXPO9cZuq6Od196TWDLMdnxZ-Ct0NxWxulyMbjTglUiI3V6g3htcM5EaurGvfu66kbNDuHO-WIQRYFfJtbm7EuOP7vYBZ26hf5Vk5KvGtCWha4zRM55i1-CKMhXvhPN_lypn6JLENzJGYHkBC9Cx2DwzaT683NWtXiVzeMJq3ohC6jvRpkezv89QRes2xUW4fRgvgRGQvaeQ4huNW_TwQKTTikH2Jg7iHbuRqqwYuPZiWuRkjqfd8_80EdlSAnO94Q";
    const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoxMDAwMDAwMDAwfQ.JBmiZBgKVSUtB4_NhD1kiUhBTnH2ufGSzcoCwC3-Gtx0QDvkFjy2KbxIU9asscenSdzziTOZN6IfFx6KgZ3_a3YB7vdCgfSINQwrAK0_6Owa-BQuNAIsKk-pNoIhJ-OcckV-zrp5wWai3Ak5Qzg3aZ1NKZQKZt5ICZmsFZcWu_4pzS-xsGPcj5gSr3Iybt61iBnetrkrEbjtVZg-3xzKr0nmMMqe-qqeknozIFy2YWAObmTkrN4sZ3AB_jzqyFPXN-nMw3a0NxIdJyetbESAOcNnPLymBKZEZmX2psKuXwJxxekvgK9egkfv2EjKYF9atpH5XwC0Pd4EWvraLAL2eg";
    const string TOKEN_SUPERUSER_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";

    #region Content examples
    const string META_CONTENT = "{\"body\":{\"rows\":[]},\"example\":true}";
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
    const string HTML_WITHOUT_BODY = $@"<!doctype html>
        <html>
            <head>
                {HEAD_CONTENT}
            </head>
        </html>";
    #endregion Content examples

    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;

    public HtmlContentProcessingIntegrationTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", true)]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", false)]
    [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "unlayer", true)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", true)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", false)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "unlayer", true)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", true)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", false)]
    [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "unlayer", true)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", true)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", false)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "unlayer", true)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", true)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", false)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "unlayer", true)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", true)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", false)]
    [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "unlayer", true)]
    public async Task PUT_campaign_should_split_html_in_head_and_content(string htmlInput, string expectedHead, string expectedContent, string type, bool existingContent)
    {
        // Arrange
        var url = "/accounts/test1@test.com/campaigns/123/content";
        var token = TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518;
        var accountName = "test1@test.com";
        var idCampaign = 123;

        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                    x.AccountName == accountName
                    && x.IdCampaign == idCampaign)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = existingContent,
                EditorType = null,
            });

        // dbContextMock
        //     .Setup(x => x.QueryAsync<QueryActiveBasicFieldsDbQuery.Result>(
        //         It.IsAny<string>()))
        //     .ReturnsAsync(new QueryActiveBasicFieldsDbQuery.Result[]
        //     {
        //         new () { IdField = 319, Name = "FIRST_NAME" },
        //         new () { IdField = 320, Name = "LAST_NAME" },
        //         new () { IdField = 321, Name = "EMAIL" },
        //         new () { IdField = 322, Name = "GENDER" },
        //         new () { IdField = 323, Name = "BIRTHDAY" },
        //         new () { IdField = 324, Name = "COUNTRY" },
        //         new () { IdField = 325, Name = "CONSENT" },
        //         new () { IdField = 326, Name = "ORIGIN" },
        //         new () { IdField = 327, Name = "SCORE" },
        //         new () { IdField = 106667, Name = "GDPR" }
        //     });

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<ContentRow>()))
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

        ContentRow contentRow = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<ContentRow>(x => AssertHelper.GetValueAndContinue(x, out contentRow))));

        Assert.Equal(idCampaign, contentRow.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, contentRow.Content);
        AssertHelper.EqualIgnoringSpaces(expectedHead, contentRow.Head);
    }

    [Theory]
    [InlineData("unlayer", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* , tenemos una oferta para vos</div>")]
    [InlineData("html", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* , tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[FIRST_NAME]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("html", "<div>Hola [[[first_name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[nombre]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("unlayer", "<div>Hola [[[first name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola  |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    [InlineData("html", "Hoy ([[[cumpleaños]]]) es tu cumpleaños", "Hoy (|*|323*|*) es tu cumpleaños")]
    public async Task PUT_campaign_should_remove_unknown_fieldIds(string type, string htmlInput, string expectedContent)
    {
        // Arrange
        var url = "/accounts/test1@test.com/campaigns/123/content";
        var token = TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518;
        var idCampaign = 123;

        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.IsAny<ByCampaignIdAndAccountNameParameters>()))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = true,
                EditorType = null,
            });

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<ContentRow>()))
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
            type = type,
            htmlContent = htmlInput,
            meta = "true" // it does not care
        }));

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ContentRow contentRow = null;
        dbContextMock.Verify(x => x.ExecuteAsync(
            It.IsAny<string>(),
            It.Is<ContentRow>(x => AssertHelper.GetValueAndContinue(x, out contentRow))));

        Assert.Equal(idCampaign, contentRow.IdCampaign);
        AssertHelper.EqualIgnoringSpaces(expectedContent, contentRow.Content);
    }

    [Theory]
    [InlineData(BODY_CONTENT, HEAD_CONTENT, BODY_CONTENT, null, null)]
    [InlineData(BODY_CONTENT, HEAD_CONTENT, BODY_CONTENT, META_CONTENT, 5)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, null, null)]
    [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, META_CONTENT, 5)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, null, null)]
    [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, META_CONTENT, 5)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, null)]
    [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, META_CONTENT, 5)]
    [InlineData(ORPHAN_DIV_CONTENT, HEAD_CONTENT, ORPHAN_DIV_CONTENT, null, null)]
    [InlineData(ORPHAN_DIV_CONTENT, HEAD_CONTENT, ORPHAN_DIV_CONTENT, META_CONTENT, 5)]
    public async Task GET_campaign_should_return_not_include_head_in_the_content(string expectedHtml, string storedHead, string storedContent, string storedMeta, int? editorType)
    {
        // Arrange
        var url = "/accounts/test1@test.com/campaigns/123/content";
        var token = TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518;
        var expectedAccountName = "test1@test.com";
        var idCampaign = 123;

        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.QueryFirstOrDefaultAsync<FirstOrDefaultContentWithCampaignStatusDbQuery.Result>(
                It.IsAny<string>(),
                It.Is<ByCampaignIdAndAccountNameParameters>(x =>
                    x.AccountName == expectedAccountName
                    && x.IdCampaign == idCampaign)))
            .ReturnsAsync(new FirstOrDefaultContentWithCampaignStatusDbQuery.Result()
            {
                IdCampaign = idCampaign,
                CampaignExists = true,
                CampaignHasContent = true,
                Content = storedContent,
                Head = storedHead,
                Meta = storedMeta,
                EditorType = editorType
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
        var response = await client.GetAsync(url);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);
        using var responseContentDoc = JsonDocument.Parse(responseContent);
        var responseContentJson = responseContentDoc.RootElement;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();
        Assert.Equal(expectedHtml, responseContentJson.GetProperty("htmlContent").GetString());
    }
}
