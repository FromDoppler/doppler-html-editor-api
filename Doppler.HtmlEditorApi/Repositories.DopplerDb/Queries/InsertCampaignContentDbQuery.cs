using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record InsertCampaignContentDbQuery(
    int IdCampaign,
    int? EditorType,
    string Content,
    string Head,
    string Meta,
    int? IdTemplate
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
INSERT INTO Content (
    IdCampaign,
    Content,
    Head,
    Meta,
    EditorType,
    IdTemplate
) VALUES (
    @IdCampaign,
    @Content,
    @Head,
    @Meta,
    @EditorType,
    @IdTemplate
)";
}
