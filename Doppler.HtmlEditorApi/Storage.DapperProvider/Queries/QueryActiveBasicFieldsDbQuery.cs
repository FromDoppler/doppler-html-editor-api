using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public record QueryActiveBasicFieldsDbQuery() : ICollectionDbQuery<DbField>
{
    public string GenerateSqlQuery() => @"
SELECT
    f.IdField,
    f.Name,
    f.IsBasicField
FROM [Field] f
WHERE
    f.Active = 1
    AND f.IsBasicField = 1";
}
