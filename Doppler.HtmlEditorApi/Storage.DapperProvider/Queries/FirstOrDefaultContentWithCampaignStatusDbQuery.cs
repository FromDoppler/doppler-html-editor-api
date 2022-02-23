using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class FirstOrDefaultContentWithCampaignStatusDbQuery : DbQuery<ByCampaignIdAndAccountNameParameters, FirstOrDefaultContentWithCampaignStatusDbQuery.Result>
{
    public FirstOrDefaultContentWithCampaignStatusDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
SELECT
    @IdCampaign as IdCampaign,
    CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignExists,
    CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignHasContent,
    co.EditorType,
    co.Content,
    co.Head,
    co.Meta
FROM [User] u
LEFT JOIN [Campaign] ca ON
    u.IdUser = ca.IdUser
    AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON
    ca.IdCampaign = co.IdCampaign
WHERE
    u.Email = @AccountName";

    public override Task<Result> ExecuteAsync(ByCampaignIdAndAccountNameParameters parameters)
        => DbContext.QueryFirstOrDefaultAsync<Result>(SqlQuery, parameters);

    public class Result
    {
        public int IdCampaign { get; init; }
        public bool CampaignExists { get; init; }
        public bool CampaignHasContent { get; init; }
        public int? EditorType { get; init; }
        public string Content { get; init; }
        public string Head { get; init; }
        public string Meta { get; init; }
    }
}
