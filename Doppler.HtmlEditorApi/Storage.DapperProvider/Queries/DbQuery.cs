using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public abstract class DbQuery<TParametes, TResult>
{
    protected IDbContext DbContext { get; }
    public DbQuery(IDbContext dbContext) => DbContext = dbContext;
    abstract protected string SqlQuery { get; }
    public abstract Task<TResult> ExecuteAsync(TParametes parameters);
}
