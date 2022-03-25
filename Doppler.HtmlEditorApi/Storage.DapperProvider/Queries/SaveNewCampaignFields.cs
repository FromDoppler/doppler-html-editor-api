
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

/// <summary>
/// It keeps existing DB entries and only adds new ones without deleting anything.
/// </summary>
public record SaveNewCampaignFields(
    int IdContent,
    IEnumerable<int> FieldIds
) : IExecutableDbQuery
{
    const string BASE_QUERY = @"
    DECLARE @T TABLE (IdField INT)
    INSERT INTO @T (IdField) VALUES {{FieldIds}}

    INSERT INTO ContentXField (IdContent, IdField)
    SELECT @IdContent, t.IdField
    From @T t
    LEFT JOIN dbo.ContentXFIeld CxF ON CxF.IdField = t.IdField AND CxF.IdContent = @IdContent
    WHERE CxF.IdContent IS NULL";

    public string GenerateSqlQuery()
    {
        var serializedFieldsId = string.Join(",", FieldIds.Select(x => $"({x})"));
        return BASE_QUERY.Replace("{{FieldIds}}", serializedFieldsId);
    }

    public object GenerateSqlParameters()
        => new { IdContent };
}
