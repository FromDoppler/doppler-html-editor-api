using System;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
using Doppler.HtmlEditorApi.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DopplerTemplateRepositoryTest : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly int _unlayerEditorType = 5;
    private readonly int _msEditorType = 4;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void GetTemplate_unlayer_template(bool isPublicExpected)
    {
        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new GetTemplateByIdWithStatusDbQuery(It.IsAny<int>(), It.IsAny<string>()))
                )
            .ReturnsAsync(new GetTemplateByIdWithStatusDbQuery.Result()
            {
                IsPublic = isPublicExpected,
                EditorType = _unlayerEditorType,
                HtmlCode = "",
                Meta = "",
                PreviewImage = ""
            });
        var repository = new DopplerTemplateRepository(dbContextMock.Object);

        var templateModel = await repository.GetOwnOrPublicTemplate(It.IsAny<string>(), It.IsAny<int>());
        var unlayerTemplateContentData = Assert.IsType<UnlayerTemplateContentData>(templateModel.Content);
        Assert.NotNull(unlayerTemplateContentData.HtmlComplete);
        Assert.NotNull(unlayerTemplateContentData.Meta);
        Assert.NotNull(templateModel.PreviewImage);
        Assert.Equal(isPublicExpected, templateModel.IsPublic);
        dbContextMock.VerifyAll();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void Get_unknown_template(bool isPublicExpected)
    {
        var dbContextMock = new Mock<IDbContext>();
        dbContextMock
            .Setup(x => x.ExecuteAsync(
                new GetTemplateByIdWithStatusDbQuery(It.IsAny<int>(), It.IsAny<string>()))
                )
            .ReturnsAsync(new GetTemplateByIdWithStatusDbQuery.Result()
            {
                IsPublic = isPublicExpected,
                EditorType = _msEditorType,
                HtmlCode = "",
                Meta = "",
                PreviewImage = ""
            });
        var repository = new DopplerTemplateRepository(dbContextMock.Object);

        var templateModel = await repository.GetOwnOrPublicTemplate(It.IsAny<string>(), It.IsAny<int>());
        var unknownTemplateContentData = Assert.IsType<UnknownTemplateContentData>(templateModel.Content);
        Assert.Equal(isPublicExpected, templateModel.IsPublic);
        Assert.Equal(_msEditorType, unknownTemplateContentData.EditorType);
        dbContextMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateTemplate_should_throw_when_content_is_not_UnlayerTemplateContentData()
    {
        // Arrange
        var otherTemplateContentData = Mock.Of<TemplateContentData>();
        var dbContext = Mock.Of<IDbContext>();
        var sut = new DopplerTemplateRepository(dbContext);
        var templateId = 123;
        var templateModel = new TemplateModel(
            TemplateId: templateId,
            IsPublic: false,
            PreviewImage: "NEW PREVIEW IMAGE",
            Name: "NEW NAME",
            Content: otherTemplateContentData);


        // Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(async () =>
        {
            // Act
            await sut.UpdateTemplate(templateModel);
        });
        Assert.StartsWith("Unsupported template content type", exception.Message);
    }

    [Fact]
    public async Task UpdateTemplate_should_execute_the_right_query_and_parameters()
    {
        // Arrange
        var dbContextMock = new Mock<IDbContext>();
        var sut = new DopplerTemplateRepository(dbContextMock.Object);
        var templateId = 123;
        var previewImage = "NEW PREVIEW IMAGE";
        var name = "NEW NAME";
        var htmlComplete = "NEW HTML CONTENT";
        var meta = "{\"test\":\"NEW META\"}";
        var templateModel = new TemplateModel(
            TemplateId: templateId,
            IsPublic: false,
            PreviewImage: previewImage,
            Name: name,
            Content: new UnlayerTemplateContentData(
                HtmlComplete: htmlComplete,
                Meta: meta));

        // Act
        await sut.UpdateTemplate(templateModel);

        // Assert
        var dbQuery = dbContextMock.VerifyAndGetExecutableDbQuery();
        dbQuery.VerifySqlParametersContain("IdTemplate", templateId);
        dbQuery.VerifySqlParametersContain("EditorType", 5);
        dbQuery.VerifySqlParametersContain("HtmlCode", htmlComplete);
        dbQuery.VerifySqlParametersContain("Meta", meta);
        dbQuery.VerifySqlParametersContain("PreviewImage", previewImage);
        dbQuery.VerifySqlParametersContain("Name", name);
        dbQuery.VerifySqlQueryContains("UPDATE Template");
        dbQuery.VerifySqlQueryContains("WHERE IdTemplate = @IdTemplate");
        dbQuery.VerifySqlQueryContains("EditorType = @EditorType");
        dbQuery.VerifySqlQueryContains("HtmlCode = @HtmlCode");
        dbQuery.VerifySqlQueryContains("Meta = @Meta");
        dbQuery.VerifySqlQueryContains("PreviewImage = @PreviewImage");
        dbQuery.VerifySqlQueryContains("Name = @Name");
    }
}
