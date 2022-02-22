
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class UpdateCampaignContentDbQuery : DbQuery<ContentRow, int>
{
    public UpdateCampaignContentDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
UPDATE Content
SET
    Content = @Content,
    Head = @Head,
    Meta = @Meta,
    EditorType = @EditorType
WHERE IdCampaign = @IdCampaign";

    public override Task<int> ExecuteAsync(ContentRow parameters)
        => DbContext.ExecuteAsync(SqlQuery, parameters);
}
