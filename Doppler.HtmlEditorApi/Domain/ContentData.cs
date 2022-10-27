namespace Doppler.HtmlEditorApi.Domain;

public abstract record ContentData();

public sealed record EmptyContentData()
    : ContentData();

public sealed record UnknownContentData(
    int CampaignId,
    string Content,
    string Head,
    string Meta,
    int? EditorType,
    string PreviewImage)
    : ContentData();

public sealed record MSEditorContentData(
    int CampaignId,
    string Content)
    : ContentData();

public abstract record BaseHtmlContentData(
    string HtmlContent,
    string HtmlHead,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : ContentData();

public sealed record HtmlContentData(
    string HtmlContent,
    string HtmlHead,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : BaseHtmlContentData(HtmlContent, HtmlHead, PreviewImage, CampaignName, IdTemplate);

public sealed record UnlayerContentData(
    string HtmlContent,
    string HtmlHead,
    string Meta,
    string PreviewImage,
    string CampaignName,
    int? IdTemplate)
    : BaseHtmlContentData(HtmlContent, HtmlHead, PreviewImage, CampaignName, IdTemplate);
