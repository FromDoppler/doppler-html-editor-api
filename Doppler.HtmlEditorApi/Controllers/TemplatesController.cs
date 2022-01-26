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
        public Task<ActionResult<TemplateModel>> GetTemplate(string accountName, int templateId)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public Task<IActionResult> SaveTemplate(string accountName, int templateId, TemplateModel templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPost("/accounts/{accountName}/templates")]
        public Task<IActionResult> CreateTemplate(string accountName, TemplateModel templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost("/shared/templates/{templateId}")]
        public Task<ActionResult<TemplateModel>> GetSharedTemplate(int templateId)
        {
            throw new NotImplementedException();
        }
    }
}
