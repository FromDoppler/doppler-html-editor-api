namespace Doppler.HtmlEditorApi.Storage;

public class ContentRow
{
    private static readonly int? HTML_EDITOR_TYPE = null;
    private const int UNLAYER_EDITOR_TYPE = 5;

    public string Content { get; set; }
    public string Meta { get; set; }
    public int IdCampaign { get; set; }
    public int? EditorType { get; set; }

    public static ContentRow CreateEmpty(int campaignId)
    {
        return ContentRow.CreateUnlayerContentRow(
            content: "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <title></title></head><body></body></html>",
            meta: "{\"body\":{\"rows\":[]}}",
            idCampaign: campaignId);
    }

    public static ContentRow CreateUnlayerContentRow(string content, string meta, int idCampaign) =>
        new ContentRow()
        {
            Content = content,
            Meta = meta,
            EditorType = UNLAYER_EDITOR_TYPE,
            IdCampaign = idCampaign
        };

    public static ContentRow CreateHtmlContentRow(string content, int idCampaign) =>
        new ContentRow()
        {
            Content = content,
            Meta = null,
            EditorType = HTML_EDITOR_TYPE,
            IdCampaign = idCampaign
        };

    public bool HasHtmlEditorType => EditorType == HTML_EDITOR_TYPE;
    public bool HasUnlayerEditorType => EditorType == UNLAYER_EDITOR_TYPE;

}
