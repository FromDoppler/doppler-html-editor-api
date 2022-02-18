using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public static class LoadCampaignQuery
{
    public class Result
    {
        public int IdCampaign { get; set; }
        public bool CampaignHasContent { get; set; }
        public bool CampaignBelongsUser { get; set; }
        public bool CampaignExists { get; set; }
        public int? EditorType { get; set; }
        public string Content { get; set; }
        public string Meta { get; set; }
    }

    public class Parameters
    {
        public int IdCampaign { get; set; }
        public string AccountName { get; set; }
    }
}
