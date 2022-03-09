using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class QueryCustomFieldsDbQueryByAccountNameDbQuery : DbQuery<ByAccountNameParameters, IEnumerable<DbField>>
{
    public QueryCustomFieldsDbQueryByAccountNameDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
SELECT
    f.IdField,
    f.Name,
    f.IsBasicField
FROM [Field] f
JOIN [User] u ON u.IdUser = f.IdUser
WHERE
    u.Email = @accountName
    AND f.IsBasicField = 0";

    public override Task<IEnumerable<DbField>> ExecuteAsync(ByAccountNameParameters parameters)
        => DbContext.QueryAsync<DbField>(SqlQuery, parameters);
}
