using System;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Doppler.HtmlEditorApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class TemplatesController
    {
        private readonly ITemplateRepository _templateRepository;

        public TemplatesController(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpGet("/accounts/{accountName}/templates/{templateId}")]
        public async Task<ActionResult<Template>> GetTemplate(string accountName, int templateId)
        {
            // TODO: Considere refactoring accountName validation
            var templateModel = await _templateRepository.GetOwnOrPublicTemplate(accountName, templateId);

            if (templateModel == null)
            {
                return new NotFoundObjectResult("Template not found or belongs to a different account");
            }

            ActionResult<Template> result = templateModel.Content switch
            {
                UnlayerTemplateContentData unlayerContent => new Template(
                    templateName: templateModel.Name,
                    isPublic: templateModel.IsPublic,
                    previewImage: templateModel.PreviewImage,
                    htmlContent: unlayerContent.HtmlComplete,
                    meta: Utils.ParseAsJsonElement(unlayerContent.Meta)),
                _ => throw new NotImplementedException($"Unsupported template content type {templateModel.Content.GetType()}")
            };

            return result;
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
