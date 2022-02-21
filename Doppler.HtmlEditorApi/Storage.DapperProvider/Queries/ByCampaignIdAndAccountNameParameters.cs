using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class ByCampaignIdAndAccountNameParameters
{
    public int IdCampaign { get; init; }
    public string AccountName { get; init; }
}
