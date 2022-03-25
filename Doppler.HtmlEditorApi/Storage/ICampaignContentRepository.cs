using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface ICampaignContentRepository
{
    Task<ContentData> GetCampaignModel(string accountName, int campaignId);
    Task SaveCampaignContent(string accountName, ContentData contentRow);
}
