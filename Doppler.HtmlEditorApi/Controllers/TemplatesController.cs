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

            if (templateModel.IsPublic)
            {
                return new NotFoundObjectResult($"It is a public template, use /shared/templates/{templateId}");
            }

            ActionResult<Template> result = templateModel.Content switch
            {
                UnlayerTemplateContentData unlayerContent => new Template(
                    type: ContentType.unlayer,
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
        public async Task<IActionResult> SaveTemplate(string accountName, int templateId, Template template)
        {
            // TODO: Considere refactoring accountName validation
            var currentTemplate = await _templateRepository.GetOwnOrPublicTemplate(accountName, templateId);

            if (currentTemplate == null || currentTemplate.IsPublic)
            {
                return new NotFoundObjectResult("Template not found, belongs to a different account, or it is a public template.");
            }

            var htmlDocument = ExtractHtmlDomFromTemplateContent(template.htmlContent);

            var templateModel = new TemplateModel(
                TemplateId: templateId,
                IsPublic: false,
                PreviewImage: template.previewImage,
                Name: template.templateName,
                Content: new UnlayerTemplateContentData(
                    HtmlComplete: htmlDocument.GetCompleteContent(),
                    Meta: template.meta.ToString()));

            await _templateRepository.UpdateTemplate(templateModel);

            return new OkObjectResult($"El template'{templateId}' del usuario '{accountName}' se guardó exitosamente.");
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

        private static DopplerHtmlDocument ExtractHtmlDomFromTemplateContent(string htmlContent)
        {
            var htmlDocument = new DopplerHtmlDocument(htmlContent);
            htmlDocument.RemoveHarmfulTags();
            htmlDocument.RemoveEventAttributes();
            htmlDocument.SanitizeTrackableLinks();
            return htmlDocument;
        }
    }
}
