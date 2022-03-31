namespace Doppler.HtmlEditorApi.Domain;

public record FieldAliasesDef
{
    public string canonicalName { get; init; }
    public string[] aliases { get; init; }
}
