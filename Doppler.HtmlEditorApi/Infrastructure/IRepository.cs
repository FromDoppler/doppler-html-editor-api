using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public interface IRepository
    {
        Task<ContentModel> GetCampaignModel(string accountName, int campaignId);
        Task<TemplateModel> GetTemplateModel(string accountName, int templateId);
        Task<TemplateModel> GetSharedTemplateModel(int templateId);
        Task SaveCampaignContent(string accountName, int campaignId, CampaignContentRequest data);
        Task SaveTemplateContent(string accountName, int templateId, TemplateModel templateModel);
        Task CreateTemplate(string accountName, TemplateModel templateModel);
    }
}
