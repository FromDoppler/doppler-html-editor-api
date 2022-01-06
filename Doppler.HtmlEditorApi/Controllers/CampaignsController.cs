using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Doppler.HtmlEditorApi.Model;
using Doppler.HtmlEditorApi.Infrastructure;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class CampaignsController
    {
        private readonly IRepository _repository;

        public CampaignsController(IRepository Repository)
        {
            _repository = Repository;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/design")]
        public async Task<IActionResult> GetCampaign(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            // TODO: Check own resource
            // TODO: Request for superUsers
            var campaign = await _repository.GetCampaignModel(accountName, campaignId);
            // TODO: Return 404 if campaign is NULL
            return new ContentResult() { Content= campaign, ContentType= "application/json", StatusCode=200};
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/thumbnail")]
        public async Task<ActionResult> GetCampaignThumbnail(string accountName, int campaignId)
        {
            var uriCampaignThumbnail = "https://via.placeholder.com/200x200";
            return new RedirectResult(uriCampaignThumbnail);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/campaigns/{campaignId}/content/")]
        public async Task<IActionResult> SaveCampaign(string accountName, int campaignId, ContentModel campaignModel)
        {
            await _repository.SaveCampaignContent(accountName, campaignId, campaignModel);
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }
    }
}
