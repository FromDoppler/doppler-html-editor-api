using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record QueryCustomFieldsDbQueryByAccountNameDbQuery(string AccountName) : ICollectionDbQuery<DbField>
{
    public string GenerateSqlQuery() => @"
SELECT
    f.IdField,
    f.Name,
    f.IsBasicField
FROM [Field] f
JOIN [User] u ON u.IdUser = f.IdUser
WHERE
    u.Email = @accountName
    AND f.IsBasicField = 0";
}
