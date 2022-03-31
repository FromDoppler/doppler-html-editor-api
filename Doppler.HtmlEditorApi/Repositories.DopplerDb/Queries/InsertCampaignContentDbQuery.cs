
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record InsertCampaignContentDbQuery(
    int IdCampaign,
    int? EditorType,
    string Content,
    string Head,
    string Meta
) : IExecutableDbQuery
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
}
