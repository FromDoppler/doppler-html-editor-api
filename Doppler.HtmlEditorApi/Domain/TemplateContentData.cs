namespace Doppler.HtmlEditorApi.Domain;

public abstract record TemplateContentData()
{ }

public sealed record UnlayerTemplateContentData(
    string HtmlCode,
    string Meta,
    string PreviewImage,
    string Name,
    int EditorType,
    bool IsPublic)
    : TemplateContentData();

public sealed record UnknownTemplateContentData(
    int EditorType,
    bool IsPublic)
    : TemplateContentData();
