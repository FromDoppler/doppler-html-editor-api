using Xunit;

namespace Doppler.HtmlEditorApi.Domain;

public class DopplerFieldsProcessorTest
{
    private static DopplerFieldsProcessor CreateSut()
        => new DopplerFieldsProcessor(
            fields: new[]
            {
                new Field(1, "FIELD_1", true),
                new Field(2, "FIELD_2", true),
                new Field(345, "custom1", false),
                new Field(456, "custom2", false),
            },
            aliasesByCanonical: new[]
            {
                new FieldAliasesDef()
                {
                    canonicalName = "FIELD_1",
                    aliases = new[] { "FIELD_A", "FIELD 1" }
                },
                new FieldAliasesDef()
                {
                    canonicalName = "FIELD_2",
                    aliases = new[] { "FIELD_B", "FIELD 2" }
                },
            }
        );

    [Theory]
    [InlineData(1, "FIELD_1")]
    [InlineData(2, "FIELD_2")]
    [InlineData(345, "custom1")]
    [InlineData(456, "custom2")]
    [InlineData(5, null)]
    [InlineData(1234, null)]
    public void GetFieldNameOrNull_should_return_names_of_known_fields_and_null_for_unknown_ones(int fieldId, string expectedFieldName)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.GetFieldNameOrNull(fieldId);

        // Assert
        Assert.Equal(expectedFieldName, result);
    }

}
