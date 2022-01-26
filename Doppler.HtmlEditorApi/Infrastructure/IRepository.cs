using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public interface IRepository
    {
        Task<ContentRow> GetCampaignModel(string accountName, int campaignId);
        Task SaveCampaignContent(string accountName, int campaignId, ContentRow contentRow);
    }
}
