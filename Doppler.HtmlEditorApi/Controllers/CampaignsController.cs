using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Doppler.HtmlEditorApi.Model;
using Doppler.HtmlEditorApi.Infrastructure;
using System.Text.Json;

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
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<ActionResult<CampaignContent>> GetCampaign(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            var contentRow = await _repository.GetCampaignModel(accountName, campaignId);

            if (contentRow == null)
            {
                return new NotFoundObjectResult("Campaign not found");
            }

            using var doc = JsonDocument.Parse(contentRow.Meta);
            var result = new CampaignContent(
                meta: doc.RootElement.Clone(),
                htmlContent: contentRow.Content);

            return result;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/thumbnail")]
        public Task<ActionResult> GetCampaignThumbnail(string accountName, int campaignId)
        {
            var uriCampaignThumbnail = "https://via.placeholder.com/200x200";
            return Task.FromResult<ActionResult>(new RedirectResult(uriCampaignThumbnail));
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<IActionResult> SaveCampaign(string accountName, int campaignId, CampaignContent campaignContent)
        {
            var contentRow = new ContentRow()
            {
                Content = campaignContent.htmlContent,
                Meta = campaignContent.meta.ToString(),
                EditorType = 5,
                IdCampaign = campaignId
            };

            await _repository.SaveCampaignContent(accountName, campaignId, contentRow);
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }
    }
}
