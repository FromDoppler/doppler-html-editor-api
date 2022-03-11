using Xunit.Abstractions;
using Xunit;
using System.Linq;

namespace Doppler.HtmlEditorApi;

public class DopplerHtmlDocumentTest
{
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

    private string CreateTestContentWithLink(string href)
        => $@"<div>
    <a href=""{href}"">Link</a>
</div>
";

}
