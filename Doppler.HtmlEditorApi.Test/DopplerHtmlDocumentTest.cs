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
        var result = htmlDocument.GetFieldIds();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(321, result);
        Assert.Contains(323, result);
    }

    [Theory]
    [InlineData("EMAIL: |*|*|*\ncorreo: |*|321*|*\nBIRTHDAY |*|*|*", 1)]
    public void GetFieldsId_with_fields_equals_zero_or_empty_should_omitted_into_return(string html, int expectedCount)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(html);

        // Act
        var result = htmlDocument.GetFieldIds();

        // Assert
        Assert.Equal(expectedCount, result.Count());
    }

    [Theory]
    [InlineData("EMAIL: |*|*|*")]
    [InlineData("No field ids")]
    public void GetFieldsId_without_field_ids_or_one_empty_field_id_should_be_return_null(string html)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(html);

        // Act
        var result = htmlDocument.GetFieldIds();

        // Assert
        Assert.Empty(result);
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

    [Fact]
    public void GetTrackableUrls_should_return_empty_list_when_there_are_not_links()
    {
        // Arrange
        var input = "<p>Hello!</p>";
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();

        // Act
        var links = htmlDocument.GetTrackableUrls();

        // Assert
        Assert.Empty(links);
    }

    [Fact]
    public void GetTrackableUrls_should_return_empty_list_when_there_are_only_socialshare_links()
    {
        // Arrange
        var input = HTML_SOCIALSHARE_TABLE_WITH_ERRORS;
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();

        // Act
        var links = htmlDocument.GetTrackableUrls();

        // Assert
        Assert.Empty(links);
    }

    [Fact]
    public void GetTrackableUrls_should_return_list_of_trackable_urls()
    {
        // Arrange
        var input = @"<ul>
    <li><a href=""https://www.google.com/search?q=search%20term"">Result 1 (HTTPS)</a></li>
    <li><a href=""HTTP://www.GOOGLE.com/search?q=SEARCH%20term"">Result 2 (HTTP, with uppercase)</a></li>
    <li><a href=""www.GOOGLE.com/search?q=SEARCH%20term"">Result 3 (with www without scheme)</a></li>
    <li><a href=""GOOGLE.com/search?q=SEARCH%20term"">No Result (without www without scheme)</a></li>
    <li><a href=""ftp://GOOGLE.com/search?q=SEARCH%20term"">Result 4 (ftp)</a></li>
    <li><a href=""www.GOOGLE.com/search?q=SEARCH%20term"">No Result (duplicated)</a></li>
</ul>";
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();

        // Act
        var links = htmlDocument.GetTrackableUrls();

        // Assert
        Assert.Equal(4, links.Count());
    }

    [Fact]
    public void GetTrackableUrls_should_not_map_fieldnames()
    {
        // Arrange
        var input = @"<ul>
    <li><a href=""https://www.|*|123*|*.com/search?q=|*|456*|*"">Result 1 (HTTPS)</a></li>
    <li><a href=""HTTP://|*|1*|*/search?q=SEARCH%20term"">Result 2 (HTTP, with uppercase)</a></li>
    <li><a href=""www.GOOGLE.com/[[[field]]]"">Result 3 (with www without scheme)</a></li>
    <li><a href=""ftp://|*|3*|*"">Result 4 (ftp)</a></li>
</ul>";
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();

        // Act
        var links = htmlDocument.GetTrackableUrls();

        // Assert
        Assert.Collection(
            links,
            link => Assert.Equal("https://www.|*|123*|*.com/search?q=|*|456*|*", link),
            link => Assert.Equal("HTTP://|*|1*|*/search?q=SEARCH%20term", link),
            link => Assert.Equal("www.GOOGLE.com/[[[field]]]", link),
            link => Assert.Equal("ftp://|*|3*|*", link));
    }

    private string CreateTestContentWithLink(string href)
        => $@"<div>
    <a href=""{href}"">Link</a>
</div>
";

}
