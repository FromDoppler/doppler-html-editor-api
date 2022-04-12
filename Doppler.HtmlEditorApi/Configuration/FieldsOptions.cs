using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Configuration;

public record FieldsOptions
{
    public FieldAliasesDef[] Aliases { get; init; }
}
