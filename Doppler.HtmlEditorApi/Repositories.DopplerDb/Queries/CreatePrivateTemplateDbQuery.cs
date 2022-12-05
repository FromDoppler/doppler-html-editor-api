using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record CreatePrivateTemplateDbQuery(
    string AccountName,
    int? EditorType,
    string HtmlCode,
    string Meta,
    string PreviewImage,
    string Name
) : ISingleItemDbQuery<CreatePrivateTemplateDbQuery.Result>
{
    public string GenerateSqlQuery() => $"""
        INSERT INTO Template (IdUser, EditorType, HtmlCode, Meta, PreviewImage, Name, Active)
        OUTPUT INSERTED.idTemplate AS NewTemplateId
        SELECT
            u.IdUser AS IdUser,
            @EditorType AS EditorType,
            @HtmlCode AS HtmlCode,
            @Meta AS Meta,
            @PreviewImage AS PreviewImage,
            @Name AS Name,
            1 AS Active
        FROM [User] u
        WHERE u.Email = @AccountName
        """;

    public class Result
    {
        public int NewTemplateId { get; init; }
    }
}
