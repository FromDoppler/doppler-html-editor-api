namespace Doppler.HtmlEditorApi.Domain;

public record TemplateModel(
    int TemplateId,
    bool IsPublic,
    string PreviewImage,
    string Name,
    TemplateContentData Content);
