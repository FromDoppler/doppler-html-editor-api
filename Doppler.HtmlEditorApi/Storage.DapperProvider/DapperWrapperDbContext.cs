using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class DapperWrapperDbContext : IDbContext, IDisposable
{
    private readonly Lazy<IDbConnection> _lazyDbConnection;
    private bool disposedValue;

    public DapperWrapperDbContext(IDatabaseConnectionFactory databaseConnectionFactory)
    {
        _lazyDbConnection = new Lazy<IDbConnection>(() => databaseConnectionFactory.GetConnection());
    }

    public Task<TResult> QueryFirstOrDefaultAsync<TResult>(string query, object param)
        => _lazyDbConnection.Value.QueryFirstOrDefaultAsync<TResult>(query, param);

    public Task<int> ExecuteAsync(string query, object param)
        => _lazyDbConnection.Value.ExecuteAsync(query, param);

    public void Dispose()
    {
        if (!disposedValue && _lazyDbConnection.IsValueCreated)
        {
            disposedValue = true;
            _lazyDbConnection.Value.Dispose();
        }
    }
}
