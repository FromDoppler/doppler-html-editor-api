using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public interface IDatabaseConnectionFactory
{
    IDbConnection GetConnection();
}
