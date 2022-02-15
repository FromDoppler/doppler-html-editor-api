using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface IRepository
{
    Task<ContentData> GetCampaignModel(string accountName, int campaignId);
    Task SaveCampaignContent(string accountName, ContentData contentRow);
}
