using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class FirstOrDefaultCampaignStatusDbQuery : DbQuery<FirstOrDefaultCampaignStatusDbQuery.Parameters, FirstOrDefaultCampaignStatusDbQuery.Result>
{
    public FirstOrDefaultCampaignStatusDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
SELECT
    CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS OwnCampaignExists,
    CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS ContentExists,
    co.EditorType
FROM [User] u
LEFT JOIN [Campaign] ca ON
    u.IdUser = ca.IdUser
    AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON
    ca.IdCampaign = co.IdCampaign
WHERE u.Email = @accountName";

    public override Task<Result> ExecuteAsync(Parameters parameters)
        => DbContext.QueryFirstOrDefaultAsync<Result>(SqlQuery, parameters);

    public class Result
    {
        public bool OwnCampaignExists { get; init; }
        public bool ContentExists { get; init; }
        public int? EditorType { get; init; }
    }

    public class Parameters
    {
        public int IdCampaign { get; init; }
        public string AccountName { get; init; }
    }
}
