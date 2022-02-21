using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class ContentRow
{
    public int IdCampaign { get; init; }
    public int? EditorType { get; init; }
    public string Content { get; init; }
    public string Meta { get; init; }
}
