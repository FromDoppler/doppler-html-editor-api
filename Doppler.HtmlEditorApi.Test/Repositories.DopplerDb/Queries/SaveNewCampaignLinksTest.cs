using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public class SaveNewCampaignLinksTest
{
    [Theory]
    [InlineData(
        1231,
        new string[0], // Unexpected escenario
        "(\n    \n    )")]
    [InlineData(
        1232,
        new[] { "https://www.google.com" },
        @"(
    SELECT @Url1 AS UrlLink
    )")]
    [InlineData(
        1232,
        new[] { "https://www.google.com", "https://www.yahoo.com" },
        @"(
    SELECT @Url1 AS UrlLink
    UNION SELECT @Url2 AS UrlLink
    )")]
    [InlineData(
        1232,
        new[] { "https://www.yahoo.com", "https://www.altavista.com", "https://www.google.com", "https://altavista.com" },
        @"(
    SELECT @Url1 AS UrlLink
    UNION SELECT @Url2 AS UrlLink
    UNION SELECT @Url3 AS UrlLink
    UNION SELECT @Url4 AS UrlLink
    )")]
    public void GenerateSqlQuery_should_insert_the_ids_in_sql_query(int idContent, string[] links, string expectedLinkParametersSubQueries)
    {
        // Arrange
        var dbQuery = new SaveNewCampaignLinks(
            IdContent: idContent,
            Links: links
        );
        var expectedQuery = $@"
INSERT INTO [Link] (IdCampaign, UrlLink, IsActiveForTracking, IsDynamic)
SELECT rest.IdCampaign, newLinks.UrlLink, rest.IsActiveForTracking, rest.IsDynamic
FROM
    (SELECT @IdCampaign AS IdCampaign, 1 AS IsActiveForTracking, 0 AS IsDynamic) rest
JOIN {expectedLinkParametersSubQueries} AS newLinks ON 1=1
LEFT JOIN [Link] oldLinks ON oldLinks.UrlLink = newLinks.UrlLink AND oldLinks.IdCampaign = rest.IdCampaign
WHERE oldLinks.IdCampaign IS NULL";

        // Act
        var sqlQuery = dbQuery.GenerateSqlQuery();

        // Assert
        Assert.Equal(expectedQuery, sqlQuery);
    }


    [Theory]
    [InlineData(
        1231,
        new string[0], // Unexpected escenario
        new string[0])]
    [InlineData(
        1232,
        new[] { "https://www.google.com" },
        new[] { "Url1" })]
    [InlineData(
        1232,
        new[] { "https://www.google.com", "https://www.yahoo.com" },
        new[] { "Url1", "Url2" })]
    [InlineData(
        1232,
        new[] { "https://www.yahoo.com", "https://www.altavista.com", "https://www.google.com", "https://altavista.com" },
        new[] { "Url1", "Url2", "Url3", "Url4" })]
    public void GenerateSqlParameters_should_generate_a_dictionary_with_each_URL(int idContent, string[] links, string[] expectedLinkParameterNames)
    {
        // Arrange
        var dbQuery = new SaveNewCampaignLinks(
            IdContent: idContent,
            Links: links
        );
        var expectedItemsCount = expectedLinkParameterNames.Length + 1; // links U IdContent

        // Act
        var sqlParameters = dbQuery.GenerateSqlParameters();

        // Assert
        var sqlParametersDictionary = Assert.IsAssignableFrom<Dictionary<string, object>>(sqlParameters);
        Assert.Contains("IdCampaign", sqlParametersDictionary.Keys);
        Assert.Equal(idContent, sqlParametersDictionary["IdCampaign"]);
        Assert.Equal(expectedItemsCount, sqlParametersDictionary.Count);

        var expectedParameters = expectedLinkParameterNames.Select((x, i) => new
        {
            name = x,
            value = links[i]
        });
        Assert.All(expectedParameters, pair =>
        {
            Assert.Contains(pair.name, sqlParametersDictionary.Keys);
            Assert.Equal(pair.value, sqlParametersDictionary[pair.name]);
        });
    }
}
