using System;
using HtmlAgilityPack;

namespace Doppler.HtmlEditorApi;

/// <summary>
/// This class deals with the conversion between a standard HTML content
/// and the set of strings that represents the content in Doppler DB.
/// It is named Doppler because it should replicate current Doppler logic.
/// </summary>
public class DopplerHtmlDocument
{
    // Old Doppler code:
    // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/ToDoppler/CampaignContent_To_DtoContent.cs#L27-L29
    private readonly HtmlNode _headNode;
    private readonly HtmlNode _contentNode;

    public DopplerHtmlDocument(string inputHtml)
    {
        var htmlDocument = LoadHtml(inputHtml);

        _headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");

        _contentNode = _headNode == null ? htmlDocument.DocumentNode
            : htmlDocument.DocumentNode.SelectSingleNode("//body")
            ?? LoadHtml(inputHtml.Replace(_headNode.OuterHtml, "")).DocumentNode;
    }

    public string GetDopplerContent()
        => EnsureContent(_contentNode.InnerHtml);

    public string GetHeadContent()
        => _headNode?.InnerHtml;

    public void ReplaceInContent(Func<string, string> replaceFunc)
    {
        // TODO: optimize it to do many replacements while traversing the HTML document
        _contentNode.InnerHtml = replaceFunc(_contentNode.InnerHtml);
    }

    private static string EnsureContent(string htmlContent)
        => string.IsNullOrWhiteSpace(htmlContent) ? "<BR>" : htmlContent;

    private static HtmlDocument LoadHtml(string inputHtml)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(inputHtml);
        return htmlDocument;
    }
}
