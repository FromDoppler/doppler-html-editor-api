using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;
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

        var templateContentData = await repository.GetTemplate(It.IsAny<string>(), It.IsAny<int>());
        var unlayerTemplateContentData = Assert.IsType<UnlayerTemplateContentData>(templateContentData);
        Assert.NotNull(unlayerTemplateContentData.HtmlComplete);
        Assert.NotNull(unlayerTemplateContentData.Meta);
        Assert.NotNull(unlayerTemplateContentData.PreviewImage);
        Assert.Equal(isPublicExpected, unlayerTemplateContentData.IsPublic);
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

        var templateContentData = await repository.GetTemplate(It.IsAny<string>(), It.IsAny<int>());
        var uknownTemplateContentData = Assert.IsType<UnknownTemplateContentData>(templateContentData);
        Assert.Equal(isPublicExpected, uknownTemplateContentData.IsPublic);
        Assert.Equal(_msEditorType, uknownTemplateContentData.EditorType);
        dbContextMock.VerifyAll();
    }
}
