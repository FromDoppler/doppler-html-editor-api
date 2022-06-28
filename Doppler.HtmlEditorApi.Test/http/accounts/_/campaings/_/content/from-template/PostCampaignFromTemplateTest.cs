using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

public class PostCampaignFromTemplateTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly ITestOutputHelper _output;
    public PostCampaignFromTemplateTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Theory]
    [InlineData("/accounts/x@x.com/campaigns/456/content/from-template/567", HttpStatusCode.Unauthorized)]
    public async Task POST_campaign_from_template_should_require_token(string url, HttpStatusCode expectedStatusCode)
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
    [InlineData("/accounts/x@x.com/campaigns/456/content/from-template/567", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    [InlineData("/accounts/x@x.com/campaigns/456/content/from-template/567", TestUsersData.TOKEN_EXPIRE_20330518, HttpStatusCode.Forbidden)]
    public async Task POST_campaign_from_template_should_not_accept_the_token_of_another_account(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567", TestUsersData.TOKEN_TEST1_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20010908, HttpStatusCode.Unauthorized)]
    public async Task POST_campaign_from_template_should_not_accept_a_expired_token(string url, string token, HttpStatusCode expectedStatusCode)
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
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567", TestUsersData.TOKEN_TEST1_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456, 567)]
    [InlineData($"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, TestUsersData.EMAIL_TEST1, 456, 567)]
    [InlineData("/accounts/otro@test.com/campaigns/456/content/from-template/567", TestUsersData.TOKEN_SUPERUSER_EXPIRE_20330518, "otro@test.com", 456, 567)]
    public async Task POST_campaign_from_template_should_accept_right_tokens_and_return_Ok(string url, string token, string expectedAccountName, int campaignId, int templateId)
    {
        // Arrange
        var repositoryMock = new Mock<ICampaignContentRepository>();
        repositoryMock
            .Setup(x => x.GetCampaignState(expectedAccountName, It.IsAny<int>()))
            .ReturnsAsync(new ClassicCampaignState(campaignId, true, null, CampaignStatus.Draft));
        repositoryMock
            .Setup(x => x.UpdateCampaignContent(campaignId, It.IsAny<BaseHtmlContentData>()))
            .Returns(Task.CompletedTask);

        var templateRepositoryMock = new Mock<ITemplateRepository>();
        templateRepositoryMock
            .Setup(x => x.GetTemplate(expectedAccountName, templateId))
            .ReturnsAsync(new UnlayerTemplateData("", "", "", 5, true));

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            Mock.Of<IFieldsRepository>(),
            templateRepositoryMock.Object,
            token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task POST_campaign_from_template_should_return_404_error_when_campaign_does_not_exist()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = false,
                ContentExists = false,
                EditorType = null,
                Status = null,
                TestType = null
            });

        var client = _factory.CreateSutClient(
            dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);

        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Fact]
    public async Task POST_campaign_from_template_should_return_404_error_when_user_does_not_exist()
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = false,
                ContentExists = false,
                EditorType = null,
                Status = null,
                TestType = null
            });

        var client = _factory.CreateSutClient(
            serviceToOverride1: dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData(CampaignStatus.Other)]
    public async Task POST_campaign_from_template_should_return_bad_request_error_when_campaign_is_not_writable(CampaignStatus campaignStatus)
    {
        // Arrange
        var repositoryMock = new Mock<ICampaignContentRepository>();
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var matchTitle = new Regex("\"title\"\\s*:\\s*\"The campaign content is read only\"");
        var matchDetail = new Regex("\"detail\"\\s*:\\s*\"The content cannot be edited because status campaign is Other\"");

        repositoryMock
            .Setup(x => x.GetCampaignState(expectedAccountName, It.IsAny<int>()))
            .ReturnsAsync(new ClassicCampaignState(456, true, null, campaignStatus));

        var client = _factory.CreateSutClient(
            repositoryMock.Object,
            Mock.Of<IFieldsRepository>(),
            token);

        // Act
        var response = await client.PostAsync(url, null);

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
    public async Task POST_campaign_from_template_should_clone_template_data(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
    {
        // Arrange
        var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
        var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
        var expectedAccountName = TestUsersData.EMAIL_TEST1;
        var expectedIdCampaign = 456;
        var expectedIdTemplate = 567;
        var htmlContent = "My HTML Content";
        var meta = "{}";
        var previewImage = "https://previewImage";

        var dbContextMock = new Mock<IDbContext>();

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
            .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
            {
                OwnCampaignExists = true,
                ContentExists = contentExists,
                EditorType = currentEditorType,
                Status = 1,
                TestType = null
            });

        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new GetTemplateByIdWithStatusDbQuery(expectedIdTemplate, expectedAccountName)
            ))
            .ReturnsAsync(new GetTemplateByIdWithStatusDbQuery.Result()
            {
                // TODO: test a different value
                IsPublic = false,
                // TODO: test a different value
                EditorType = 5,
                HtmlCode = htmlContent,
                Meta = meta,
                PreviewImage = previewImage
            });

        dbContextMock
            .SetupInsertOrUpdateContentRow(
                sqlQueryStartsWith,
                expectedIdCampaign,
                htmlContent,
                meta,
                result: 1);

        var client = _factory.CreateSutClient(
            dbContextMock.Object,
            token: token);

        // Act
        var response = await client.PostAsync(url, null);
        _output.WriteLine(response.GetHeadersAsString());
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine(responseContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dbContextMock.VerifyAll();

        // TODO: Test UpdateCampaignStatus
        // TODO: Test UpdateCampaignPreviewImage
        // TODO: Test SaveNewFieldIds
        // TODO: Test SaveLinks
        // TODO: TestAB
    }

    // [Theory]
    // [InlineData(null, true, "UPDATE")]
    // [InlineData(55, true, "UPDATE")]
    // [InlineData(4, true, "UPDATE")]
    // [InlineData(5, true, "UPDATE")]
    // [InlineData(null, false, "INSERT")]
    // public async Task POST_campaign_from_template_should_store_html_content_when_campaign_is_TestAB(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 123;
    //     var idCampaignResult = 567;
    //     var htmlContent = "My HTML Content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = contentExists,
    //             EditorType = currentEditorType,
    //             Status = 1,
    //             TestType = 1,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });

    //     dbContextMock
    //         .SetupInsertOrUpdateContentRow(
    //             sqlQueryStartsWith,
    //             expectedIdCampaign,
    //             htmlContent,
    //             meta: null,
    //             result: 1);

    //     dbContextMock
    //         .SetupInsertOrUpdateContentRow(
    //             sqlQueryStartsWith,
    //             idCampaignB,
    //             htmlContent,
    //             meta: null,
    //             result: 1);

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    // }

    // [Fact]
    // public async Task POST_campaign_from_template_should_update_campaign_status_when_campaign_is_TestAB_and_content_not_exist()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 123;
    //     var idCampaignResult = 567;
    //     // var htmlContent = "My HTML Content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = false,
    //             EditorType = null,
    //             Status = 1,
    //             TestType = 1,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });


    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<UpdateCampaignStatusDbQuery>(q =>
    //             q.SetCurrentStep == 2 &&
    //             q.SetHtmlSourceType == 2 &&
    //             q.WhenIdCampaignIs == expectedIdCampaign &&
    //             q.WhenCurrentStepIs == 1
    //         )));

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<UpdateCampaignStatusDbQuery>(q =>
    //             q.SetCurrentStep == 2 &&
    //             q.SetHtmlSourceType == 2 &&
    //             q.WhenIdCampaignIs == idCampaignB &&
    //             q.WhenCurrentStepIs == 1
    //         )));

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<UpdateCampaignStatusDbQuery>(q =>
    //             q.SetCurrentStep == 2 &&
    //             q.SetHtmlSourceType == 2 &&
    //             q.WhenIdCampaignIs == idCampaignResult &&
    //             q.WhenCurrentStepIs == 1
    //         )));
    // }

    // [Fact]
    // public async Task POST_campaign_from_template_should_not_update_campaign_status_when_campaign_is_TestAB_and_content_exists()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 123;
    //     var idCampaignResult = 567;
    //     // var htmlContent = "My HTML Content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 4,
    //             Status = 1,
    //             TestType = 1,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });


    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.IsAny<UpdateCampaignStatusDbQuery>()), Times.Never);
    // }

    // DUPLICADO< TEST DE STATUS NO ESTÁ EN EL OTRO
    // [Fact]
    // public async Task POST_campaign_from_template_should_store_unlayer_content_and_ensure_campaign_status_when_content_no_exist()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var htmlContent = "My HTML Content";
    //     var metaAsString = "{\"data\":\"My Meta Content\"}";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = false,
    //             EditorType = null,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock
    //         .SetupInsertOrUpdateContentRow(
    //             "INSERT",
    //             expectedIdCampaign,
    //             htmlContent,
    //             metaAsString,
    //             result: 1);

    //     var setCurrentStep = 2;
    //     var setHtmlSourceType = 2;
    //     var whenCurrentStepIs = 1;
    //     var whenIdCampaignIs = expectedIdCampaign;
    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new UpdateCampaignStatusDbQuery(
    //                 setCurrentStep,
    //                 setHtmlSourceType,
    //                 whenIdCampaignIs,
    //                 whenCurrentStepIs)))
    //         .ReturnsAsync(1);

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    // }

//     [Theory]
//     // Insert HTML Content
//     [InlineData(
//         "https://1.fromdoppler.net/image1.png",
//         false,
//         @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content"",
//     ""previewImage"": ""https://1.fromdoppler.net/image1.png""
// }")]
//     // Update HTML Content
//     [InlineData(
//         "https://2.fromdoppler.net/image2.png",
//         true,
//         @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content"",
//     ""previewImage"": ""https://2.fromdoppler.net/image2.png""
// }")]
//     // Insert Unlayer Content
//     [InlineData(
//         "https://3.fromdoppler.net/image3.png",
//         false,
//         @"{
//     ""type"": ""unlayer"",
//     ""htmlContent"": ""My HTML Content"",
//     ""meta"": ""{}"",
//     ""previewImage"": ""https://3.fromdoppler.net/image3.png""
// }")]
//     // Update Unlayer Content
//     [InlineData(
//         "https://4.fromdoppler.net/image4.png",
//         true,
//         @"{
//     ""type"": ""unlayer"",
//     ""htmlContent"": ""My HTML Content"",
//     ""meta"": ""{}"",
//     ""previewImage"": ""https://4.fromdoppler.net/image4.png""
// }")]
//     [InlineData(
//         null,
//         false,
//         @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content""
// }")]
//     public async Task POST_campaign_from_template_should_store_previewImage(string expectedPreviewImage, bool contentExists, string requestBody)
//     {
//         // Arrange
//         var idCampaign = 456;
//         var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/{idCampaign}/content";
//         var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;

//         var dbContextMock = new Mock<IDbContext>();

//         dbContextMock
//             .Setup(x => x.ExecuteAsync(
//                 new FirstOrDefaultCampaignStatusDbQuery(idCampaign, TestUsersData.EMAIL_TEST1)))
//             .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
//             {
//                 OwnCampaignExists = true,
//                 ContentExists = contentExists,
//                 EditorType = null,
//                 Status = 1,
//                 TestType = null
//             });

//         var client = _factory.CreateSutClient(
//             serviceToOverride1: dbContextMock.Object,
//             token: token);

//         // Act
//         var response = await client.PostAsync(url, null);
//         _output.WriteLine(response.GetHeadersAsString());
//         var responseContent = await response.Content.ReadAsStringAsync();
//         _output.WriteLine(responseContent);

//         // Assert
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//         dbContextMock.Verify(x => x.ExecuteAsync(It.Is<UpdateCampaignPreviewImageDbQuery>(q =>
//             q.SqlQueryContains("PreviewImage = @PreviewImage")
//             && q.SqlQueryContains("WHERE IdCampaign = @IdCampaign")
//             && q.SqlParametersContain("PreviewImage", expectedPreviewImage)
//             && q.SqlParametersContain("IdCampaign", idCampaign))));
//     }

//     [Theory]
//     // Insert HTML Content
//     [InlineData(
//     "https://1.fromdoppler.net/image1.png",
//     false,
//     @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content"",
//     ""previewImage"": ""https://1.fromdoppler.net/image1.png""
// }")]
//     // Update HTML Content
//     [InlineData(
//     "https://2.fromdoppler.net/image2.png",
//     true,
//     @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content"",
//     ""previewImage"": ""https://2.fromdoppler.net/image2.png""
// }")]
//     // Insert Unlayer Content
//     [InlineData(
//     "https://3.fromdoppler.net/image3.png",
//     false,
//     @"{
//     ""type"": ""unlayer"",
//     ""htmlContent"": ""My HTML Content"",
//     ""meta"": ""{}"",
//     ""previewImage"": ""https://3.fromdoppler.net/image3.png""
// }")]
//     // Update Unlayer Content
//     [InlineData(
//     "https://4.fromdoppler.net/image4.png",
//     true,
//     @"{
//     ""type"": ""unlayer"",
//     ""htmlContent"": ""My HTML Content"",
//     ""meta"": ""{}"",
//     ""previewImage"": ""https://4.fromdoppler.net/image4.png""
// }")]
//     [InlineData(
//     null,
//     false,
//     @"{
//     ""type"": ""html"",
//     ""htmlContent"": ""My HTML Content""
// }")]
//     public async Task POST_campaign_from_template_should_store_previewImage_in_campaign_TestAB(string expectedPreviewImage, bool contentExists, string requestBody)
//     {
//         // Arrange
//         var idCampaign = 456;
//         var idCampaignB = 789;
//         var idCampaignResult = 321;
//         var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/{idCampaign}/content";
//         var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;

//         var dbContextMock = new Mock<IDbContext>();

//         dbContextMock
//             .Setup(x => x.ExecuteAsync(
//                 new FirstOrDefaultCampaignStatusDbQuery(idCampaign, TestUsersData.EMAIL_TEST1)))
//             .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
//             {
//                 OwnCampaignExists = true,
//                 ContentExists = contentExists,
//                 EditorType = null,
//                 Status = 1,
//                 TestType = 1,
//                 IdCampaignA = idCampaign,
//                 IdCampaignB = idCampaignB,
//                 IdCampaignResult = idCampaignResult
//             });

//         var client = _factory.CreateSutClient(
//             serviceToOverride1: dbContextMock.Object,
//             token: token);

//         // Act
//         var response = await client.PostAsync(url, null);
//         _output.WriteLine(response.GetHeadersAsString());
//         var responseContent = await response.Content.ReadAsStringAsync();
//         _output.WriteLine(responseContent);

//         // Assert
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//         dbContextMock.Verify(x => x.ExecuteAsync(It.Is<UpdateCampaignPreviewImageDbQuery>(q =>
//             q.SqlQueryContains("PreviewImage = @PreviewImage")
//             && q.SqlQueryContains("WHERE IdCampaign = @IdCampaign")
//             && q.SqlParametersContain("PreviewImage", expectedPreviewImage)
//             && q.SqlParametersContain("IdCampaign", idCampaign))));

//         dbContextMock.Verify(x => x.ExecuteAsync(It.Is<UpdateCampaignPreviewImageDbQuery>(q =>
//             q.SqlQueryContains("PreviewImage = @PreviewImage")
//             && q.SqlQueryContains("WHERE IdCampaign = @IdCampaign")
//             && q.SqlParametersContain("PreviewImage", expectedPreviewImage)
//             && q.SqlParametersContain("IdCampaign", idCampaignB))));
//     }


//DUPLICADO PORQUE UNLAYER Y HTML VAN JUNTOS?
    // [Theory]
    // [InlineData(null, true, "UPDATE")]
    // [InlineData(55, true, "UPDATE")]
    // [InlineData(4, true, "UPDATE")]
    // [InlineData(5, true, "UPDATE")]
    // public async Task POST_campaign_from_template_should_store_unlayer_content_and_ensure_campaign_status(int? currentEditorType, bool contentExists, string sqlQueryStartsWith)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var htmlContent = "My HTML Content";
    //     var metaAsString = "{\"data\":\"My Meta Content\"}";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = contentExists,
    //             EditorType = currentEditorType,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock
    //         .SetupInsertOrUpdateContentRow(
    //             sqlQueryStartsWith,
    //             expectedIdCampaign,
    //             htmlContent,
    //             metaAsString,
    //             result: 1);

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    // }

    // [Theory]
    // [InlineData("[[[firstname]]]", "VALUES (319)")]
    // [InlineData("[[[firstname]]] [[[lastname]]]", "VALUES (319),(320)")]
    // [InlineData("[[[firstname]]] [[[lastname]]] [[[noexist]]]", "VALUES (319),(320)")]
    // public async Task POST_campaign_from_template_should_store_field_relations(string htmlContent, string expectedSubQuery)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<SaveNewCampaignFields>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && q.SqlQueryContains(expectedSubQuery))
    //     ), Times.Once);
    // }

    // [Theory]
    // [InlineData("[[[firstname]]]", "VALUES (319)")]
    // [InlineData("[[[firstname]]] [[[lastname]]]", "VALUES (319),(320)")]
    // [InlineData("[[[firstname]]] [[[lastname]]] [[[noexist]]]", "VALUES (319),(320)")]
    // public async Task POST_campaign_from_template_should_store_field_relations_in_campaign_TestAB(string htmlContent, string expectedSubQuery)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 789;
    //     var idCampaignResult = 321;

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = 1,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<SaveNewCampaignFields>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && q.SqlQueryContains(expectedSubQuery))
    //     ), Times.Once);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<SaveNewCampaignFields>(q =>
    //             q.IdContent == idCampaignB
    //             && q.SqlQueryContains(expectedSubQuery))
    //     ), Times.Once);
    // }

// HACER
    // [Fact]
    // public async Task POST_campaign_from_template_should_no_store_field_relations_when_no_fields()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     // var htmlContent = "<html>No content</html>";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();
    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.IsAny<SaveNewCampaignFields>()
    //     ), Times.Never);
    // }

    // [Theory]
    // [InlineData(
    //     $@"<a href=""https://www.google.com"">Google</a><br>
    //         <a href=""{"\n"} https://{"\t"}goo gle1{"\n"}.com    {"\r\n"}  "">Google1 (dirty)</a><br>
    //         <a href=""https://google2.com"">Google2</a><br>
    //         <a href=""{"\n"} https://{"\t"}goo gle2{"\n"}.com    {"\r\n"}  "">Google2 (dirty)</a><br>",
    //     new[] { "https://www.google.com", "https://google1.com", "https://google2.com" })]
    // [InlineData(
    //     @"<a href=""https://www.google.com?q=[[[firstname]]]"">Find my name!</a><br>
    //         <a href=""https://www.google.com?q=[[[apellido]]]"">Find my lastname</a><br>
    //         <a href=""https://www.google.com?q=[[[nombre]]]"">Find my name again!</a>",
    //     new[] { "https://www.google.com?q=|*|319*|*", "https://www.google.com?q=|*|320*|*" })]
    // public async Task POST_campaign_from_template_should_add_and_remove_link_relations(string htmlContent, string[] expectedLinks)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = null,
    //             TestABCategory = null,
    //             IdCampaignA = 456,
    //             IdCampaignB = null,
    //             IdCampaignResult = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dbContextMock.VerifyLinksSendToSaveNewCampaignLinks(expectedIdCampaign, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks(expectedIdCampaign, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteRemovedCampaignLinks(expectedIdCampaign, expectedLinks);
    // }

    // [Theory]
    // [InlineData(
    //     $@"<a href=""https://www.google.com"">Google</a><br>
    //         <a href=""{"\n"} https://{"\t"}goo gle1{"\n"}.com    {"\r\n"}  "">Google1 (dirty)</a><br>
    //         <a href=""https://google2.com"">Google2</a><br>
    //         <a href=""{"\n"} https://{"\t"}goo gle2{"\n"}.com    {"\r\n"}  "">Google2 (dirty)</a><br>",
    //     new[] { "https://www.google.com", "https://google1.com", "https://google2.com" })]
    // [InlineData(
    //     @"<a href=""https://www.google.com?q=[[[firstname]]]"">Find my name!</a><br>
    //         <a href=""https://www.google.com?q=[[[apellido]]]"">Find my lastname</a><br>
    //         <a href=""https://www.google.com?q=[[[nombre]]]"">Find my name again!</a>",
    //     new[] { "https://www.google.com?q=|*|319*|*", "https://www.google.com?q=|*|320*|*" })]
    // public async Task POST_campaign_from_template_should_add_and_remove_link_relations_in_campaign_TestAB(string htmlContent, string[] expectedLinks)
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 789;
    //     var idCampaignResult = 321;

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = 1,
    //             TestABCategory = null,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dbContextMock.VerifyLinksSendToSaveNewCampaignLinks(expectedIdCampaign, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks(expectedIdCampaign, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteRemovedCampaignLinks(expectedIdCampaign, expectedLinks);

    //     dbContextMock.VerifyLinksSendToSaveNewCampaignLinks(idCampaignB, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteAutomationConditionalsOfRemovedCampaignLinks(idCampaignB, expectedLinks);

    //     dbContextMock.VerifyLinksSendToDeleteRemovedCampaignLinks(idCampaignB, expectedLinks);

    // }

// HACER
    // [Fact]
    // public async Task POST_campaign_from_template_should_no_add_links_relations_when_no_links()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     // var htmlContent = "<html>No content</html>";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.IsAny<SaveNewCampaignLinks>()
    //     ), Times.Never);
    // }

// HACER
    // [Fact]
    // public async Task POST_campaign_from_template_should_remove_links_relations_when_no_links()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     // var htmlContent = "<html>No content</html>";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && !q.Links.Any())
    //     ), Times.Once);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteRemovedCampaignLinks>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && !q.Links.Any())
    //     ), Times.Once);
    // }

// HACER
    // [Fact]
    // public async Task POST_campaign_from_template_should_remove_links_relations_when_no_links_in_campaign_TestAB()
    // {
    //     // Arrange
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/456/content/from-template/567";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     var expectedIdCampaign = 456;
    //     var idCampaignB = 789;
    //     var idCampaignResult = 321;
    //     // var htmlContent = "<html>No content</html>";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(expectedIdCampaign, expectedAccountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = 5,
    //             Status = 1,
    //             TestType = 1,
    //             IdCampaignA = expectedIdCampaign,
    //             IdCampaignB = idCampaignB,
    //             IdCampaignResult = idCampaignResult
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);
    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && !q.Links.Any())
    //     ), Times.Once);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteRemovedCampaignLinks>(q =>
    //             q.IdContent == expectedIdCampaign
    //             && !q.Links.Any())
    //     ), Times.Once);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteAutomationConditionalsOfRemovedCampaignLinks>(q =>
    //             q.IdContent == idCampaignB
    //             && !q.Links.Any())
    //     ), Times.Once);

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<DeleteRemovedCampaignLinks>(q =>
    //             q.IdContent == idCampaignB
    //             && !q.Links.Any())
    //     ), Times.Once);
    // }

    // [Theory]
    // [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(HTML_WITH_HEAD_AND_BODY, HEAD_CONTENT, BODY_CONTENT, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(ORPHAN_DIV_CONTENT, null, ORPHAN_DIV_CONTENT, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(ONLY_HEAD, HEAD_CONTENT, "<BR>", "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD, null, HTML_WITHOUT_HEAD, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, null, HTML_WITHOUT_HEAD_WITH_ORPHAN_DIV, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", true, typeof(UpdateCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "html", false, typeof(InsertCampaignContentDbQuery))]
    // [InlineData(HTML_WITHOUT_BODY_WITH_ORPHAN_DIV, HEAD_CONTENT, HTML_WITHOUT_BODY_WITH_ORPHAN_DIV_WITHOUT_HEAD, "unlayer", true, typeof(UpdateCampaignContentDbQuery))]
    // public async Task POST_campaign_from_template_should_split_html_in_head_and_content(string htmlInput, string expectedHead, string expectedContent, string type, bool existingContent, Type queryType)
    // {
    //     // Arrange
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var accountName = TestUsersData.EMAIL_TEST1;
    //     var idCampaign = 456;
    //     var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = existingContent,
    //             EditorType = null,
    //             Status = 1,
    //             TestType = null
    //         });

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);

    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     dbContextMock.VerifyAll();

    //     dynamic dbQuery = null;

    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<IExecutableDbQuery>(q =>
    //         q.GetType() == queryType
    //         && AssertHelper.GetDynamicValueAndContinue(q, out dbQuery))));

    //     Assert.Equal(idCampaign, dbQuery.IdCampaign);
    //     AssertHelper.EqualIgnoringMeaninglessSpaces(expectedContent, dbQuery.Content);
    //     AssertHelper.EqualIgnoringMeaninglessSpaces(expectedHead, dbQuery.Head);
    // }

    // [Theory]
    // [InlineData("unlayer", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola |*|319*|* , tenemos una oferta para vos</div>")]
    // [InlineData("html", "<div>Hola |*|319*|* |*|98765*|*, tenemos una oferta para vos</div>", "<div>Hola |*|319*|* , tenemos una oferta para vos</div>")]
    // [InlineData("unlayer", "<div>Hola [[[FIRST_NAME]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    // [InlineData("html", "<div>Hola [[[first_name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    // [InlineData("unlayer", "<div>Hola [[[nombre]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    // [InlineData("unlayer", "<div>Hola [[[first name]]] [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>", "<div>Hola |*|319*|* [[[UNKNOWN_FIELD]]], tenemos una oferta para vos</div>")]
    // [InlineData("html", "Hoy ([[[cumpleaños]]]) es tu cumpleaños", "Hoy (|*|323*|*) es tu cumpleaños")]
    // [InlineData(
    //     "unlayer",
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first name]]]|*|12345678*|*\">[[[first name]]]</a> [[[cumpleaños]]]</b></p>",
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    // [InlineData(
    //     "unlayer",
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first%20name]]]%20[[[cumplea&#241;os]]]\">[[[first%20name]]]</a> [[[cumplea&ntilde;os]]]</b></p>",
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*%20|*|323*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    // [InlineData("html", "[[[custom1]]] [[[Custom2]]] [[[UNKNOWN_FIELD]]]", "|*|12345*|* |*|456789*|* [[[UNKNOWN_FIELD]]]")]
    // public async Task POST_campaign_from_template_should_replace_fields_and_remove_unknown_fieldIds(string type, string htmlInput, string expectedContent)
    // {
    //     // Arrange
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var accountName = TestUsersData.EMAIL_TEST1;
    //     var idCampaign = 456;
    //     var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = null,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();
    //     dbContextMock.SetupCustomFields(
    //         expectedAccountName: accountName,
    //         result: new DbField[] {
    //             new()
    //             {
    //                 IdField = 12345,
    //                 IsBasicField = false,
    //                 Name = "CUSTOM1"
    //             },
    //             new()
    //             {
    //                 IdField = 456789,
    //                 IsBasicField = false,
    //                 Name = "custom2"
    //             }
    //         });

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);

    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    //     UpdateCampaignContentDbQuery dbQuery = null;
    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<UpdateCampaignContentDbQuery>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));

    //     Assert.Equal(idCampaign, dbQuery.IdCampaign);
    //     AssertHelper.EqualIgnoringMeaninglessSpaces(expectedContent, dbQuery.Content);
    // }

    // [Theory]
    // [InlineData(
    //     1,
    //     "html",
    //     // Sanitization not required
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=[[[first name]]]|*|12345678*|*\">[[[first name]]]</a> [[[cumpleaños]]]</b></p>",
    //     "<p>Hola <b><a href=\"https://www.google.com/search?q=|*|319*|*\">|*|319*|*</a> |*|323*|*</b></p>")]
    // [InlineData(
    //     2,
    //     "unlayer",
    //     "<a href=\"https://\tgoo gle1\n.com    \r\n  \">Link</a>",
    //     "<a href=\"https://google1.com\">Link</a>")]
    // public async Task POST_campaign_from_template_should_sanitize_links(int idCampaign, string type, string htmlInput, string expectedContent)
    // {
    //     // Arrange
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var accountName = TestUsersData.EMAIL_TEST1;
    //     var url = $"/accounts/{accountName}/campaigns/{idCampaign}/content";

    //     var dbContextMock = new Mock<IDbContext>();

    //     dbContextMock
    //         .Setup(x => x.ExecuteAsync(
    //             new FirstOrDefaultCampaignStatusDbQuery(idCampaign, accountName)))
    //         .ReturnsAsync(new FirstOrDefaultCampaignStatusDbQuery.Result()
    //         {
    //             OwnCampaignExists = true,
    //             ContentExists = true,
    //             EditorType = null,
    //             Status = 1,
    //             TestType = null
    //         });

    //     dbContextMock.SetupBasicFields();

    //     var client = _factory.CreateSutClient(
    //         serviceToOverride1: dbContextMock.Object,
    //         token: token);

    //     // Act
    //     var response = await client.PostAsync(url, null);

    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    //     UpdateCampaignContentDbQuery dbQuery = null;
    //     dbContextMock.Verify(x => x.ExecuteAsync(
    //         It.Is<UpdateCampaignContentDbQuery>(q => AssertHelper.GetValueAndContinue(q, out dbQuery))));

    //     Assert.Equal(idCampaign, dbQuery.IdCampaign);
    //     Assert.Equal(expectedContent, dbQuery.Content);
    // }

    // [Fact]
    // public async Task POST_campaign_from_template_should_return_bad_request_error_when_campaign_is_testAB_by_content()
    // {
    //     // Arrange
    //     var repositoryMock = new Mock<ICampaignContentRepository>();
    //     var campaignId = 456;
    //     var campaignIdA = 456;
    //     var campaignIdB = 789;
    //     var campaignIdResult = 123;
    //     var url = $"/accounts/{TestUsersData.EMAIL_TEST1}/campaigns/{campaignId}/content";
    //     var token = TestUsersData.TOKEN_TEST1_EXPIRE_20330518;
    //     var expectedAccountName = TestUsersData.EMAIL_TEST1;
    //     // var htmlContent = "My HTML Content";
    //     var matchTitle = new Regex("\"title\"\\s*:\\s*\"The campaign is AB Test by content\"");
    //     var matchDetail = new Regex($"\"detail\"\\s*:\\s*\"The type of campaign is AB Test by content and it's unsupported\"");

    //     repositoryMock
    //         .Setup(x => x.GetCampaignState(expectedAccountName, campaignId))
    //         .ReturnsAsync(new TestABCampaignState(true, null, CampaignStatus.Draft, TestABCondition.TypeTestABContent, campaignIdA, campaignIdB, campaignIdResult));

    //     var client = _factory.CreateSutClient(
    //         repositoryMock.Object,
    //         Mock.Of<IFieldsRepository>(),
    //         token);

    //     // Act
    //     var response = await client.PostAsync(url, null);

    //     _output.WriteLine(response.GetHeadersAsString());
    //     var responseContent = await response.Content.ReadAsStringAsync();
    //     _output.WriteLine(responseContent);

    //     // Assert
    //     Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    //     Assert.Matches(matchTitle, responseContent);
    //     Assert.Matches(matchDetail, responseContent);
    //     repositoryMock.VerifyAll();
    //     repositoryMock.VerifyNoOtherCalls();
    // }
}
