using System;

namespace HtmlAgilityPack;

public static class HtmlAgilityPackUtils
{
    public static HtmlDocument LoadHtml(string inputHtml)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(inputHtml);
        return htmlDocument;
    }
}
