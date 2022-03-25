using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record FirstOrDefaultContentWithCampaignStatusDbQuery(
    int IdCampaign,
    string AccountName
) : ISingleItemDbQuery<FirstOrDefaultContentWithCampaignStatusDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
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
