using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.DataAccess;

public interface IDbContext
{
    Task<TResult> ExecuteAsync<TResult>(ISingleItemDbQuery<TResult> query);
    Task<IEnumerable<TResult>> ExecuteAsync<TResult>(ICollectionDbQuery<TResult> query);
    Task<int> ExecuteAsync(IExecutableDbQuery query);
}
