using System;
using HtmlAgilityPack;

namespace Doppler.HtmlEditorApi.DopplerHtml.HtmlAgilityPack;

public class AgilityPackDopplerHtmlProcessor : IDopplerHtmlProcessor
{
    public DopplerHtmlData ExtractDopplerHtmlData(string inputHtml)
    {
        // Old Doppler code:
        // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/ToDoppler/CampaignContent_To_DtoContent.cs#L27-L29
        var doc = new HtmlDocument();
        doc.LoadHtml(inputHtml);

        var headNode = doc.DocumentNode.SelectSingleNode("//head");

        string head;
        string htmlContent;

        if (headNode == null)
        {
            head = null;
            htmlContent = inputHtml;
        }
        else
        {
            head = headNode.InnerHtml;
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            htmlContent = bodyNode == null ? inputHtml.Replace(headNode.OuterHtml, "")
                : bodyNode.InnerHtml;
        }

        return new DopplerHtmlData(
            head,
            EnsureContent(htmlContent));
    }

    public string GenerateHtmlContent(DopplerHtmlData dopplerHtmlData)
    {
        // Old Doppler code:
        // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/FromDoppler/DtoContent_To_CampaignContent.cs#L23
        // Notice that it is not symmetric with ExtractDopplerHtmlData.
        // The head is being lossed here. It is not good if we try to edit an imported content.
        return dopplerHtmlData.htmlContent;
    }

    private static string EnsureContent(string htmlContent)
        => string.IsNullOrWhiteSpace(htmlContent) ? "<BR>" : htmlContent;

}
