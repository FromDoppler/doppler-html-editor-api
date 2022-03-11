using System;
using System.Collections.Generic;
using System.Linq;
using Doppler.HtmlEditorApi;

namespace HtmlAgilityPack;

public static class HtmlAgilityPackUtils
{
    public static void TraverseAndReplaceTextsAndAttributeValues(this HtmlNode node, Func<string, string> replaceFunc)
    {
        if (node is HtmlTextNode textNode)
        {
            textNode.ReplaceText(replaceFunc);
        }

        foreach (var attribute in node.Attributes)
        {
            attribute.ReplaceValue(replaceFunc);
        }

        foreach (var child in node.ChildNodes)
        {
            child.TraverseAndReplaceTextsAndAttributeValues(replaceFunc);
        }
    }

    public static void ReplaceValue(this HtmlAttribute attribute, Func<string, string> replaceFunc)
    {
        var originalText = attribute.Value;

        if (originalText == null)
        {
            return;
        }

        var newText = replaceFunc(originalText);
        if (originalText != newText)
        {
            attribute.Value = newText;
        }
    }

    public static void ReplaceText(this HtmlTextNode textNode, Func<string, string> replaceFunc)
    {
        var originalText = textNode.InnerHtml;

        if (originalText == null)
        {
            return;
        }

        var newText = replaceFunc(originalText);
        if (originalText != newText)
        {
            textNode.InnerHtml = newText;
        }
    }

    public static IEnumerable<HtmlNode> GetLinkNodes(this HtmlNode node)
        => node.SelectNodes(@"//a").EmptyIfNull()
            .Union(node.SelectNodes(@"//area").EmptyIfNull());

    public static HtmlDocument LoadHtml(string inputHtml)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(inputHtml);
        return htmlDocument;
    }
}
