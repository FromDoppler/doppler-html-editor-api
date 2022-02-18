namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class FirstOrDefaultContentWithCampaignStatusDbQuery
{
    public class Result
    {
        public int IdCampaign { get; init; }
        public bool CampaignHasContent { get; init; }
        public bool CampaignBelongsUser { get; init; }
        public bool CampaignExists { get; init; }
        public int? EditorType { get; init; }
        public string Content { get; init; }
        public string Meta { get; init; }
    }

    public class Parameters
    {
        public int IdCampaign { get; init; }
        public string AccountName { get; init; }
    }
}
