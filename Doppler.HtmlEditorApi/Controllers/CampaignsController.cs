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
        public async Task<ActionResult<ContentModel>> GetCampaign(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            var campaignModel = await _repository.GetCampaignModel(accountName, campaignId);
            return campaignModel != null ? campaignModel : new NotFoundObjectResult("Campaign not found");
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
        public async Task<IActionResult> SaveCampaign(string accountName, int campaignId, CampaignContentRequest data)
        {
            await _repository.SaveCampaignContent(accountName, campaignId, data);
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }
    }
}
