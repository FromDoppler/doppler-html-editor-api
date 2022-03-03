namespace Doppler.HtmlEditorApi;

public record FieldAliasesDef(
    string canonicalName,
    params string[] aliases);
