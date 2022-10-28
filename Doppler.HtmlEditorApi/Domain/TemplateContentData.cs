namespace Doppler.HtmlEditorApi.Domain;

// TemplateContentData is different than CampaignContentData because CampaignContentData has
// two fields HtmlContent and HtmlHead and TemplateContentData merges both in HtmlComplete
public abstract record TemplateContentData()
{ }

public sealed record UnlayerTemplateContentData(
    string HtmlComplete,
    string Meta)
    : TemplateContentData();

public sealed record UnknownTemplateContentData(
    int EditorType)
    : TemplateContentData();
