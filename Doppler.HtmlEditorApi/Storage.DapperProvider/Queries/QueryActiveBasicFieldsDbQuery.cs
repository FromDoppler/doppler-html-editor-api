using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class QueryActiveBasicFieldsDbQuery : DbQuery<IEnumerable<QueryActiveBasicFieldsDbQuery.Result>>
{
    public QueryActiveBasicFieldsDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
SELECT
    f.IdField,
    f.Name
FROM [Field] f
WHERE
    f.Active = 1
    AND IsBasicField = 1";

    public override Task<IEnumerable<Result>> ExecuteAsync()
        => DbContext.QueryAsync<Result>(SqlQuery);

    public class Result
    {
        public int IdField { get; init; }
        public string Name { get; init; }
    }
}
