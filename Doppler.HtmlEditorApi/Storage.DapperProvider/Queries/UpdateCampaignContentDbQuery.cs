using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record UpdateCampaignContentDbQuery(ContentRow contentRow) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType
WHERE IdCampaign = @IdCampaign";

    public object GenerateSqlParameters()
        => contentRow;
}
