using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.DataAccess.DapperProvider;

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(IOptions<DatabaseSettings> dopplerDatabaseSettings)
    {
        _connectionString = dopplerDatabaseSettings.Value.GetSqlConnectionString();
    }

    public IDbConnection GetConnection()
        => new SqlConnection(_connectionString);
}
