using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Doppler.HtmlEditorApi.Model;
using Doppler.HtmlEditorApi.Infrastructure;


namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class TemplatesController
    {
        private readonly IRepository _repository;

        public TemplatesController(IRepository Repository)
        {
            _repository = Repository;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/templates/{templateId}")]
        public async Task<ActionResult<TemplateModel>> GetTemplate(string accountName, int templateId)
        {
            var template = await _repository.GetTemplateModel(accountName, templateId);
            return template;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public async Task<IActionResult> SaveTemplate(string accountName, int templateId, TemplateModel templateModel)
        {
            await _repository.SaveTemplateContent(accountName, templateId, templateModel);
            return new OkObjectResult($"El template '{templateId}' se guardó exitosamente.");
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPost("/accounts/{accountName}/templates")]
        public async Task<IActionResult> CreateTemplate(string accountName, TemplateModel templateModel)
        {
            await _repository.CreateTemplate(accountName, templateModel);
            return new OkObjectResult($"El template se creó exitosamente en la cuenta '{accountName}'");
        }

        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost("/shared/templates/{templateId}")]
        public async Task<ActionResult<TemplateModel>> GetSharedTemplate(int templateId)
        {
            var sharedTemplate = await _repository.GetSharedTemplateModel(templateId);
            return sharedTemplate;
        }
    }
}
