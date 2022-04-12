namespace Doppler.HtmlEditorApi.Domain;

public abstract record ContentData(
    int CampaignId);

public sealed record EmptyContentData(
    int CampaignId)
    : ContentData(CampaignId);

public sealed record UnknownContentData(
    int CampaignId,
    string Content,
    string Head,
    string Meta,
    int? EditorType)
    : ContentData(CampaignId);

public sealed record MSEditorContentData(
    int CampaignId,
    string Content)
    : ContentData(CampaignId);

public abstract record BaseHtmlContentData(
    int CampaignId,
    string HtmlContent,
    string HtmlHead)
    : ContentData(CampaignId);

public sealed record HtmlContentData(
    int CampaignId,
    string HtmlContent,
    string HtmlHead)
    : BaseHtmlContentData(CampaignId, HtmlContent, HtmlHead);

public sealed record UnlayerContentData(
    int CampaignId,
    string HtmlContent,
    string HtmlHead,
    string Meta)
    : BaseHtmlContentData(CampaignId, HtmlContent, HtmlHead);
