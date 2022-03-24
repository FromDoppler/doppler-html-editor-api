
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record InsertCampaignContentDbQuery(ContentRow contentRow) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
INSERT INTO Content (
    IdCampaign,
    Content,
    Head,
    Meta,
    EditorType
) VALUES (
    @IdCampaign,
    @Content,
    @Head,
    @Meta,
    @EditorType
)";

    public object GenerateSqlParameters()
        => contentRow;
}
