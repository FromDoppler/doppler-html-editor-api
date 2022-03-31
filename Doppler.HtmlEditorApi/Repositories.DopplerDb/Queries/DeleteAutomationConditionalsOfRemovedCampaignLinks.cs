using System.Collections.Generic;
using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

/// <summary>
/// It deletes AutomationConditional link entries that are not present in the payload.
/// It does not insert or update nothing else.
/// </summary>
/// <remarks>
/// Dapper deals with the SQL `IN` operator and the IEnumerable.
/// (See https://github.com/DapperLib/Dapper#list-support)
/// Example with 4 links:
///     exec sp_executesql N'
///       DELETE ac
///       FROM [AutomationConditional] ac
///       INNER JOIN [Link] l ON ac.IdLink = l.IdLink
///       WHERE l.IdCampaign = @IdContent
///       AND l.UrlLink NOT IN (@Links1,@Links2,@Links3,@Links4)',
///     N'@IdContent int,@Links1 nvarchar(4000),@Links2 nvarchar(4000),@Links3 nvarchar(4000),@Links4 nvarchar(4000)',
///     @IdContent=36357513,@Links1=N'https://www.google.com?q=|*|320*|*',@Links2=N'a',@Links3=N'b',@Links4=N'c'
/// Example without links:
///     exec sp_executesql N'
///       DELETE ac
///       FROM [AutomationConditional] ac
///       INNER JOIN [Link] l ON ac.IdLink = l.IdLink
///       WHERE l.IdCampaign = @IdContent
///       AND l.UrlLink NOT IN (SELECT @Links WHERE 1 = 0)',
///     N'@IdContent int,@Links nvarchar(4000)',
///     @IdContent=36357513,@Links=NULL
/// </remarks>
public record DeleteAutomationConditionalsOfRemovedCampaignLinks(
    int IdContent,
    IEnumerable<string> Links
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
DELETE ac
FROM [AutomationConditional] ac
INNER JOIN [Link] l ON ac.IdLink = l.IdLink
WHERE l.IdCampaign = @IdContent
AND l.UrlLink NOT IN @Links
";
}
