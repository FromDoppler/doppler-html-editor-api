using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdateCampaignContentDbQuery(
    int IdCampaign,
    int? EditorType,
    string Content,
    string Head,
    string Meta,
    int? IdTemplate
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @$"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType
    {(IdTemplate.HasValue ? ",IdTemplate = @IdTemplate" : "")}
WHERE IdCampaign = @IdCampaign";
}
