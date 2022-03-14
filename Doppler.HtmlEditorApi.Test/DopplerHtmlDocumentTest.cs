using Xunit.Abstractions;
using Xunit;
using System.Linq;

namespace Doppler.HtmlEditorApi;

public class DopplerHtmlDocumentTest
{
    #region Content examples
    const string HTML_SOCIALSHARE_TABLE_WITH_ERRORS = $@"
<table>
    <tr>
        <td id=""facebook"" valign=""middle"">
            <a
                socialshare=""4""
                ondragstart=""return false;""
                href=""    http://www.facebook.com/share.php?u=https%3a%2f%2fvp.mydplr.com%2f123&amp;t=Prueba+MSEditor""
                target=""_blank""
            >
                <img
                ondragstart=""return false;""
                src=""https://app2.dopplerfiles.com/MSEditor/images/color_small_facebook.png""
                alt=""Facebook""
                width=""40""
                />
            </a>
        </td>
        <td id=""linkedin"" valign=""middle"">
            <a
                socialshare=""3""
                ondragstart=""return false;""
                href=""
http://www.linkedin.com/
shareArticle?mini=true&amp;url=https%3a%2f%2fvp.mydplr.com%2f123&amp;title=Prueba+MSEditor&amp;summary=&amp;source=Add%20to%20Any
""
                target=""_blank""
            >
                <img
                ondragstart=""return false;""
                src=""https://app2.dopplerfiles.com/MSEditor/images/color_small_linkedin.png""
                alt=""Linkedin""
                width=""40""
                />
            </a>
        </td>
        <td id=""pinterest"" valign=""middle"">
            <a
                socialshare=""20""
                ondragstart=""return false;""
                href=""WRONG URL""
                target=""_blank""
            >
                <img
                ondragstart=""return false;""
                src=""https://app2.dopplerfiles.com/MSEditor/images/color_small_pinterest.png""
                alt=""Pinterest""
                width=""40""
                />
            </a>
        </td>
        <td id=""whatsapp"" class=""dplr-mobile-only"" valign=""middle"">
            <a
                socialshare=""24""
                ondragstart=""return false;""
                href=""whatsapp://send?text=https%3a%2f%2fvp.mydplr.com%2f123""
                target=""_blank""
            >
                <img
                ondragstart=""return false;""
                src=""https://app2.dopplerfiles.com/MSEditor/images/color_small_whatsapp.png""
                alt=""Whatsapp""
                width=""40""
                />
            </a>
        </td>
        <td id=""twitter"" valign=""middle"">
            <a
                socialshare=""2""
                ondragstart=""return false;""
                href=""http://twitter.com/share?related=fromdoppler&amp;text=Prueba+MSEditor&amp;url=https%3a%2f%2fvp.mydplr.com%2f123""
                target=""_blank""
            >
                <img
                ondragstart=""return false;""
                src=""https://app2.dopplerfiles.com/MSEditor/images/color_small_twitter.png""
                alt=""Twitter""
                width=""40""
                />
            </a>
        </td>
    </tr>
</table>";
    #endregion

    private readonly ITestOutputHelper _output;

    public DopplerHtmlDocumentTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void GetFieldsId_should_return_distinct_field_ids()
    {
        // Arrange
        var html = $@"<!doctype html>
        <html>
            <body>
            EMAIL: |*|321*|*
            correo: |*|321*|*
            BIRTHDAY |*|323*|*
            </body>
        </html>";
        var htmlDocument = new DopplerHtmlDocument(html);

        // Act
        var result = htmlDocument.GetFieldsId();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(321, result);
        Assert.Contains(323, result);
    }

    [Theory]
    [InlineData("EMAIL: |*|*|*\ncorreo: |*|321*|*\nBIRTHDAY |*|*|*", 1)]
    [InlineData("EMAIL: |*|*|*", 0)]
    public void GetFieldsId_with_fields_equals_zero_or_empty_should_omitted_into_return(string html, int expectedCount)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(html);

        // Act
        var result = htmlDocument.GetFieldsId();

        // Assert
        Assert.Equal(expectedCount, result.Count());
    }

    [Fact]
    public void SanitizeTrackableLinks_should_not_modify_html_when_there_are_no_links()
    {
        // Arrange
        var input = $@"<p>Hello!</p>";
        var htmlDocument = new DopplerHtmlDocument(input);
        var contentWithoutSanitization = htmlDocument.GetDopplerContent();

        // Act
        htmlDocument.SanitizeTrackableLinks();
        var output = htmlDocument.GetDopplerContent();

        // Assert
        Assert.Equal(contentWithoutSanitization, output);
    }

    [Theory]
    [InlineData(
        "https://\tgoo gle1\n.com    \r\n  ",
        "https://google1.com"
    )]
    [InlineData(
        "HTTPS://GOOGLE.com/   SEGMENT",
        "https://google.com/SEGMENT"
    )]
    [InlineData(
        "\n  https://google2.com\n   ",
        "https://google2.com"
    )]
    [InlineData(
        "%20\n%20  https://google3.com/test%20space%20\n %20  ",
        "https://google3.com/test%20space"
    )]
    [InlineData(
        "%20\n%20  HTTP://GOOGLE4.com/TEST%20space%20\n %20  ",
        "http://google4.com/TEST%20space"
    )]
    [InlineData(
        "%20\n%20  www.google5.com%20\n %20  ",
        "http://www.google5.com"
    )]
    [InlineData(
        "%20\n%20  google6.com%20\n %20  ",
        "%20\n%20  google6.com%20\n %20  "
    )]
    [InlineData(
        "%20\n%20  WWW.GOOGLE7.com/TEST%20space%20\n %20  ",
        "http://www.google7.com/TEST%20space"
    )]
    public void SanitizeTrackableLinks_should_sanitize_trackable_links(string inputHref, string expectedHref)
    {
        // Arrange
        var input = CreateTestContentWithLink(inputHref);
        var expected = CreateTestContentWithLink(expectedHref);

        var htmlDocument = new DopplerHtmlDocument(input);

        // Act
        htmlDocument.SanitizeTrackableLinks();
        var output = htmlDocument.GetDopplerContent();

        // Assert
        Assert.Equal(expected, output);
    }

    [Fact]
    public void SanitizeTrackableLinks_should_not_modify_socialshare_links()
    {
        // Arrange
        var input = HTML_SOCIALSHARE_TABLE_WITH_ERRORS;
        var htmlDocument = new DopplerHtmlDocument(input);
        var contentWithoutSanitization = htmlDocument.GetDopplerContent();

        // Act
        htmlDocument.SanitizeTrackableLinks();
        var output = htmlDocument.GetDopplerContent();

        // Assert
        Assert.Equal(contentWithoutSanitization, output);
    }

    private string CreateTestContentWithLink(string href)
        => $@"<div>
    <a href=""{href}"">Link</a>
</div>
";

}
