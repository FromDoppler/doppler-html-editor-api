
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class SaveFieldsId : DbQuery<SaveFieldsId.Parameters, int>
{
    public SaveFieldsId(IDbContext dbContext) : base(dbContext) { }

    /* TODO: Query suggestion --> @"
    DECLARE @T TABLE (IdField INT)
    INSERT INTO @T (IdField) VALUES @FieldsId

    INSERT INTO ContentXField (IdContent, IdField)
    SELECT @IdContent, t.IdField
    From @T t
    LEFT JOIN dbo.ContentXFIeld CxF ON CxF.IdField = t.IdField AND CxF.IdContent = @IdContent
    WHERE CxF.IdContent IS NULL"
    */
    protected override string SqlQuery => @"
    DECLARE @T TABLE (IdField INT)
    INSERT INTO @T (IdField) VALUES {{FieldIds}}

    INSERT INTO ContentXField (IdContent, IdField)
    SELECT @IdContent, t.IdField
    From @T t
    LEFT JOIN dbo.ContentXFIeld CxF ON CxF.IdField = t.IdField AND CxF.IdContent = @IdContent
    WHERE CxF.IdContent IS NULL";

    public override Task<int> ExecuteAsync(Parameters parameters)
    {
        var serializedFieldsId = string.Join(",", parameters.FieldIds.Select(x => $"({x})"));
        var query = SqlQuery.Replace("{{FieldIds}}", serializedFieldsId);

        return DbContext.ExecuteAsync(query, parameters);
    }

    public record Parameters(int IdContent, IEnumerable<int> FieldIds);

}
