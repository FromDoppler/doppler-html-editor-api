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

            ActionResult<CampaignContent> result = contentRow switch
            {
                null => new NotFoundObjectResult("Campaign not found or belongs to a different account"),
                var r when r.HasUnlayerEditorType => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(r.Meta),
                    htmlContent: r.Content),
                _ => throw new NotImplementedException($"Unsupported campaign content type {contentRow.EditorType}")
            };

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
            var contentRow = campaignContent.type switch
            {
                ContentType.unlayer => ContentRow.CreateUnlayerContentRow(
                    content: campaignContent.htmlContent,
                    meta: campaignContent.meta.ToString(),
                    idCampaign: campaignId),
                _ => throw new NotImplementedException($"Unsupported campaign content type {campaignContent.type:G}")
            };

            await _repository.SaveCampaignContent(accountName, campaignId, contentRow);
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }
    }
}
