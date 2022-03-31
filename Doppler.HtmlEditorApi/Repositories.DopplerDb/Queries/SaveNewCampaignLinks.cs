
using System.Collections.Generic;
using System.Linq;
using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

/// <summary>
/// It keeps existing DB entries and only adds new ones without deleting or updating anything.
/// </summary>
public record SaveNewCampaignLinks(
    int IdContent,
    IEnumerable<string> Links
) : IExecutableDbQuery
{
    const string BASE_QUERY = @"
INSERT INTO [Link] (IdCampaign, UrlLink, IsActiveForTracking, IsDynamic)
SELECT rest.IdCampaign, newLinks.UrlLink, rest.IsActiveForTracking, rest.IsDynamic
FROM
    (SELECT @IdCampaign AS IdCampaign, 1 AS IsActiveForTracking, 0 AS IsDynamic) rest
JOIN (
    {{linksSelect}}
    ) AS newLinks ON 1=1
LEFT JOIN [Link] oldLinks ON oldLinks.UrlLink = newLinks.UrlLink AND oldLinks.IdCampaign = rest.IdCampaign
WHERE oldLinks.IdCampaign IS NULL";

    // The result is something like:
    //    SELECT @Url1 AS UrlLink
    //    UNION SELECT @Url2 AS UrlLink
    //    UNION SELECT @Url3 AS UrlLink
    public string GenerateSqlQuery()
        => BASE_QUERY.Replace(
            "{{linksSelect}}",
            string.Join(
                "\n    UNION ",
                Enumerable.Range(1, Links.Count())
                .Select(x => $"SELECT @Url{x} AS UrlLink")));

    // The result is something like:
    //     Dictionary<string, object>()
    //     {
    //         ["IdCampaign"] = 36357512,
    //         ["Url1"] = "https://www.google.com",
    //         ["Url2"] = "https://www.yahoo.com",
    //         ["Url3"] = "https://www.altavista.com",
    //     };
    public object GenerateSqlParameters()
        => new[] { new { name = "IdCampaign", value = (object)IdContent } }
            .Union(Links.Select((x, i) => new { name = $"Url{i + 1}", value = (object)x }))
            .ToDictionary(x => x.name, x => x.value);

}
