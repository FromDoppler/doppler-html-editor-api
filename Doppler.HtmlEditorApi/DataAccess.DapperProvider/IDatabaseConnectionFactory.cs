using System.Data;

namespace Doppler.HtmlEditorApi.DataAccess.DapperProvider;

public interface IDatabaseConnectionFactory
{
    IDbConnection GetConnection();
}
