using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Doppler.HtmlEditorApi.Model;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class CampaignsController
    {
        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/design")]
        public ActionResult<ContentModel> GetCampaign(string accountName, int campaignId)
        {
            ContentModel campaign = new()
            {
                counters = new()
                {
                    u_row = 1,
                    u_column = 2,
                    u_content_text = 1,
                    u_content_heading = 1,
                    u_content_menu = 1
                },
                body = new()
                {
                    rows = Array.Empty<string>(),
                    values = new()
                    {
                        textColor = "#000000",
                        backgroundColor = "#e7e7e7",
                        backgroundImage = new()
                        {
                            url = "",
                            fullWidth = true,
                            repeat = false,
                            center = true,
                            cover = false
                        },
                        contentWidth = "500px",
                        contentAlign = "center",
                        fontFamily = new()
                        {
                            label = "Arial",
                            value = "arial,helvetica,sans-serif"
                        },
                        preheaderText = "",
                        linkStyle = new()
                        {
                            body = true,
                            linkColor = "#0000ee",
                            linkHoverColor = "#0000ee",
                            linkUnderline = true,
                            linkHoverUnderline = true
                        },
                        _meta = new()
                        {
                            htmlID = "u_body",
                            htmlClassNames = "u_body"
                        }
                    }
                },
                schemaVersion = 6
            };

            return campaign;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/thumbnail")]
        public ActionResult GetCampaignThumbnail(string accountName, int campaignId)
        {
            var uriCampaignThumbnail = "https://via.placeholder.com/200x200";
            return new RedirectResult(uriCampaignThumbnail);
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/campaigns/{campaignId}/content/")]
        public IActionResult SaveCampaign(string accountName, int campaignId, ContentModel campaignModel)
        {
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }
    }
}
