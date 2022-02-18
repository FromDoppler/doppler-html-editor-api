using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public static class LoadCampaignQuery
{
    private const string QUERY = @"
SELECT
    @IdCampaign as IdCampaign,
    CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignExists,
    CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignHasContent,
    co.EditorType,
    co.Content,
    co.Meta
FROM [User] u
LEFT JOIN [Campaign] ca ON
    u.IdUser = ca.IdUser
    AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON
    ca.IdCampaign = co.IdCampaign
WHERE
    u.Email = @AccountName";

    public static Task<Result> QueryFirstOrDefaultAsync(this IDbContext dbContext, Parameters param)
        => dbContext.QueryFirstOrDefaultAsync<Result>(QUERY, param);

    public class Result
    {
        public int IdCampaign { get; set; }
        public bool CampaignExists { get; set; }
        public bool CampaignHasContent { get; set; }
        public int? EditorType { get; set; }
        public string Content { get; set; }
        public string Meta { get; set; }
    }

    public class Parameters
    {
        public int IdCampaign { get; set; }
        public string AccountName { get; set; }
    }
}
