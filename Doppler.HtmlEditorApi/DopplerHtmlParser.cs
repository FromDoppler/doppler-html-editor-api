namespace Doppler.HtmlEditorApi;

/// <summary>
/// This class deals with the conversion between a standard HTML content
/// and the set of strings that represents the content in Doppler DB.
/// It is named Doppler because it should replicate current Doppler logic.
/// </summary>
public class DopplerHtmlParser
{
    // Old Doppler code:
    // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/ToDoppler/CampaignContent_To_DtoContent.cs#L27-L29
    private readonly string _inputHtml;

    public DopplerHtmlParser(string inputHtml)
    {
        _inputHtml = inputHtml;
    }

    public string GetDopplerContent()
        => _inputHtml;

    public string GetHeadContent()
        => null;
}
