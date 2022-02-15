namespace Doppler.HtmlEditorApi.Storage;

public abstract record ContentData(
    int campaignId);

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
    : BaseHtmlContentData(campaignId, htmlContent)
{
    public static UnlayerContentData CreateEmpty(int campaignId) =>
        new UnlayerContentData(
            campaignId,
            htmlContent: "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <title></title></head><body></body></html>",
            meta: "{\"body\":{\"rows\":[]}}");
}
