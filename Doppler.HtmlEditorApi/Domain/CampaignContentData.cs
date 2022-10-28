namespace Doppler.HtmlEditorApi.Domain;

// TemplateContentData is different than CampaignContentData because CampaignContentData has
// two fields HtmlContent and HtmlHead and TemplateContentData merges both in HtmlComplete
public abstract record CampaignContentData();

public sealed record EmptyCampaignContentData()
    : CampaignContentData();

public sealed record UnknownCampaignContentData(
    string Content,
    string Head,
    string Meta,
    int? EditorType)
    : CampaignContentData();

public sealed record MSEditorCampaignContentData(
    string Content)
    : CampaignContentData();

public abstract record BaseHtmlCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    int? IdTemplate)
    : CampaignContentData();

public sealed record HtmlCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    int? IdTemplate)
    : BaseHtmlCampaignContentData(HtmlContent, HtmlHead, IdTemplate);

public sealed record UnlayerCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    string Meta,
    int? IdTemplate)
    : BaseHtmlCampaignContentData(HtmlContent, HtmlHead, IdTemplate);
