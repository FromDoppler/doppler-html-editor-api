using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record FirstOrDefaultCampaignStatusDbQuery(
    int IdCampaign,
    string AccountName
) : ISingleItemDbQuery<FirstOrDefaultCampaignStatusDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
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

    public class Result
    {
        public bool OwnCampaignExists { get; init; }
        public bool ContentExists { get; init; }
        public int? EditorType { get; init; }
    }
}
