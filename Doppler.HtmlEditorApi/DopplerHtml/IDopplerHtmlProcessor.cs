namespace Doppler.HtmlEditorApi.DopplerHtml;

/// <summary>
/// This service deal with the conversion between a standard HTML content
/// and the set of strings that represents the content in Doppler DB.
/// It is named Doppler because it should replicate current Doppler logic.
/// </summary>
public interface IDopplerHtmlProcessor
{
    DopplerHtmlData ExtractDopplerHtmlData(string html);
    string GenerateHtmlContent(DopplerHtmlData dopplerHtmlData);
}
