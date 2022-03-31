using Dapper;
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class DapperFieldsRepository : IFieldsRepository
{
    private readonly IDbContext _dbContext;
    public DapperFieldsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Field>> GetActiveBasicFields()
        => (await _dbContext.ExecuteAsync(new QueryActiveBasicFieldsDbQuery()))
            .Select(x => new Field(
                id: x.IdField,
                name: x.Name,
                isBasic: x.IsBasicField));

    public async Task<IEnumerable<Field>> GetCustomFields(string accountName)
        => (await _dbContext.ExecuteAsync(new QueryCustomFieldsDbQueryByAccountNameDbQuery(accountName)))
            .Select(x => new Field(
                id: x.IdField,
                name: x.Name,
                isBasic: x.IsBasicField));
}
