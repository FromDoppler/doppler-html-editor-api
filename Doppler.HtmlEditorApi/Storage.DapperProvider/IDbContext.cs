using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public interface IDbContext
{
    Task<TResult> ExecuteAsync<TResult>(ISingleItemDbQuery<TResult> query);
    Task<IEnumerable<TResult>> ExecuteAsync<TResult>(ICollectionDbQuery<TResult> query);
    Task<int> ExecuteAsync(IExecutableDbQuery query);
}

public interface IDbQuery
{
    string GenerateSqlQuery();
    object GenerateSqlParameters() => this;
}

public interface IExecutableDbQuery : IDbQuery { }

public interface ICollectionDbQuery<TResult> : IDbQuery { }

public interface ISingleItemDbQuery<TResult> : IDbQuery { }
