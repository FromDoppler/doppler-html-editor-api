namespace Doppler.HtmlEditorApi.Domain;

// TemplateContentData is different than CampaignContentData because CampaignContentData has
// two fields HtmlContent and HtmlHead and TemplateContentData merges both in HtmlComplete
public abstract record CampaignContentData();

public sealed record EmptyCampaignContentData()
    : CampaignContentData();

public sealed record UnknownCampaignContentData(
    int CampaignId,
    string Content,
    string Head,
    string Meta,
    int? EditorType,
    string PreviewImage)
    : CampaignContentData();

public sealed record MSEditorCampaignContentData(
    int CampaignId,
    string Content)
    : CampaignContentData();

public abstract record BaseHtmlCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : CampaignContentData();

public sealed record HtmlCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : BaseHtmlCampaignContentData(HtmlContent, HtmlHead, PreviewImage, CampaignName, IdTemplate);

public sealed record UnlayerCampaignContentData(
    string HtmlContent,
    string HtmlHead,
    string Meta,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : BaseHtmlCampaignContentData(HtmlContent, HtmlHead, PreviewImage, CampaignName, IdTemplate);
