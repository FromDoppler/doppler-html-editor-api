namespace Doppler.HtmlEditorApi.Domain;

public abstract record TemplateData()
{ }

public sealed record UnlayerTemplateData(
    string HtmlCode,
    string Meta,
    string PreviewImage,
    string Name,
    int EditorType,
    bool IsPublic)
    : TemplateData();

public sealed record UnknownTemplateData(
    int EditorType,
    bool IsPublic)
    : TemplateData();
