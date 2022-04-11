using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.HtmlEditorApi.DataAccess.DapperProvider;

public class DapperWrapperDbContext : IDbContext, IDisposable
{
    private readonly Lazy<IDbConnection> _lazyDbConnection;
    private bool _disposedValue;

    public DapperWrapperDbContext(IDatabaseConnectionFactory databaseConnectionFactory)
    {
        _lazyDbConnection = new Lazy<IDbConnection>(() => databaseConnectionFactory.GetConnection());
    }

    public Task<TResult> ExecuteAsync<TResult>(ISingleItemDbQuery<TResult> query)
        => _lazyDbConnection.Value.QuerySingleOrDefaultAsync<TResult>(
            query.GenerateSqlQuery(),
            query.GenerateSqlParameters());

    public Task<IEnumerable<TResult>> ExecuteAsync<TResult>(ICollectionDbQuery<TResult> query)
        => _lazyDbConnection.Value.QueryAsync<TResult>(
            query.GenerateSqlQuery(),
            query.GenerateSqlParameters());

    public Task<int> ExecuteAsync(IExecutableDbQuery query)
        => _lazyDbConnection.Value.ExecuteAsync(
            query.GenerateSqlQuery(),
            query.GenerateSqlParameters());

    public void Dispose()
    {
        if (!_disposedValue && _lazyDbConnection.IsValueCreated)
        {
            _disposedValue = true;
            _lazyDbConnection.Value.Dispose();
        }
    }
}
