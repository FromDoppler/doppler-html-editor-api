using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public class SaveNewCampaignFieldsTest
{
    [Theory]
    [InlineData(1231, new int[0], "")]
    [InlineData(1232, new[] { 456 }, "(456)")]
    [InlineData(1233, new[] { 123, 456, 234 }, "(123),(456),(234)")]
    [InlineData(1234, new[] { 123, 456, 234, 123 }, "(123),(456),(234),(123)")]
    public void GenerateSqlQuery_should_insert_the_ids_in_sql_query(int idContent, int[] fieldIds, string expectedFieldIdValues)
    {
        // Arrange
        var dbQuery = new SaveNewCampaignFields(
            IdContent: idContent,
            FieldIds: fieldIds
        );
        var expectedQuery = $@"
    DECLARE @T TABLE (IdField INT)
    INSERT INTO @T (IdField) VALUES {expectedFieldIdValues}

    INSERT INTO ContentXField (IdContent, IdField)
    SELECT @IdContent, t.IdField
    From @T t
    LEFT JOIN dbo.ContentXFIeld CxF ON CxF.IdField = t.IdField AND CxF.IdContent = @IdContent
    WHERE CxF.IdContent IS NULL";

        // Act
        var sqlQuery = dbQuery.GenerateSqlQuery();

        // Assert
        Assert.Equal(expectedQuery, sqlQuery);
    }


    [Theory]
    [InlineData(1231, new int[0], "")]
    [InlineData(1232, new[] { 456 }, "(456)")]
    [InlineData(1233, new[] { 123, 456, 234 }, "(123),(456),(234)")]
    [InlineData(1234, new[] { 123, 456, 234, 123 }, "(123),(456),(234),(123)")]
    public void GenerateSqlParameters_should_generate_an_object_with_only_IdContent(int idContent, int[] fieldIds, string expectedFieldIdValues)
    {
        // Arrange
        var dbQuery = new SaveNewCampaignFields(
            IdContent: idContent,
            FieldIds: fieldIds
        );

        // Act
        var sqlParameters = dbQuery.GenerateSqlParameters();

        // Assert
        Assert.Equal($"{{ IdContent = {idContent} }}", sqlParameters.ToString());
    }
}
