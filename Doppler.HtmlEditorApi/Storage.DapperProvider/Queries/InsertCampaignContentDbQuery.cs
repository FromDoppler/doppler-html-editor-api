
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class InsertCampaignContentDbQuery : DbQuery<ContentRow, int>
{
    public InsertCampaignContentDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
INSERT INTO Content (
    IdCampaign,
    Content,
    Meta,
    EditorType
) VALUES (
    @IdCampaign,
    @Content,
    @Meta,
    @EditorType
)";

    public override Task<int> ExecuteAsync(ContentRow parameters)
        => DbContext.ExecuteAsync(SqlQuery, parameters);
}
