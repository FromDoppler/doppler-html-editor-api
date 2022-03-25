using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record UpdateCampaignContentDbQuery(
    int IdCampaign,
    int? EditorType,
    string Content,
    string Head,
    string Meta
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType
WHERE IdCampaign = @IdCampaign";
}
