using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.DataAccess;

public interface IDbQuery
{
    string GenerateSqlQuery();
    object GenerateSqlParameters() => this;
}

public interface IExecutableDbQuery : IDbQuery { }

public interface ICollectionDbQuery<TResult> : IDbQuery { }

public interface ISingleItemDbQuery<TResult> : IDbQuery { }
