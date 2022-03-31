using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.DataAccess.DapperProvider;

public interface IDatabaseConnectionFactory
{
    IDbConnection GetConnection();
}
