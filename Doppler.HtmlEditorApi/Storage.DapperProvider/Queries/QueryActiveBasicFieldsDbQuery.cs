using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class QueryActiveBasicFieldsDbQuery : DbQuery<IEnumerable<DbField>>
{
    public QueryActiveBasicFieldsDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
SELECT
    f.IdField,
    f.Name,
    f.IsBasicField
FROM [Field] f
WHERE
    f.Active = 1
    AND f.IsBasicField = 1";

    public override Task<IEnumerable<DbField>> ExecuteAsync()
        => DbContext.QueryAsync<DbField>(SqlQuery);
}
