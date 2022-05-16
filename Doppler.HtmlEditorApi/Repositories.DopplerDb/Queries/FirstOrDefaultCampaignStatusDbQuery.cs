using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record FirstOrDefaultCampaignStatusDbQuery(
    int IdCampaign,
    string AccountName
) : ISingleItemDbQuery<FirstOrDefaultCampaignStatusDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
SELECT
    CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS OwnCampaignExists,
    CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS ContentExists,
    co.EditorType,
    ca.Status,
    t.TestType,
    caa.IdCampaign AS IdCampaignA,
    cab.IdCampaign AS IdCampaignB,
    car.IdCampaign AS IdCampaignResult
FROM [User] u
LEFT JOIN [Campaign] ca ON
    u.IdUser = ca.IdUser
    AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON
    ca.IdCampaign = co.IdCampaign
LEFT JOIN [TestAB] t ON
    ca.IdTestAB = t.IdTestAB
LEFT JOIN [Campaign] caa ON
    t.IdTestAB = caa.IdTestAB
    AND caa.TestABCategory = 1
LEFT JOIN [Campaign] cab ON
    t.IdTestAB = cab.IdTestAB
    AND cab.TestABCategory = 2
LEFT JOIN [Campaign] car ON
    t.IdTestAB = car.IdTestAB
    AND car.TestABCategory = 3
WHERE u.Email = @accountName";

    public class Result
    {
        public bool OwnCampaignExists { get; init; }
        public bool ContentExists { get; init; }
        public int? EditorType { get; init; }
        public int? Status { get; init; }
        public int? TestType { get; init; }
        public int? TestABCategory { get; init; }
        public int? IdCampaignA { get; init; }
        public int? IdCampaignB { get; init; }
        public int? IdCampaignResult { get; init; }
    }
}
