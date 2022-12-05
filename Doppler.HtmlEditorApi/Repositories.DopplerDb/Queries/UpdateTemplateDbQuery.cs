using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdateTemplateDbQuery(
    int IdTemplate,
    int? EditorType,
    string HtmlCode,
    string Meta,
    string PreviewImage,
    string Name
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => $"""
        UPDATE Template
        SET
            EditorType = @EditorType,
            HtmlCode = @HtmlCode,
            Meta = @Meta,
            PreviewImage = @PreviewImage,
            Name = @Name,
            ModifiedAt = GETUTCDATE()
        WHERE IdTemplate = @IdTemplate
        """;
}
