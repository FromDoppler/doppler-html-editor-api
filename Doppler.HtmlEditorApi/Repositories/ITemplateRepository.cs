using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Repositories;

public interface ITemplateRepository
{
    Task<TemplateModel> GetTemplate(string accountName, int templateId);
}
