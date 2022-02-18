namespace Doppler.HtmlEditorApi.Storage;

public abstract record ContentData(
    int campaignId);

public sealed record EmptyContentData(
    int campaignId)
    : ContentData(campaignId);

public sealed record UnknownContentData(
    int campaignId,
    string content,
    string meta,
    int? editorType)
    : ContentData(campaignId);

public sealed record MSEditorContentData(
    int campaignId,
    string content)
    : ContentData(campaignId);

public abstract record BaseHtmlContentData(
    int campaignId,
    string htmlContent)
    : ContentData(campaignId);

public sealed record HtmlContentData(
    int campaignId,
    string htmlContent)
    : BaseHtmlContentData(campaignId, htmlContent);

public sealed record UnlayerContentData(
    int campaignId,
    string htmlContent,
    string meta)
    : BaseHtmlContentData(campaignId, htmlContent);
