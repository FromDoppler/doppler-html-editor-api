using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

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
