using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DopplerFieldsRepository : IFieldsRepository
{
    private readonly IDbContext _dbContext;
    public DopplerFieldsRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Field>> GetActiveBasicFields()
        => (await _dbContext.ExecuteAsync(new QueryActiveBasicFieldsDbQuery()))
            .Select(x => new Field(
                Id: x.IdField,
                Name: x.Name,
                IsBasic: x.IsBasicField));

    public async Task<IEnumerable<Field>> GetCustomFields(string accountName)
        => (await _dbContext.ExecuteAsync(new QueryCustomFieldsDbQueryByAccountNameDbQuery(accountName)))
            .Select(x => new Field(
                Id: x.IdField,
                Name: x.Name,
                IsBasic: x.IsBasicField));
}
