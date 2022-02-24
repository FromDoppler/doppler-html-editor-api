namespace Doppler.HtmlEditorApi.Storage;

public abstract record ContentData(
    int campaignId);

public sealed record EmptyContentData(
    int campaignId)
    : ContentData(campaignId);

public sealed record UnknownContentData(
    int campaignId,
    string content,
    string head,
    string meta,
    int? editorType)
    : ContentData(campaignId);

public sealed record MSEditorContentData(
    int campaignId,
    string content)
    : ContentData(campaignId);

public abstract record BaseHtmlContentData(
    int campaignId,
    string htmlContent,
    string htmlHead)
    : ContentData(campaignId);

public sealed record HtmlContentData(
    int campaignId,
    string htmlContent,
    string htmlHead)
    : BaseHtmlContentData(campaignId, htmlContent, htmlHead);

public sealed record UnlayerContentData(
    int campaignId,
    string htmlContent,
    string htmlHead,
    string meta)
    : BaseHtmlContentData(campaignId, htmlContent, htmlHead);
