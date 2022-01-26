using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public interface IRepository
    {
        Task<ContentModel> GetCampaignModel(string accountName, int campaignId);
        Task SaveCampaignContent(string accountName, int campaignId, CampaignContentRequest data);
    }
}
