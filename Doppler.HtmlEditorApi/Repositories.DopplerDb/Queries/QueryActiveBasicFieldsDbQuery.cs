using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

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
