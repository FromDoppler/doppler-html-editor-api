using System.Data;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> GetConnection();
    }
}
