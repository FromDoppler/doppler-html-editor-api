using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public interface IDbContext
{
    Task<TResult> QueryFirstOrDefaultAsync<TResult>(string query, object param);
    Task<IEnumerable<TResult>> QueryAsync<TResult>(string query);
    Task<int> ExecuteAsync(string query, object param);
}
