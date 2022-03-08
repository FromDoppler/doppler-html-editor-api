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

    public Task<IEnumerable<Field>> GetActiveBasicFields()
        => Task.FromResult(new[]
        {
            new Field(319, "FIRST_NAME", true),
            new Field(320, "LAST_NAME", true),
            new Field(321, "EMAIL", true),
            new Field(322, "GENDER", true),
            new Field(323, "BIRTHDAY", true),
            new Field(324, "COUNTRY", true),
            new Field(325, "CONSENT", true),
            new Field(326, "ORIGIN", true),
            new Field(327, "SCORE", true),
            new Field(106667, "GDPR", true),
        }.AsEnumerable());

    public Task<IEnumerable<Field>> GetCustomFields(string accountname)
        => Task.FromResult(new Field[0].AsEnumerable());
}
