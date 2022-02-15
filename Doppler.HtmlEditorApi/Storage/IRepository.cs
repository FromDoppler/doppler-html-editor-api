using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface IRepository
{
    Task<ContentRow> GetCampaignModel(string accountName, int campaignId);
    Task SaveCampaignContent(string accountName, ContentRow contentRow);
}
