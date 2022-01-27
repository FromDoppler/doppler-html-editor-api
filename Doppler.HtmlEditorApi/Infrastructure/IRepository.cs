using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public interface IRepository
    {
        Task<CampaignContent> GetCampaignModel(string accountName, int campaignId);
        Task SaveCampaignContent(string accountName, int campaignId, CampaignContent data);
    }
}
