namespace Doppler.HtmlEditorApi.Domain;

public record FieldAliasesDef
{
    public string CanonicalName { get; init; }
    public string[] Aliases { get; init; }
}
