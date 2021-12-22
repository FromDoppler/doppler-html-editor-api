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
    public class TemplatesController : Controller
    {
        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/templates/{templateId}")]
        public ActionResult<TemplateModel> GetTemplate(string accountName, int templateId)
        {
            TemplateModel template = new()
            {
                name = "Template Name",
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

            return template;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public IActionResult SaveTemplate(string accountName, int templateId, TemplateModel templateModel)
        {
            return new OkObjectResult($"El template '{templateId}' se guardó exitosamente.");
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPost("/accounts/{accountName}/templates")]
        public IActionResult CreateTemplate(string accountName, TemplateModel templateModel)
        {
            return new OkObjectResult($"El template se creó exitosamente en la cuenta '{accountName}'");
        }

        [Authorize(Policies.ONLY_SUPERUSER)]
        [HttpPost("/shared/templates/{templateId}")]
        public ActionResult<TemplateModel> GetSharedTemplate(int templateId)
        {
            TemplateModel template = new()
            {
                name = "Shared Template name",
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

            return template;
        }
    }
}
