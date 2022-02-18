using System;
using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public interface IDbContext
{
    Task<dynamic> QueryFirstOrDefaultAsync(string query, object param);
    Task<int> ExecuteAsync(string query, object param);
}
