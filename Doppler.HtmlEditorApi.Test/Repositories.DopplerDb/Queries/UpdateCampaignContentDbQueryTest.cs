using Xunit;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public class UpdateCampaignContentDbQueryTest
{
    [Fact]
    public void UpdateCampaignContentQuery_should_update_when_idTemplate_has_value()
    {
        // Arrange
        var idCampaign = 567;
        var editorType = 5;
        var content = "<html></html>";
        var head = "<head></head>";
        var meta = "{}";
        var idTemplate = 123;

        var dbQuery = new UpdateCampaignContentDbQuery(
            IdCampaign: idCampaign,
            EditorType: editorType,
            Content: content,
            Head: head,
            Meta: meta,
            IdTemplate: idTemplate
        );

        var expectedQuery = @$"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType
,IdTemplate = @IdTemplate
WHERE IdCampaign = @IdCampaign";

        // Act
        var sqlQuery = dbQuery.GenerateSqlQuery();

        // Assert
        Assert.Equal(expectedQuery, sqlQuery);
    }

    [Fact]
    public void UpdateCampaignContentQuery_should_update_when_idTemplate_is_null()
    {
        // Arrange
        var idCampaign = 567;
        var editorType = 5;
        var content = "<html></html>";
        var head = "<head></head>";
        var meta = "{}";

        var dbQuery = new UpdateCampaignContentDbQuery(
            IdCampaign: idCampaign,
            EditorType: editorType,
            Content: content,
            Head: head,
            Meta: meta,
            IdTemplate: null
        );

        var expectedQuery = @$"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType

WHERE IdCampaign = @IdCampaign";

        // Act
        var sqlQuery = dbQuery.GenerateSqlQuery();

        // Assert
        Assert.Equal(expectedQuery, sqlQuery);
    }
}
