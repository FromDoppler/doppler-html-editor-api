using System;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Doppler.HtmlEditorApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        [HttpGet("/accounts/{accountName}/templates/{templateId}", Name = "GetTemplate")]
        public async Task<Results<NotFound<ProblemDetails>, ProblemHttpResult, Ok<Template>>> GetTemplate(string accountName, int templateId)
        {
            // TODO: Considere refactoring accountName validation
            var templateModel = await _templateRepository.GetOwnOrPublicTemplate(accountName, templateId);

            if (templateModel == null)
            {
                return TypedResults.NotFound(new ProblemDetails()
                {
                    Detail = "Template not found or belongs to a different account"
                });
            }

            if (templateModel.IsPublic)
            {
                return TypedResults.NotFound(new ProblemDetails()
                {
                    Detail = $"It is a public template, use /shared/templates/{templateId}"
                });
            }

            if (templateModel.Content is UnlayerTemplateContentData unlayerContent)
            {
                return TypedResults.Ok(new Template(
                    type: ContentType.unlayer,
                    templateName: templateModel.Name,
                    isPublic: templateModel.IsPublic,
                    previewImage: templateModel.PreviewImage,
                    htmlContent: unlayerContent.HtmlComplete,
                    meta: Utils.ParseAsJsonElement(unlayerContent.Meta)));
            }

            return TypedResults.Problem(
                detail: $"Unsupported template content type {templateModel.Content.GetType()}",
                statusCode: StatusCodes.Status501NotImplemented,
                title: "Not Implemented");
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPut("/accounts/{accountName}/templates/{templateId}")]
        public async Task<Results<NotFound<ProblemDetails>, Ok<string>>> SaveTemplate(string accountName, int templateId, Template template)
        {
            // TODO: Considere refactoring accountName validation
            var currentTemplate = await _templateRepository.GetOwnOrPublicTemplate(accountName, templateId);

            if (currentTemplate == null || currentTemplate.IsPublic)
            {
                return TypedResults.NotFound(new ProblemDetails()
                {
                    Detail = "Template not found, belongs to a different account, or it is a public template."
                });
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

            return TypedResults.Ok($"El template'{templateId}' del usuario '{accountName}' se guardó exitosamente.");
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPost("/accounts/{accountName}/templates/from-template/{baseTemplateId}")]
        public async Task<Results<NotFound<ProblemDetails>, CreatedAtRoute<ResourceCreated>>> CreateTemplateFromTemplate(string accountName, int baseTemplateId)
        {
            var templateModel = await _templateRepository.GetOwnOrPublicTemplate(accountName, baseTemplateId);
            if (templateModel == null)
            {
                return TypedResults.NotFound(new ProblemDetails()
                {
                    Detail = "The template not exists or Inactive"
                });
            }

            if (templateModel.Content is not UnlayerTemplateContentData)
            {
                throw new NotImplementedException($"Unsupported template content type {templateModel.Content.GetType()}");
            }

            // To avoid ambiguities
            var newTemplate = templateModel with
            {
                TemplateId = 0,
                IsPublic = false
            };

            var templateId = await _templateRepository.CreatePrivateTemplate(accountName, newTemplate);

            return TypedResults.CreatedAtRoute(new ResourceCreated(templateId), "GetTemplate", new { accountName, templateId });
        }


        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPost("/accounts/{accountName}/templates")]
        public Task<IResult> CreateTemplate(string accountName, Template templateModel)
        {
            throw new NotImplementedException();
        }

        [Authorize(Policies.OnlySuperUser)]
        [HttpPost("/shared/templates/{templateId}")]
        public Task<Results<NotFound<string>, Ok<Template>>> GetSharedTemplate(int templateId)
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
