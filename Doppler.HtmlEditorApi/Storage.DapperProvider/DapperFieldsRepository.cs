using Dapper;
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class DapperFieldsRepository : IFieldsRepository
{
    private readonly IDbContext _dbContext;
    public DapperFieldsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Field>> GetActiveBasicFields()
        => (await new QueryActiveBasicFieldsDbQuery(_dbContext).ExecuteAsync())
            .Select(x => new Field(
                id: x.IdField,
                name: x.Name,
                isBasic: x.IsBasicField));

    public Task<IEnumerable<Field>> GetCustomFields(string accountname)
        => Task.FromResult(new Field[0].AsEnumerable());
}
