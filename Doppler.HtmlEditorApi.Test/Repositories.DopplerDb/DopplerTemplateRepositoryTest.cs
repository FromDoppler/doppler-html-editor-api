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

        var templateData = await repository.GetTemplate(It.IsAny<string>(), It.IsAny<int>());
        var unlayerTemplateData = Assert.IsType<UnlayerTemplateContentData>(templateData);
        Assert.NotNull(unlayerTemplateData.HtmlCode);
        Assert.NotNull(unlayerTemplateData.Meta);
        Assert.NotNull(unlayerTemplateData.PreviewImage);
        Assert.Equal(isPublicExpected, unlayerTemplateData.IsPublic);
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

        var templateData = await repository.GetTemplate(It.IsAny<string>(), It.IsAny<int>());
        var uknownTemplateData = Assert.IsType<UnknownTemplateContentData>(templateData);
        Assert.Equal(isPublicExpected, uknownTemplateData.IsPublic);
        Assert.Equal(_msEditorType, uknownTemplateData.EditorType);
        dbContextMock.VerifyAll();
    }
}
