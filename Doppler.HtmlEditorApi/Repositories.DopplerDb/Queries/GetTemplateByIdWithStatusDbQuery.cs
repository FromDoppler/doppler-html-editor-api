using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record GetTemplateByIdWithStatusDbQuery(
    int IdTemplate,
    string AccountName
) : ISingleItemDbQuery<GetTemplateByIdWithStatusDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
SELECT
    @IdTemplate as IdTemplate,
    CAST (CASE WHEN Tp.IdUser IS NULL THEN 1 ELSE 0 END AS BIT) AS IsPublic,
    Tp.EditorType,
    Tp.HtmlCode,
    Tp.Meta,
    Tp.PreviewImage,
    Tp.Name
FROM [Template] Tp
LEFT JOIN [User] u ON
    u.IdUser = Tp.IdUser
WHERE
    Tp.IdTemplate = @IdTemplate
    AND (u.Email = @AccountName OR Tp.IdUser IS NULL)
    AND Tp.Active = 1";

    public class Result
    {
        public bool IsPublic { get; init; }
        public int EditorType { get; init; }
        public string HtmlCode { get; init; }
        public string Meta { get; init; }
        public string PreviewImage { get; init; }
        public string Name { get; init; }
    }
}
