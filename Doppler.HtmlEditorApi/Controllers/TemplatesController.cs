using System;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class TemplatesController
    {
        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpGet("/accounts/{accountName}/templates/{templateId}")]
        public Task<ActionResult<Template>> GetTemplate(string accountName, int templateId)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public Task<IActionResult> SaveTemplate(string accountName, int templateId, Template templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPost("/accounts/{accountName}/templates")]
        public Task<IActionResult> CreateTemplate(string accountName, Template templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OnlySuperUser)]
        [HttpPost("/shared/templates/{templateId}")]
        public Task<ActionResult<Template>> GetSharedTemplate(int templateId)
        {
            throw new NotImplementedException();
        }
    }
}
