using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.Storage;


namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class TemplatesController
    {
        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/templates/{templateId}")]
        public Task<ActionResult<Template>> GetTemplate(string accountName, int templateId)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public Task<IActionResult> SaveTemplate(string accountName, int templateId, Template templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPost("/accounts/{accountName}/templates")]
        public Task<IActionResult> CreateTemplate(string accountName, Template templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost("/shared/templates/{templateId}")]
        public Task<ActionResult<Template>> GetSharedTemplate(int templateId)
        {
            throw new NotImplementedException();
        }
    }
}
