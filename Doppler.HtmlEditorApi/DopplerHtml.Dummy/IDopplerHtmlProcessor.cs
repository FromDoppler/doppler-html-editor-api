using System;

namespace Doppler.HtmlEditorApi.DopplerHtml.Dummy;

public class DummyDopplerHtmlProcessor : IDopplerHtmlProcessor
{
    public DopplerHtmlData ExtractDopplerHtmlData(string html)
        => new DopplerHtmlData(
            head: null,
            htmlContent: html);

    public string GenerateHtmlContent(DopplerHtmlData dopplerHtmlData)
        => dopplerHtmlData.htmlContent;
}
