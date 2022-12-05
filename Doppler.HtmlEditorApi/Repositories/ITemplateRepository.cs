using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Repositories;

public interface ITemplateRepository
{
    Task<TemplateModel> GetOwnOrPublicTemplate(string accountName, int templateId);
    Task UpdateTemplate(TemplateModel templateModel);
    Task<int> CreatePrivateTemplate(string accountName, TemplateModel templateModel);
}
