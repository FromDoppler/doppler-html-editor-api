using System.Linq;
using Doppler.HtmlEditorApi.Test.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.HtmlEditorApi.Domain;

public class DopplerHtmlDocumentTest
{
    #region Content examples
    private const string HTML_SOCIALSHARE_TABLE_WITH_ERRORS = $@"
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
    [InlineData(
        "https://test.fromdoppler.net?QueryString=ItShouldHaveInstance",
        "https://test.fromdoppler.net?QueryString=ItShouldHaveInstance"
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
    public void GetTrackableUrls_should_not_map_field_names()
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

    [Fact]
    public void SanitizeDynamicContentNodes_should_modify_html_dynamic_content_when_has_two_or_more_child_node_and_replace_image_src()
    {
        // Arrange
        var input = $@"<dynamiccontent action=""abandoned_cart"" items=""2"">
    <div role=""container"">
        <section>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">
                <img
                    src=""https://cdn.fromdoppler.com/unlayer-editor/assets/cart_v2.svg""
                    alt=""product image""
                />
            </a>
        </section>
        <section>
            <span>[[[DC:TITLE]]]</span>
            <span >[[[DC:PRICE]]]</span>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">Comprar</a>
        </section>
    </div>
    <div role=""container"">
        <section>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">
                <img
                    src=""https://cdn.fromdoppler.com/unlayer-editor/assets/cart_v2.svg""
                    alt=""product image""
                />
            </a>
        </section>
        <section>
            <span>[[[DC:TITLE]]]</span>
            <span >[[[DC:PRICE]]]</span>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">Comprar</a>
        </section>
    </div></dynamiccontent>";
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();
        var sanitizedContent = $@"<dynamiccontent action=""abandoned_cart"" items=""2"">
    <div role=""container"">
        <section>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">
                <img src=""[[[DC:IMAGE]]]"" alt=""product image"">
            </a>
        </section>
        <section>
            <span>[[[DC:TITLE]]]</span>
            <span>[[[DC:PRICE]]]</span>
            <a role=""link"" href=""[[[DC:URL]]]"" target=""_blank"">Comprar</a>
        </section>
    </div>
    </dynamiccontent>";

        // Act
        htmlDocument.SanitizeDynamicContentNodes();
        var content = htmlDocument.GetDopplerContent();

        // Assert
        Assert.Equal(sanitizedContent, content);
    }


    [Fact]
    public void GetTrackableUrls_remove_encoding_on_sanitize_to_save()
    {
        // Arrange
        var input = "<p><a href=\"https://midomain.com/?param1=aa&amp;param2=bb\">This is a link with parameters</a></p>";
        var htmlDocument = new DopplerHtmlDocument(input);
        htmlDocument.GetDopplerContent();
        htmlDocument.SanitizeTrackableLinks();

        // Act
        var links = htmlDocument.GetTrackableUrls();

        // Assert
        Assert.Equal("https://midomain.com/?param1=aa&param2=bb", links.FirstOrDefault());
    }

    [Theory]
    [InlineDataAttribute(
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph</p>
    <p>This is another paragraph <SCRIPT>script</SCRIPT> with a script</p>
    <SCRIPT>another script</SCRIPT>
    <embed>embed tag should not be closed</embed>
    <IFRAME />
    <IFRAME title=""Inline Frame Example"" src=""malicious.html""></IFRAME>
</article>",
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph</p>
    <p>This is another paragraph  with a script</p>
    embed tag should not be closed
</article>")]
    [InlineDataAttribute(
        // Weird behavior with self-closed script
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph <script src=""malicious.js"" /> with a not well-formed script</p>
    <script>another script</script>
    <p>This is another paragraph</p>
</article>",
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph
    <p>This is another paragraph</p>
</article>")]
    [InlineDataAttribute(
        // Weird behavior with self-closed script
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph <script src=""malicious.js"" /> with a not well-formed script</p>
    <p>This is another paragraph</p>
</article>",
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph </p></article>")]
    [InlineDataAttribute(
        @"
<html>
<head>
    <title>Hello safety!</title>
    <script>script in the head</script>
</head>
<body>
    <h1>Title</h1>
    <p>This is a paragraph</p>
    <p>This is another paragraph <script>script</script> with a script</p>
    <script>another script</script>
</body>
</html>",
        @"
<h1>Title</h1>
<p>This is a paragraph</p>
<p>This is another paragraph  with a script</p>
")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><iframe /><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><iframe><p>paragraph 2</p>",
        @"<p>paragraph 1</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p></iframe><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><SCRIPT /><p>paragraph 2</p>",
        @"<p>paragraph 1</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><script><p>paragraph 2</p>",
        @"<p>paragraph 1</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p></script><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><embed /><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p><EMBED><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"<p>paragraph 1</p></embed><p>paragraph 2</p>",
        @"<p>paragraph 1</p><p>paragraph 2</p>")]
    [InlineDataAttribute(
        @"
<meta name=""copyright"" content=""© 2022 FromDoppler"">
<meta http-equiv=""  refresh  "" content=""5;url=https://malisious.site"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">",
        @"
<meta name=""copyright"" content=""© 2022 FromDoppler"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">")]
    public void RemoveHarmfulTags_should_remove_harmful_tags_from_body(string input, string expectedBody)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(input);

        // Act
        htmlDocument.RemoveHarmfulTags();

        // Assert
        var content = htmlDocument.GetDopplerContent();
        AssertHelper.EqualIgnoringMeaninglessSpaces(expectedBody, content);
    }

    [Theory]
    [InlineDataAttribute(
        @"
<html>
<head>
    <IFRAME></IFRAME>
    <embed>
    <title>Hello safety!</title>
    <script>script in the head</script>
</head>
<body>
    <h1>Title</h1>
    <p>This is a paragraph</p>
    <p>This is another paragraph <script>script</script> with a script</p>
    <script>another script</script>
</body>
</html>",
        @"
<title>Hello safety!</title>")]
    [InlineDataAttribute(
        @"
<html>
<head>
    <title>Hello safety!</title>
    <meta name=""copyright"" content=""© 2022 FromDoppler"">
    <meta http-equiv=""  refresh  "" content=""5;url=https://malisious.site"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
</head>
<body>
    <p>This is a paragraph</p>
</body>
</html>",
        @"
<title>Hello safety!</title>
<meta name=""copyright"" content=""© 2022 FromDoppler"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">")]
    public void RemoveHarmfulTags_should_remove_harmful_tags_from_head(string input, string expectedHead)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(input);

        // Act
        htmlDocument.RemoveHarmfulTags();

        // Assert
        var headContent = htmlDocument.GetHeadContent();
        AssertHelper.EqualIgnoringMeaninglessSpaces(expectedHead, headContent);
    }

    [Theory]
    [InlineDataAttribute(
        @"
<article>
    <h1>Title</h1>
    <p onclick=""alert('malicious message')"">This is a paragraph.</p>
    <p OnMouseOver=""window.href='https://malicious.site'"">This is another paragraph.</p>
    <button onclick=""alert('malicious message')"">This is a button.</button>
</article>",
        @"
<article>
    <h1>Title</h1>
    <p>This is a paragraph.</p>
    <p>This is another paragraph.</p>
    <button>This is a button.</button>
</article>")]
    public void RemoveEventAttributes_should_remove_eventAttributes_from_body(string input, string expectedBody)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(input);

        // Act
        htmlDocument.RemoveEventAttributes();

        // Assert
        var content = htmlDocument.GetDopplerContent();
        AssertHelper.EqualIgnoringMeaninglessSpaces(expectedBody, content);
    }

    [Theory]
    [InlineDataAttribute(
        @"
<html>
<head>
    <title>Hello safety!</title>
    <meta name=""copyright"" content=""© 2022 FromDoppler"">
    <p onclick=""alert('malicious message')"">This is a paragraph.</p>
    <p OnMouseOver=""window.href='https://malicious.site'"">This is another paragraph.</p>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
</head>
<body>
    <p>This is a paragraph</p>
</body>
</html>",
        @"
<title>Hello safety!</title>
<meta name=""copyright"" content=""© 2022 FromDoppler"">
<p>This is a paragraph.</p>
<p>This is another paragraph.</p>
<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">")]
    public void RemoveEventAttributes_should_remove_eventAttributes_from_head(string input, string expectedHead)
    {
        // Arrange
        var htmlDocument = new DopplerHtmlDocument(input);

        // Act
        htmlDocument.RemoveEventAttributes();

        // Assert
        var headContent = htmlDocument.GetHeadContent();
        AssertHelper.EqualIgnoringMeaninglessSpaces(expectedHead, headContent);
    }

    private static string CreateTestContentWithLink(string href)
        => $@"<div>
    <a href=""{href}"">Link</a>
</div>
";

}
