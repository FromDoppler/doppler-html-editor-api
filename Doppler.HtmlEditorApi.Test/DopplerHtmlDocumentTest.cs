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
}
